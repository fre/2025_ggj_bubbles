using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject BubblePrefab;
    [SerializeField] private float SpawnRate = 1f;
    [SerializeField] private int InitialSpawnCount = 5;

    [Header("Spawn Area")]
    [SerializeField] private Vector2 SpawnAreaSize = new Vector2(10f, 10f);

    [Header("Bubble Settings")]
    [SerializeField] private float MinRadius = 0.4f;
    [SerializeField] private float MaxRadius = 1.5f;
    [SerializeField] private float MinForce = 2f;
    [SerializeField] private float MaxForce = 5f;

    private float _nextSpawnTime;

    private void Start()
    {
        // Spawn initial batch of bubbles
        for (int i = 0; i < InitialSpawnCount; i++)
        {
            SpawnBubble();
        }
    }

    private void Update()
    {
        // Check if it's time to spawn a new bubble
        if (Time.time >= _nextSpawnTime && SpawnRate > 0)
        {
            SpawnBubble();
            _nextSpawnTime = Time.time + 1 / SpawnRate;
        }
    }

    private void SpawnBubble()
    {
        // Calculate random position within spawn area
        Vector3 randomPosition = transform.position + new Vector3(
            Random.Range(-SpawnAreaSize.x / 2, SpawnAreaSize.x / 2),
            Random.Range(-SpawnAreaSize.y / 2, SpawnAreaSize.y / 2),
            0f
        );

        // Instantiate the bubble
        GameObject bubble = Instantiate(BubblePrefab, randomPosition, Quaternion.identity);

        // Set random radius
        Bubble bubbleComponent = bubble.GetComponent<Bubble>();
        if (bubbleComponent != null)
        {
            bubbleComponent.BubbleRadius = Random.Range(MinRadius, MaxRadius);
            bubbleComponent.Hue = Random.Range(0f, 1f);
        }

        // Apply random force in random direction
        Rigidbody2D rb = bubble.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float randomForce = Random.Range(MinForce, MaxForce);
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
        Gizmos.DrawWireCube(transform.position, new Vector3(SpawnAreaSize.x, SpawnAreaSize.y, 0f));
    }
}
