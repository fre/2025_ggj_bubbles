using UnityEngine;

[CreateAssetMenu(fileName = "GameRulesData", menuName = "Bubbles/Game Rules Data")]
public class GameRulesData : ScriptableObject
{
  [Header("Global")]
  public int MaxBubbles = 100;
  public Vector2 WorldSize = new Vector2(20f, 12f);

  [Header("Bubble Variants")]
  public int VariantCount = 5;
  public BubbleVariant DefaultVariant;
  public BubbleVariant[] VariantOverrides;

  [Header("Spawning")]
  public float SpawnRadius = 0.5f;
  public float SpawnInterval = 0.1f;
  public int InitialSpawnCount = 5;

  [Header("Rendering")]
  public float OutlineThickness = 0.1f;
  public float CoreOpacity = 0.5f;
  public float EdgeOpacity = 0.5f;
  public float OpacityFalloff = 0.5f;
  public float OpacitySmoothing = 0.5f;
  public float OutlineSmoothRadius = 0.5f;
  public float SmallRadiusPreservationFactor = 0.5f;
  public Color OutlineColor = Color.white;
  public Color BackgroundColor = Color.black;
  public float WaveAmplitude = 0.02f;
  public float WaveCount = 8f;
  public float WaveRotationSpeed = 1f;

  [Header("Hover Effects")]
  public float HoverTransitionSpeed = 0.5f;
  public Color HoverOutlineColor = new Color(1f, 1f, 1f, 1f);
  public float HoverOutlineThickness = 0.08f;

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