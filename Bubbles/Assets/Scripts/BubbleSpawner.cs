using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject BubblePrefab;  // Fallback prefab if none set in GameRules
    [SerializeField] private LayerMask WallLayer; // Layer for walls
    public float WorldMargin = 0.5f;

    private float _nextSpawnTime;
    private Camera _mainCamera;

    private static readonly Plane _gamePlane = new Plane(Vector3.forward, 0);


    private void Start()
    {
        _mainCamera = Camera.main;
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

        // Handle click spawning
        if (GameRules.Data.SpawnOnClick && Input.GetMouseButtonDown(0))
        {
            Vector2? mousePosition = GetMouseWorldPoint();
            if (mousePosition.HasValue)
            {
                // Get variant and size if not specified
                int bubbleVariant = GameRules.Data.SpawnOnClickVariant >= 0 ?
                    GameRules.Data.SpawnOnClickVariant :
                    Random.Range(GameRules.Data.MinVariantId, GameRules.Data.VariantCount);
                float impulse = GameRules.BubbleVariantData(bubbleVariant).SpawnOnClickImpulse >= 0 ?
                    GameRules.BubbleVariantData(bubbleVariant).SpawnOnClickImpulse :
                    GameRules.BubbleVariantData(bubbleVariant).InitialImpulse;
                SpawnBubbleAt(mousePosition.Value, bubbleVariant, GameRules.Data.SpawnOnClickSize, impulse);
            }
        }
    }

    private bool IsPositionClear(Vector3 position, float bubbleSize)
    {
        // Check if point overlaps with any walls
        RaycastHit2D hit = Physics2D.Raycast(
            position,        // origin
            Vector2.zero,    // direction (not used since we're just checking position)
            0f,             // distance
            WallLayer       // layer mask for walls
        );

        return hit.collider == null;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        return transform.position + new Vector3(
            Random.Range(-GameRules.Data.WorldSize.x / 2 + WorldMargin, GameRules.Data.WorldSize.x / 2 - WorldMargin),
            Random.Range(-GameRules.Data.WorldSize.y / 2 + WorldMargin, GameRules.Data.WorldSize.y / 2 - WorldMargin),
            0f
        );
    }

    private void SpawnBubble()
    {
        if (GameRules.Data.PopRandomToSpawn
            && (GameRules.Data.MinBubblesToPopRandom >= 0 ?
                Bubble.ActiveBubbles.Count >= GameRules.Data.MinBubblesToPopRandom :
                Bubble.ActiveBubbles.Count >= GameRules.Data.MaxBubbles))
        {
            // Try to pop a random bubble to make space
            TryPopRandomBubble();
        }
        if (Bubble.ActiveBubbles.Count >= GameRules.Data.MaxBubbles)
        {
            return;

        }

        // Get random variant info first to know the size
        int variant = Random.Range(GameRules.Data.MinVariantId, GameRules.Data.VariantCount);
        BubbleVariant variantData = GameRules.BubbleVariantData(variant);
        float bubbleSize = Random.Range(variantData.SizeRange.x, variantData.SizeRange.y);
        float impulse = variantData.InitialImpulse;

        SpawnBubbleAt(GetRandomSpawnPosition(), variant, bubbleSize, impulse);
    }

    private bool TryPopRandomBubble()
    {
        if (Bubble.ActiveBubbles.Count == 0)
        {
            return false;
        }

        // Get a random bubble
        int randomIndex = Random.Range(0, Bubble.ActiveBubbles.Count);
        Bubble bubbleToPop = Bubble.ActiveBubbles[randomIndex];

        // Check if the bubble can be popped
        if (bubbleToPop != null && !bubbleToPop.Invulnerable)
        {
            bubbleToPop.Pop();
            return true;
        }

        return false;
    }

    private void SpawnBubbleAt(Vector2 position, int bubbleVariant, float bubbleSize, float impulse)
    {
        if (Bubble.ActiveBubbles.Count >= GameRules.Data.MaxBubbles)
        {
            if (GameRules.Data.PopRandomToSpawn)
            {
                // Try to pop a random bubble to make space
                if (!TryPopRandomBubble())
                {
                    return; // Failed to pop a bubble (probably all invulnerable), so abandon spawning
                }
            }
            else
            {
                return;
            }
        }

        if (!IsPositionClear(position, bubbleSize))
        {
            Debug.Log("Position is not clear");
            return;
        }

        BubbleVariant variantData = GameRules.BubbleVariantData(bubbleVariant);

        // Use prefab from GameRules if available, otherwise use serialized prefab
        GameObject prefabToUse = GameRules.Data.BubblePrefab != null ? GameRules.Data.BubblePrefab : BubblePrefab;
        if (prefabToUse == null)
        {
            Debug.LogError("No bubble prefab set in either GameRules or BubbleSpawner!");
            return;
        }

        // Instantiate the bubble
        GameObject bubble = Instantiate(prefabToUse, position, Quaternion.identity);
        bubble.name = prefabToUse.name;

        // Set bubble properties
        Bubble bubbleComponent = bubble.GetComponent<Bubble>();
        if (bubbleComponent != null)
        {
            bubbleComponent.Variant = bubbleVariant;
            bubbleComponent.Size = bubbleSize;
            bubbleComponent.UpdateShape();

            // Apply initial impulse
            Rigidbody2D rb = bubble.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
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

    private Vector2? GetMouseWorldPoint()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (_gamePlane.Raycast(ray, out float distance))
        {
            Vector3 worldPoint = ray.GetPoint(distance);
            return new Vector2(worldPoint.x, worldPoint.y);
        }
        return null;
    }
}
