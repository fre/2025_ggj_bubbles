using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject BubblePrefab;

    private float _nextSpawnTime;

    private void Start()
    {
        // Spawn initial batch of bubbles
        for (int i = 0; i < GameRules.Data.InitialSpawnCount; i++)
        {
            SpawnBubble();
        }
    }

    private void Update()
    {
        // Check if it's time to spawn a new bubble
        if (Time.time >= _nextSpawnTime && GameRules.Data.SpawnInterval > 0)
        {
            SpawnBubble();
            _nextSpawnTime = Time.time + GameRules.Data.SpawnInterval;
        }
    }

    private void SpawnBubble()
    {
        if (Bubble.ActiveBubbles.Count >= GameRules.Data.MaxBubbles)
        {
            // Debug.LogWarning("Max bubbles reached");
            return;
        }

        // Calculate random position within spawn area
        Vector3 randomPosition = transform.position + new Vector3(
            Random.Range(-GameRules.Data.WorldSize.x / 2, GameRules.Data.WorldSize.x / 2),
            Random.Range(-GameRules.Data.WorldSize.y / 2, GameRules.Data.WorldSize.y / 2),
            0f
        );

        // Instantiate the bubble
        GameObject bubble = Instantiate(BubblePrefab, randomPosition, Quaternion.identity);
        bubble.name = BubblePrefab.name;

        // Set bubble properties
        Bubble bubbleComponent = bubble.GetComponent<Bubble>();
        if (bubbleComponent != null)
        {
            bubbleComponent.Size = Random.Range(
                GameRules.Data.MinBubbleSize,
                GameRules.Data.MaxBubbleSize
            );
            bubbleComponent.CoreSizeRatio = GameRules.Data.CoreSizeRatio;
            bubbleComponent.Variant = Random.Range(0, GameRules.Data.VariantCount);
        }

        // Apply random force in random direction
        Rigidbody2D rb = bubble.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float randomForce = Random.Range(
                GameRules.Data.BounceForce / 2,
                GameRules.Data.BounceForce
            );
            float randomAngle = Random.Range(0f, 360f);
            Vector2 randomDirection = new Vector2(
                Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                Mathf.Sin(randomAngle * Mathf.Deg2Rad)
            );
            rb.AddForce(randomDirection * randomForce, ForceMode2D.Impulse);
        }
    }

    // Optional: Visualize spawn area in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(GameRules.Data.WorldSize.x, GameRules.Data.WorldSize.y, 0f));
    }
}
