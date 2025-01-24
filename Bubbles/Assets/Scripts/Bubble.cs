using UnityEngine;

public class Bubble : MonoBehaviour
{
  [Header("Bubble Properties")]
  public float BubbleRadius = 1f;  // Visual and repulsion radius
  public float Hue = 0f;

  [Header("Collision Settings")]
  [SerializeField] private float _coreRadiusMultiplier = 0.6f;  // Multiplier of the standard 0.5 collider
  [SerializeField] private float _repulsionForce = 5f;

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
