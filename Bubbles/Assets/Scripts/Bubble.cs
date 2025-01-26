using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Bubble : MonoBehaviour
{
  // Static bubble tracking
  private static readonly List<Bubble> _activeBubbles = new List<Bubble>();
  public static IReadOnlyList<Bubble> ActiveBubbles => _activeBubbles;

  [Header("Bubble Properties")]
  public float Size = 1f;
  public float CoreSizeRatio = 0.6f;
  public bool Invulnerable = false;
  public int Variant;
  public float Hue => (float)Variant / GameRules.Data.VariantCount;
  public bool IsPopped { get; private set; }
  private bool _isAnimating = false; // Guard flag for animations
  public float HoverT => _currentHoverT; // Expose hover transition for renderer

  [Header("Colliders")]
  [SerializeField] private CircleCollider2D _bubbleCollider;
  [SerializeField] private CircleCollider2D _coreCollider;

  [Header("Audio")]
  [SerializeField] private BubbleSoundManager _soundManager;

  [Header("Effects")]
  [SerializeField] private GameObject _popParticlePrefab;

  private bool _isHovered = false;
  private float _currentHoverT = 0f;  // Transition value for hover effect

  public float Radius => Size * 0.5f;
  private float Volume => Mathf.PI * Size * Size;  // 2D "volume" is area
  private Rigidbody2D _rb;

  private void Start()
  {
    _rb = GetComponent<Rigidbody2D>();
    UpdateShape();
    _activeBubbles.Add(this);
  }

  private void OnDestroy()
  {
    _activeBubbles.Remove(this);
  }

  private void Update()
  {
    // Update hover transition
    float targetHoverT = _isHovered ? 1f : 0f;
    _currentHoverT = Mathf.MoveTowards(_currentHoverT, targetHoverT, Time.deltaTime * GameRules.Data.HoverTransitionSpeed);

    // Check if bubble has grown too large
    BubbleVariant variantData = GameRules.BubbleVariantData(Variant);
    if (Size >= variantData.PopAtSize && !Invulnerable && !_isAnimating)
    {
      Pop();
    }
  }

  public void SetHovered(bool hovered)
  {
    if (_isHovered != hovered)
    {
      _isHovered = hovered;
    }
  }

  public void UpdateShape()
  {
    transform.localScale = Vector3.one * Size;

    if (_coreCollider != null)
    {
      _coreCollider.radius = CoreSizeRatio * 0.5f;
    }
  }

  private void OnTriggerStay2D(Collider2D other)
  {
    if (_isAnimating) return; // Skip collision checks if animating

    BubbleVariant variantData = GameRules.BubbleVariantData(Variant);
    Rigidbody2D rb = GetComponent<Rigidbody2D>();
    if (rb == null) return;

    if (other.CompareTag("Bubble"))
    {
      Bubble otherBubble = other.GetComponent<Bubble>();
      if (otherBubble == null || otherBubble._isAnimating) return;

      // Calculate centers distance and overlap
      Vector2 direction = (transform.position - other.transform.position).normalized;
      float distance = Vector2.Distance(transform.position, other.transform.position);
      float combinedRadii = Radius + otherBubble.Radius;
      float overlap = Mathf.Max(0, combinedRadii - distance);
      float overlapRatio = overlap / combinedRadii;

      // Check for matching variants and sufficient overlap
      bool isMatchingVariant = Variant == otherBubble.Variant;

      if (isMatchingVariant)
      {
        if (variantData.PopMatchingVariants && overlapRatio >= variantData.MinOverlapToPop)
        {
          Pop();
          otherBubble.Pop();
          return;
        }
        else if (variantData.MergeMatchingVariants && overlapRatio >= variantData.MinOverlapToMerge)
        {
          MergeWith(otherBubble);
          return;
        }
      }

      // Apply variant-based forces
      VariantForceType forceType = isMatchingVariant ?
        variantData.MatchingVariantForce :
        variantData.NonMatchingVariantForce;

      if (forceType != VariantForceType.None)
      {
        // Calculate base force magnitude
        float forceMagnitude;
        if (forceType == VariantForceType.Attract)
        {
          // Attraction increases with distance up to combined radii
          float distanceRatio = Mathf.Clamp01(distance / combinedRadii);
          forceMagnitude = variantData.AttractionForce * distanceRatio;
          direction = -direction; // Reverse direction for attraction
        }
        else // Repulse
        {
          // Repulsion increases with overlap
          forceMagnitude = variantData.RepulsionForce * overlapRatio;
        }

        // Apply forces to both bubbles' Rigidbodies
        Vector2 force = direction * forceMagnitude;
        Rigidbody2D otherRb = other.GetComponent<Rigidbody2D>();

        if (rb != null) rb.AddForce(force, ForceMode2D.Force);
        if (otherRb != null) otherRb.AddForce(-force, ForceMode2D.Force);
      }
    }
    else if (other.CompareTag("Wall"))
    {
      // Get the closest point on the wall collider to the bubble's center
      Vector2 bubbleCenter = (Vector2)transform.position;
      Vector2 closestPoint = other.ClosestPoint(bubbleCenter);

      // Calculate direction and distance
      Vector2 direction = (bubbleCenter - closestPoint).normalized;
      float distance = Vector2.Distance(bubbleCenter, closestPoint);

      // Calculate overlap and force
      float overlap = Mathf.Max(0, Radius - distance);
      float overlapRatio = overlap / Radius;

      // Apply repulsion force if overlapping
      if (overlap > 0)
      {
        float forceMagnitude = variantData.RepulsionForce * overlapRatio;
        Vector2 force = direction * forceMagnitude;
        rb.AddForce(force, ForceMode2D.Force);
      }
    }
  }

  private void MergeWith(Bubble other)
  {
    if (IsPopped || other.IsPopped) return;
    if (Invulnerable || other.Invulnerable) return;
    if (_isAnimating || other._isAnimating) return;

    // Calculate new bubble properties
    float combinedVolume = Volume + other.Volume;
    float newSize = Mathf.Sqrt(combinedVolume / Mathf.PI);

    // Start merge effect
    _isAnimating = true;
    other._isAnimating = true;
    StartCoroutine(MergeEffect(other, newSize));
  }

  private IEnumerator MergeEffect(Bubble other, float newSize)
  {
    try
    {
      BubbleVariant variantData = GameRules.BubbleVariantData(Variant);

      // Play merge sound immediately
      if (_soundManager != null)
      {
        _soundManager.PlayMergeSound(transform.position);
      }

      // Wait for merge delay
      if (variantData.MergeDelay > 0)
      {
        float startTime = Time.time;
        float initialSize = Size;
        float otherInitialSize = other.Size;

        // Shrink animation
        while (Time.time < startTime + variantData.MergeDelay)
        {
          float progress = (Time.time - startTime) / variantData.MergeDelay;
          float currentShrink = Mathf.Lerp(1f, variantData.MergeSizeShrink, progress);

          Size = initialSize * currentShrink;
          other.Size = otherInitialSize * currentShrink;

          UpdateShape();
          other.UpdateShape();

          yield return null;
        }
      }

      // Calculate weighted center position based on bubble sizes
      Vector2 newPosition = Vector2.Lerp(
        transform.position,
        other.transform.position,
        other.Size / (Size + other.Size)
      );

      // Create the merged bubble
      GameObject bubblePrefab = gameObject;
      GameObject newBubble = Instantiate(bubblePrefab, newPosition, Quaternion.identity);
      newBubble.name = bubblePrefab.name;
      Bubble newBubbleComponent = newBubble.GetComponent<Bubble>();

      // Set properties of new bubble
      newBubbleComponent.Size = newSize;
      newBubbleComponent.Variant = Variant;
      newBubbleComponent.CoreSizeRatio = variantData.CoreSizeRatio;

      // Apply merge effect to nearby bubbles
      ApplyMergeEffect(newPosition, newSize);

      // Destroy the original bubbles
      other.IsPopped = true;
      IsPopped = true;
      Destroy(other.gameObject);
      Destroy(gameObject);
    }
    finally
    {
      _isAnimating = false;
      if (other != null)
      {
        other._isAnimating = false;
      }
    }
  }

  private void ApplyMergeEffect(Vector2 position, float size)
  {
    BubbleVariant variantData = GameRules.BubbleVariantData(Variant);
    float mergeRadius = size * variantData.MergeRadiusRatio;

    // Apply implosion force to nearby bubbles
    Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(position, mergeRadius);
    foreach (Collider2D collider in nearbyColliders)
    {
      if (collider.gameObject != gameObject && collider.CompareTag("Bubble"))
      {
        Bubble targetBubble = collider.GetComponent<Bubble>();
        Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
        if (rb != null && targetBubble != null)
        {
          Vector2 direction = ((Vector2)collider.transform.position - position).normalized;
          float distance = Vector2.Distance(position, collider.transform.position);

          // Calculate overlap between merge radius and target bubble
          float combinedRadius = mergeRadius + targetBubble.Radius;
          float overlap = Mathf.Max(0, combinedRadius - distance);
          float overlapRatio = overlap / combinedRadius;

          float force = variantData.MergeForce * overlapRatio;
          rb.AddForce(direction * force, ForceMode2D.Impulse);
        }
      }
    }
  }

  public float DistanceToPoint(Vector2 point)
  {
    float rawDistance = Vector2.Distance(point, (Vector2)transform.position);
    float adjustedRadius = Radius; // We don't have wave deformation in the C# version
    float radiusPreservation = Mathf.Lerp(1f, 1f / Mathf.Max(adjustedRadius, 0.001f), GameRules.Data.SmallRadiusPreservationFactor);
    return rawDistance / (adjustedRadius * radiusPreservation);
  }

  public static Bubble FindClosestBubbleTo(Vector2 point)
  {
    Bubble closest = null;
    float closestDistance = float.MaxValue;

    foreach (var bubble in _activeBubbles)
    {
      float distance = bubble.DistanceToPoint(point);
      if (distance < closestDistance)
      {
        closestDistance = distance;
        closest = bubble;
      }
    }

    return closest;
  }

  public static bool TryPopAtPoint(Vector2 point)
  {
    Bubble closest = FindClosestBubbleTo(point);
    if (closest != null && closest.DistanceToPoint(point) < 1)  // Check if point is inside bubble
    {
      BubbleVariant variantData = GameRules.BubbleVariantData(closest.Variant);
      if (variantData.PopOnClick)
      {
        return closest.Pop();
      }
    }
    return false;
  }

  public bool Pop()
  {
    if (IsPopped) return false;
    if (Invulnerable) return false;
    if (_isAnimating) return false;

    IsPopped = true;
    _isAnimating = true;
    LevelStats.Instance.BubblesPopped.Increment();
    StartCoroutine(PopEffect());
    return true;
  }

  private IEnumerator PopEffect()
  {
    try
    {
      BubbleVariant variantData = GameRules.BubbleVariantData(Variant);

      // Play pop sound immediately
      if (_soundManager != null)
      {
        _soundManager.PlayPopSound(transform.position);
      }

      // Store initial size
      float initialSize = Size;
      float startTime = Time.time;

      // Growth animation
      if (variantData.PopDelay > 0)
      {
        while (Time.time < startTime + variantData.PopDelay)
        {
          float progress = (Time.time - startTime) / variantData.PopDelay;
          Size = initialSize * Mathf.Lerp(1f, variantData.PopSizeIncrease, progress);
          UpdateShape();
          yield return null;
        }
      }

      // Spawn particle effect
      if (_popParticlePrefab != null)
      {
        GameObject particleObj = Instantiate(_popParticlePrefab, transform.position, Quaternion.identity);
        particleObj.transform.localScale = Vector3.one * Size;

        BubblePopEffect popEffect = particleObj.GetComponent<BubblePopEffect>();
        if (popEffect != null)
        {
          popEffect.Initialize(this);
        }
      }

      // Calculate pop effect radius based on bubble size
      float popRadius = Radius * variantData.PopRadiusRatio;

      // Apply explosion force to nearby bubbles and check for matching neighbors
      Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, popRadius);
      foreach (Collider2D collider in nearbyColliders)
      {
        if (collider.gameObject != gameObject && collider.CompareTag("Bubble"))
        {
          Bubble targetBubble = collider.GetComponent<Bubble>();
          Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
          if (rb != null && targetBubble != null)
          {
            Vector2 direction = (collider.transform.position - transform.position).normalized;
            float distance = Vector2.Distance(transform.position, collider.transform.position);

            float combinedRadius = popRadius + targetBubble.Radius;
            float overlap = Mathf.Max(0, combinedRadius - distance);
            float overlapRatio = overlap / combinedRadius;

            // Apply pop force
            float force = variantData.PopForce * overlapRatio;
            rb.AddForce(direction * force, ForceMode2D.Impulse);

            // Check if we should pop matching neighbors (using actual radii for touch detection)
            float actualCombinedRadius = Radius + targetBubble.Radius;
            float actualOverlap = Mathf.Max(0, actualCombinedRadius - distance);
            float actualOverlapRatio = actualOverlap / actualCombinedRadius;

            if (variantData.PopMatchingNeighbors && targetBubble.Variant == Variant &&
                actualOverlapRatio >= variantData.MinOverlapToPop)
            {
              float randomDelay = Random.Range(variantData.NeighborPopDelay.x, variantData.NeighborPopDelay.y);
              targetBubble.PopWithDelay(randomDelay);
            }
          }
        }
      }

      // Destroy the bubble
      Destroy(gameObject);
    }
    finally
    {
      _isAnimating = false;
    }
  }

  public void PopWithDelay(float delay)
  {
    if (IsPopped || Invulnerable || _isAnimating) return;
    StartCoroutine(DelayedPop(delay));
  }

  private IEnumerator DelayedPop(float delay)
  {
    yield return new WaitForSeconds(delay);
    if (!IsPopped && !Invulnerable)
    {
      Pop();
    }
  }

  private void OnDrawGizmos()
  {
    // Draw outer repulsion radius
    Gizmos.color = new Color(1f, 1f, 0f, 0.3f);  // Semi-transparent yellow
    Gizmos.DrawWireSphere(transform.position, Radius);

    // Draw core collision radius
    Gizmos.color = new Color(1f, 0f, 0f, 0.3f);  // Semi-transparent red
    Gizmos.DrawWireSphere(transform.position, Radius * CoreSizeRatio);
  }

  private void FixedUpdate()
  {
    // Apply drag force
    if (_rb != null)
    {
      BubbleVariant variantData = GameRules.BubbleVariantData(Variant);

      // Update gravity scale
      _rb.gravityScale = variantData.GravityFactor;

      // Update mass with non-linear scaling
      float area = Mathf.PI * Size * Size * 0.25f; // πr² = π(d/2)²
      _rb.mass = variantData.Density * Mathf.Pow(area, 0.8f);

      // Apply drag force
      _rb.AddForce(-_rb.linearVelocity * variantData.DragForce, ForceMode2D.Force);

      // Clamp velocity
      if (_rb.linearVelocity.magnitude > variantData.MaxVelocity)
      {
        _rb.linearVelocity = _rb.linearVelocity.normalized * variantData.MaxVelocity;
      }
    }
  }
}
