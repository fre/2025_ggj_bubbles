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
  private int _variant;
  public int Variant
  {
    get => _variant;
    set
    {
      _variant = value;
      Hue = (float)value / GameRules.Data.VariantCount;
    }
  }
  public float Hue { get; private set; }
  public bool IsPopped { get; private set; }
  private bool _isAnimating = false; // Guard flag for animations

  [Header("Colliders")]
  [SerializeField] private CircleCollider2D _bubbleCollider;
  [SerializeField] private CircleCollider2D _coreCollider;

  [Header("Audio")]
  [SerializeField] private BubbleSoundManager _soundManager;

  public float Radius => Size * 0.5f;
  private float Volume => Mathf.PI * Size * Size;  // 2D "volume" is area

  private void Start()
  {
    UpdateShape();
    _activeBubbles.Add(this);
  }

  private void OnDestroy()
  {
    _activeBubbles.Remove(this);
  }

  public void UpdateShape()
  {
    // Update visual scale
    transform.localScale = Vector3.one * Size;

    if (_coreCollider != null)
    {
      _coreCollider.radius = CoreSizeRatio * 0.5f;
    }
  }

  private void OnTriggerStay2D(Collider2D other)
  {
    if (_isAnimating) return; // Skip collision checks if animating

    if (other.gameObject.CompareTag("Bubble"))
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
      if (Variant == otherBubble.Variant)
      {
        if (GameRules.Data.PopMatchingVariants && overlapRatio >= GameRules.Data.MinOverlapToPop)
        {
          Pop();
          otherBubble.Pop();
          return;
        }
        else if (GameRules.Data.MergeMatchingVariants && overlapRatio >= GameRules.Data.MinOverlapToMerge)
        {
          MergeWith(otherBubble);
          return;
        }
      }

      // Only repel if penetrating
      if (distance < combinedRadii)
      {
        // Force based on penetration depth
        float forceMagnitude = GameRules.Data.RepulsionForce * overlapRatio;

        Vector2 force = direction * forceMagnitude;

        // Apply forces to both bubbles' Rigidbodies
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Rigidbody2D otherRb = other.GetComponent<Rigidbody2D>();

        if (rb != null) rb.AddForce(force, ForceMode2D.Force);
        if (otherRb != null) otherRb.AddForce(-force, ForceMode2D.Force);
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
      // Play merge sound immediately
      if (_soundManager != null)
      {
        _soundManager.PlayMergeSound(transform.position);
      }

      // Wait for merge delay
      if (GameRules.Data.MergeDelay > 0)
      {
        float startTime = Time.time;
        float initialSize = Size;
        float otherInitialSize = other.Size;

        // Shrink animation
        while (Time.time < startTime + GameRules.Data.MergeDelay)
        {
          float progress = (Time.time - startTime) / GameRules.Data.MergeDelay;
          float currentShrink = Mathf.Lerp(1f, GameRules.Data.MergeSizeShrink, progress);

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
      newBubbleComponent.CoreSizeRatio = GameRules.Data.CoreSizeRatio;

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
    float mergeRadius = size * GameRules.Data.MergeRadiusRatio;

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

          float force = GameRules.Data.MergeForce * overlapRatio;
          rb.AddForce(direction * force, ForceMode2D.Impulse);
        }
      }
    }
  }

  public float DistanceToPoint(Vector2 point)
  {
    return Vector2.Distance(point, (Vector2)transform.position) / Radius;
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

  public static void TryPopAtPoint(Vector2 point)
  {
    Bubble closest = FindClosestBubbleTo(point);
    if (closest != null && closest.DistanceToPoint(point) < 1)  // Check if point is inside bubble
    {
      closest.Pop();
    }
  }

  public void Pop()
  {
    if (IsPopped) return;
    if (Invulnerable) return;
    if (_isAnimating) return;

    IsPopped = true;
    _isAnimating = true;
    StartCoroutine(PopEffect());
  }

  private IEnumerator PopEffect()
  {
    try
    {
      // Play pop sound immediately
      if (_soundManager != null)
      {
        Debug.Log("Attempting to play pop sound");
        _soundManager.PlayPopSound(transform.position);
      }

      // Store initial size
      float initialSize = Size;
      float startTime = Time.time;

      // Growth animation
      if (GameRules.Data.PopDelay > 0)
      {
        while (Time.time < startTime + GameRules.Data.PopDelay)
        {
          float progress = (Time.time - startTime) / GameRules.Data.PopDelay;
          Size = initialSize * Mathf.Lerp(1f, GameRules.Data.PopSizeIncrease, progress);
          UpdateShape();
          yield return null;
        }
      }

      // Calculate pop effect radius based on bubble size
      float popRadius = Radius * GameRules.Data.PopRadiusRatio;

      // Apply explosion force to nearby bubbles
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

            float force = GameRules.Data.PopForce * overlapRatio;
            rb.AddForce(direction * force, ForceMode2D.Impulse);
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

  private void OnDrawGizmos()
  {
    // Draw outer repulsion radius
    Gizmos.color = new Color(1f, 1f, 0f, 0.3f);  // Semi-transparent yellow
    Gizmos.DrawWireSphere(transform.position, Radius);

    // Draw core collision radius
    Gizmos.color = new Color(1f, 0f, 0f, 0.3f);  // Semi-transparent red
    Gizmos.DrawWireSphere(transform.position, Radius * CoreSizeRatio);
  }
}
