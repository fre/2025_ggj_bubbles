using UnityEngine;

public enum WinConditionType
{
  BubblesPopped,    // Win when X bubbles are popped
  MinBubblesLeft,   // Win when bubbles <= X
  MaxBubblesLeft    // Win when bubbles >= X
}

public enum SuccessMeasure
{
  Time,   // Lower is better
  Clicks  // Lower is better
}

[CreateAssetMenu(fileName = "GameRulesData", menuName = "Bubbles/Game Rules Data")]
public class GameRulesData : ScriptableObject
{
  [Header("Global")]
  public int MaxBubbles = 100;
  public int TargetBubbleCount = 10;
  public WinConditionType WinCondition = WinConditionType.BubblesPopped;
  public SuccessMeasure SuccessMeasure = SuccessMeasure.Clicks;
  public Vector2 WorldSize = new Vector2(20f, 12f);

  [Header("Bubble Variants")]
  public int VariantCount = 5;
  public BubbleVariant DefaultVariant;
  public BubbleVariant[] VariantOverrides;

  [Header("Spawning")]
  public float SpawnInterval = 0.1f;
  public int InitialSpawnCount = 5;
  public int MinVariantId = 0;  // Minimum variant ID to spawn (inclusive)

  [Header("Rendering")]
  public float OutlineThickness = 0.05f;
  public Color OutlineColor = new Color(1f, 1f, 1f, 0.8f);
  public Color BackgroundColor = new Color(0f, 0f, 0f, 0f);
  public float OutlineSmoothRadius = 1.0f;
  public float SmallRadiusPreservationFactor = 0.5f;
  public float WaveAmplitude = 0.02f;
  public float WaveCount = 8f;
  public float WaveRotationSpeed = 1f;

  [Header("Hover Effects")]
  public float HoverTransitionSpeed = 0.5f;
  public Color HoverOutlineColor = new Color(1f, 1f, 1f, 1f);
  public float HoverOutlineThickness = 0.08f;

  [Header("Bubble Transparency")]
  public float CoreOpacity = 0.9f;
  public float EdgeOpacity = 0.4f;
  public float OpacityFalloff = 2.0f;
  public float OpacitySmoothing = 0.5f;

  public BubbleVariant GetVariantData(int variant)
  {
    if (variant < 0 || variant >= VariantCount)
    {
      Debug.LogWarning($"Variant {variant} is out of range [0, {VariantCount}). Using default variant.");
      return DefaultVariant;
    }

    if (VariantOverrides != null && variant < VariantOverrides.Length && VariantOverrides[variant] != null)
    {
      return VariantOverrides[variant];
    }

    return DefaultVariant;
  }
}