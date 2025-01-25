using UnityEngine;
using System.Collections.Generic;

public class Bubble : MonoBehaviour
{
  // Static bubble tracking
  private static readonly List<Bubble> _activeBubbles = new List<Bubble>();
  public static IReadOnlyList<Bubble> ActiveBubbles => _activeBubbles;

  [Header("Bubble Properties")]
  public float Size = 1f;
  public float CoreSizeRatio = 0.6f;
  public float Hue = 0f;
  public bool IsPopped { get; private set; }

  [Header("Colliders")]
  [SerializeField] private CircleCollider2D _bubbleCollider;
  [SerializeField] private CircleCollider2D _coreCollider;

  public float Radius => Size * 0.5f;

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
    if (other.gameObject.CompareTag("Bubble"))
    {
      Bubble otherBubble = other.GetComponent<Bubble>();
      if (otherBubble == null) return;

      // Calculate centers distance
      Vector2 direction = (transform.position - other.transform.position).normalized;
      float distance = Vector2.Distance(transform.position, other.transform.position);

      // Calculate combined radii
      float combinedRadii = Radius + otherBubble.Radius;

      // Only repel if penetrating
      if (distance < combinedRadii)
      {
        // Force based on penetration depth
        float penetrationDepth = combinedRadii - distance;
        float forceMagnitude = GameRules.Data.RepulsionForce * (penetrationDepth / combinedRadii);

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
        if (distanceToCenter <= Radius)
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
    Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, GameRules.Data.PopRadius);
    foreach (Collider2D collider in nearbyColliders)
    {
      if (collider.gameObject != gameObject && collider.CompareTag("Bubble"))
      {
        Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
          Vector2 direction = (collider.transform.position - transform.position).normalized;
          float distance = Vector2.Distance(transform.position, collider.transform.position);
          float force = Mathf.Lerp(GameRules.Data.PopForce, 0f, distance / GameRules.Data.PopRadius);
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
    Gizmos.DrawWireSphere(transform.position, Radius);

    // Draw core collision radius
    Gizmos.color = new Color(1f, 0f, 0f, 0.3f);  // Semi-transparent red
    Gizmos.DrawWireSphere(transform.position, Radius * CoreSizeRatio);
  }
}
