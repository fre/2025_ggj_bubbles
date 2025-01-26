using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject BubblePrefab;
    [SerializeField] private LayerMask WallLayer; // Layer for walls

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

    private bool IsPositionClear(Vector3 position, float bubbleSize)
    {
        // Cast a small circle to check for walls
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            position,          // origin
            bubbleSize / 2,    // radius (half the bubble size)
            Vector2.zero,      // direction (not used since we're just checking position)
            0f,               // distance
            WallLayer         // layer mask for walls
        );

        return hits.Length == 0;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        return transform.position + new Vector3(
            Random.Range(-GameRules.Data.WorldSize.x / 2, GameRules.Data.WorldSize.x / 2),
            Random.Range(-GameRules.Data.WorldSize.y / 2, GameRules.Data.WorldSize.y / 2),
            0f
        );
    }

    private void SpawnBubble()
    {
        if (Bubble.ActiveBubbles.Count >= GameRules.Data.MaxBubbles)
        {
            // Debug.LogWarning("Max bubbles reached");
            return;
        }

        // Get random variant info first to know the size
        int variant = Random.Range(GameRules.Data.MinVariantId, GameRules.Data.VariantCount);
        BubbleVariant variantData = GameRules.BubbleVariantData(variant);
        float bubbleSize = Random.Range(variantData.SizeRange.x, variantData.SizeRange.y);

        // Try up to 5 times to find a clear position
        Vector3 spawnPosition = Vector3.zero;
        bool foundClearPosition = false;

        for (int attempt = 0; attempt < 5; attempt++)
        {
            spawnPosition = GetRandomSpawnPosition();
            if (IsPositionClear(spawnPosition, bubbleSize))
            {
                foundClearPosition = true;
                break;
            }
        }

        if (!foundClearPosition)
        {
            return; // Couldn't find a clear position after 5 attempts
        }

        // Instantiate the bubble at the clear position
        GameObject bubble = Instantiate(BubblePrefab, spawnPosition, Quaternion.identity);
        bubble.name = BubblePrefab.name;

        // Set bubble properties
        Bubble bubbleComponent = bubble.GetComponent<Bubble>();
        if (bubbleComponent != null)
        {
            bubbleComponent.Variant = variant;
            bubbleComponent.Size = bubbleSize;
            bubbleComponent.UpdateShape();

            // Apply initial impulse
            Rigidbody2D rb = bubble.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float impulse = variantData.InitialImpulse;
                float randomForce = Random.Range(impulse / 2, impulse);
                float randomAngle = Random.Range(0f, 360f);
                Vector2 randomDirection = new Vector2(
                    Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                    Mathf.Sin(randomAngle * Mathf.Deg2Rad)
                );
                rb.AddForce(randomDirection * randomForce, ForceMode2D.Impulse);
            }
        }
    }

    // Optional: Visualize spawn area in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(GameRules.Data.WorldSize.x, GameRules.Data.WorldSize.y, 0f));
    }
}
