using UnityEngine;

[CreateAssetMenu(fileName = "GameRulesData", menuName = "Bubbles/Game Rules Data")]
public class GameRulesData : ScriptableObject
{
  [Header("Global")]
  public int MaxBubbles = 100;
  public Vector2 WorldSize = new Vector2(20f, 12f);

  [Header("Bubble Variants")]
  public int VariantCount = 5;

  [Header("Spawning")]
  public float SpawnRadius = 0.5f;
  public float SpawnInterval = 0.1f;
  public int InitialSpawnCount = 5;
  public float MinBubbleSize = 0.3f;
  public float MaxBubbleSize = 1.0f;

  [Header("Physics")]
  public float BounceForce = 5f;
  public float DragForce = 1f;
  public float MaxVelocity = 10f;
  public float CoreSizeRatio = 0.6f;
  public float RepulsionForce = 5f;

  [Header("Pop Effects")]
  public bool PopMatchingVariants = false;
  public float MinOverlapToPop = 0.4f;
  public float PopForce = 10f;
  public float PopRadiusRatio = 1.5f;
  public float PopDelay = 0.2f;
  public float PopSizeIncrease = 1.5f; // How much the bubble grows before popping

  [Header("Merge Effects")]
  public bool MergeMatchingVariants = true;
  public float MinOverlapToMerge = 0.3f;
  public float MergeForce = -8f;
  public float MergeRadiusRatio = 1.5f;
  public float MergeDelay = 0.2f;
  public float MergeSizeShrink = 0.7f; // How much the bubbles shrink before merging

  [Header("Rendering")]
  public float OutlineThickness = 0.05f;
  public Color OutlineColor = new Color(1f, 1f, 1f, 0.8f);  // White with 80% opacity
  public Color BackgroundColor = new Color(0f, 0f, 0f, 0f); // Fully transparent black
  public float OutlineSmoothRadius = 1.0f;  // Controls the smoothness of outline transitions
  public float WaveAmplitude = 0.02f;       // Max deviation from radius (as fraction of radius)
  public float WaveCount = 8f;              // Number of complete waves around the bubble
  public float WaveRotationSpeed = 1f;      // Rotations per second

  [Header("Hover Effects")]
  public float HoverTransitionSpeed = 0.5f;  // How fast the hover effect transitions
  public Color HoverOutlineColor = new Color(1f, 1f, 1f, 1f);  // Brighter white for hover
  public float HoverOutlineThickness = 0.08f;

  [Header("Bubble Transparency")]
  public float CoreOpacity = 0.9f;     // Alpha at bubble center
  public float EdgeOpacity = 0.4f;     // Alpha at bubble edge
  public float OpacityFalloff = 2.0f;  // Power curve for opacity transition (higher = sharper falloff)
  public float OpacitySmoothing = 0.5f;  // Smooths the opacity curve (0 = sharp, 1 = smooth)
}