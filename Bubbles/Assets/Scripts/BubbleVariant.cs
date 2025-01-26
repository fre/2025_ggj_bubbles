using UnityEngine;

public enum VariantForceType
{
  None,
  Attract,
  Repulse
}

[CreateAssetMenu(fileName = "BubbleVariant", menuName = "Bubbles/Bubble Variant")]
public class BubbleVariant : ScriptableObject
{
  [Header("Visual")]
  public float ColorSaturation = 0.7f;
  public float ColorValue = 0.6f;

  [Header("Physics")]
  public float InitialImpulse = 5f;
  public float DragForce = 1f;
  public float MaxVelocity = 10f;
  public float GravityFactor = 0f;
  public float CoreSizeRatio = 0.6f;
  public float RepulsionForce = 5f;
  public float AttractionForce = 3f;
  public VariantForceType MatchingVariantForce = VariantForceType.Repulse;
  public VariantForceType NonMatchingVariantForce = VariantForceType.Repulse;

  [Header("Spawning")]
  public Vector2 SizeRange = new Vector2(1.2f, 3f);
  public float Density = 1f;  // Mass per unit area

  [Header("Pop Effects")]
  public bool PopOnClick = true;  // Whether this variant can be popped by clicking
  public bool PopMatchingVariants = false;
  public bool PopMatchingNeighbors = false;
  public Vector2 NeighborPopDelay = new Vector2(0.1f, 0.2f);
  public float MinOverlapToPop = 0.4f;
  public float PopForce = 10f;
  public float PopRadiusRatio = 1.5f;
  public float PopDelay = 0.2f;
  public float PopSizeIncrease = 1.5f;
  public float PopAtSize = 5.0f;

  [Header("Merge Effects")]
  public bool MergeMatchingVariants = true;
  public float MinOverlapToMerge = 0.3f;
  public float MergeForce = -8f;
  public float MergeRadiusRatio = 1.5f;
  public float MergeDelay = 0.2f;
  public float MergeSizeShrink = 0.7f;
}