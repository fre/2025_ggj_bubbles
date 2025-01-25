using UnityEngine;

public class Bubble : MonoBehaviour
{
  [Header("Bubble Properties")]
  public float BubbleRadius = 1f;  // Visual and repulsion radius
  public float Hue = 0f;
  public bool IsPopped { get; private set; }

  [Header("Collision Settings")]
  [SerializeField] private float _coreRadiusMultiplier = 0.6f;  // Multiplier of the standard 0.5 collider
  [SerializeField] private float _repulsionForce = 5f;

  [Header("Pop Settings")]
  [SerializeField] private float _popForce = 10f;
  [SerializeField] private float _popRadius = 3f;

  private CircleCollider2D _coreCollider;
  private const float BASE_COLLIDER_RADIUS = 0.5f;  // Unity's default circle collider radius

  private void Awake()
  {
    _coreCollider = GetComponent<CircleCollider2D>();
    if (_coreCollider != null)
    {
      _coreCollider.radius = BASE_COLLIDER_RADIUS * _coreRadiusMultiplier;
    }

    // Set the visual scale based on bubble radius
    transform.localScale = Vector3.one * (BubbleRadius * 2f);  // Diameter = 2 * radius
  }

  private void OnTriggerStay2D(Collider2D other)
  {
    if (other.gameObject.CompareTag("Bubble"))
    {
      Bubble otherBubble = other.GetComponent<Bubble>();
      if (otherBubble == null) return;

      // Calculate centers distance
      Vector2 direction = (transform.position - other.transform.position).normalized;
      float distance = Vector2.Distance(transform.position, other.transform.position);

      // Calculate combined radii
      float combinedRadii = BubbleRadius + otherBubble.BubbleRadius;

      // Only repel if penetrating
      if (distance < combinedRadii)
      {
        // Force based on penetration depth
        float penetrationDepth = combinedRadii - distance;
        float forceMagnitude = _repulsionForce * (penetrationDepth / combinedRadii);

        Vector2 force = direction * forceMagnitude;

        // Apply forces to both bubbles' Rigidbodies
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Rigidbody2D otherRb = other.GetComponent<Rigidbody2D>();

        if (rb != null) rb.AddForce(force, ForceMode2D.Force);
        if (otherRb != null) otherRb.AddForce(-force, ForceMode2D.Force);
      }
    }
  }

  private void OnMouseDown()
  {
    if (!IsPopped)
    {
      // Get click position using camera ray
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      Plane plane = new Plane(Vector3.forward, 0); // z=0 plane

      if (plane.Raycast(ray, out float distance))
      {
        Vector3 hitPoint = ray.GetPoint(distance);
        float distanceToCenter = Vector2.Distance(new Vector2(hitPoint.x, hitPoint.y), (Vector2)transform.position);

        // Use the outer bubble radius
        if (distanceToCenter <= BubbleRadius)
        {
          Pop();
        }
      }
    }
  }

  public void Pop()
  {
    IsPopped = true;

    // Apply explosion force to nearby bubbles
    Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, _popRadius);
    foreach (Collider2D collider in nearbyColliders)
    {
      if (collider.gameObject != gameObject && collider.CompareTag("Bubble"))
      {
        Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
          Vector2 direction = (collider.transform.position - transform.position).normalized;
          float distance = Vector2.Distance(transform.position, collider.transform.position);
          float force = Mathf.Lerp(_popForce, 0f, distance / _popRadius);
          rb.AddForce(direction * force, ForceMode2D.Impulse);
        }
      }
    }

    // TODO: Add pop effect/particles here if desired

    // Destroy the bubble
    Destroy(gameObject);
  }

  private void OnDrawGizmos()
  {
    // Draw outer repulsion radius
    Gizmos.color = new Color(1f, 1f, 0f, 0.3f);  // Semi-transparent yellow
    Gizmos.DrawWireSphere(transform.position, BubbleRadius);

    // Draw core collision radius
    Gizmos.color = new Color(1f, 0f, 0f, 0.3f);  // Semi-transparent red
    Gizmos.DrawWireSphere(transform.position, BASE_COLLIDER_RADIUS * _coreRadiusMultiplier);
  }
}
