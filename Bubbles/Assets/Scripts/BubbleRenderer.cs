using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
public class BubbleRenderer : MonoBehaviour
{
  private static int BubbleDataProperty;
  private static int BubbleCountProperty;
  private static int MaxBubbleCountProperty;
  private static int OutlineThicknessProperty;
  private static int CoreOpacityProperty;
  private static int EdgeOpacityProperty;
  private static int OpacityFalloffProperty;
  private static int OpacitySmoothingProperty;
  private static int OutlineColorProperty;
  private static int BackgroundColorProperty;
  private static int HoverOutlineColorProperty;
  private static int HoverOutlineThicknessProperty;
  private static int OutlineSmoothRadiusProperty;
  private static int SmallRadiusPreservationFactorProperty;

  private Material _material;
  private Texture2D _bubbleDataTexture;
  private GameRulesData _lastRules;

  private static int CHANNELS_PER_BUBBLE = 4;

  private void Awake()
  {
    Initialize();
  }

  private void OnEnable()
  {
    _lastRules = GameRules.Data;
  }

  private void CreateBubbleDataTexture(int maxBubbles)
  {
    if (_bubbleDataTexture != null)
    {
      DestroyImmediate(_bubbleDataTexture);
    }
    _bubbleDataTexture = new Texture2D(CHANNELS_PER_BUBBLE, maxBubbles, TextureFormat.RGBAFloat, false);
    _bubbleDataTexture.filterMode = FilterMode.Point;
    _bubbleDataTexture.wrapMode = TextureWrapMode.Clamp;

    // Initialize with empty data
    Color[] initialData = new Color[CHANNELS_PER_BUBBLE * maxBubbles];
    for (int i = 0; i < initialData.Length; i++)
    {
      initialData[i] = Color.clear;
    }
    _bubbleDataTexture.SetPixels(initialData);
    _bubbleDataTexture.Apply();

    _material.SetTexture(BubbleDataProperty, _bubbleDataTexture);
    _material.SetFloat(MaxBubbleCountProperty, maxBubbles);
  }

  private void Initialize()
  {
    if (_material != null) return;

    if (GameRules.Data == null)
    {
      Debug.LogError("GameRules.Data is null! Cannot initialize BubbleRenderer.");
      return;
    }

    _material = GetComponent<MeshRenderer>().sharedMaterial;
    if (_material == null)
    {
      Debug.LogError("Material is null! Cannot initialize BubbleRenderer.");
      return;
    }

    IndexPropertyIds();

    // Create texture for bubble data
    int maxBubbles = Mathf.Max(1, GameRules.Data.MaxBubbles); // Ensure at least 1 bubble
    CreateBubbleDataTexture(maxBubbles);
    UpdateShaderProperties();
  }

  private void IndexPropertyIds()
  {
    BubbleDataProperty = Shader.PropertyToID("_BubbleData");
    BubbleCountProperty = Shader.PropertyToID("_BubbleCount");
    MaxBubbleCountProperty = Shader.PropertyToID("_MaxBubbleCount");
    OutlineThicknessProperty = Shader.PropertyToID("_OutlineThickness");
    CoreOpacityProperty = Shader.PropertyToID("_CoreOpacity");
    EdgeOpacityProperty = Shader.PropertyToID("_EdgeOpacity");
    OpacityFalloffProperty = Shader.PropertyToID("_OpacityFalloff");
    OpacitySmoothingProperty = Shader.PropertyToID("_OpacitySmoothing");
    OutlineColorProperty = Shader.PropertyToID("_OutlineColor");
    BackgroundColorProperty = Shader.PropertyToID("_BackgroundColor");
    HoverOutlineColorProperty = Shader.PropertyToID("_HoverOutlineColor");
    HoverOutlineThicknessProperty = Shader.PropertyToID("_HoverOutlineThickness");
    OutlineSmoothRadiusProperty = Shader.PropertyToID("_OutlineSmoothRadius");
    SmallRadiusPreservationFactorProperty = Shader.PropertyToID("_SmallRadiusPreservationFactor");
  }

  private void UpdateShaderProperties()
  {
    _material.SetFloat(OutlineThicknessProperty, GameRules.Data.OutlineThickness);
    _material.SetFloat(CoreOpacityProperty, GameRules.Data.CoreOpacity);
    _material.SetFloat(EdgeOpacityProperty, GameRules.Data.EdgeOpacity);
    _material.SetFloat(OpacityFalloffProperty, GameRules.Data.OpacityFalloff);
    _material.SetFloat(OpacitySmoothingProperty, GameRules.Data.OpacitySmoothing);
    _material.SetColor(OutlineColorProperty, GameRules.Data.OutlineColor);
    _material.SetColor(BackgroundColorProperty, GameRules.Data.BackgroundColor);
    _material.SetColor(HoverOutlineColorProperty, GameRules.Data.HoverOutlineColor);
    _material.SetFloat(HoverOutlineThicknessProperty, GameRules.Data.HoverOutlineThickness);
    _material.SetFloat(OutlineSmoothRadiusProperty, GameRules.Data.OutlineSmoothRadius);
    _material.SetFloat(SmallRadiusPreservationFactorProperty, GameRules.Data.SmallRadiusPreservationFactor);
  }

  private void OnValidate()
  {
    if (!Application.isPlaying)
    {
      Initialize();
    }
  }

  private void Update()
  {
    // Check if rules have changed
    if (_lastRules != GameRules.Data)
    {
      _lastRules = GameRules.Data;
      UpdateShaderProperties();
    }

    if (!Application.isPlaying)
    {
      UpdateShaderProperties();
    }
  }

  private void LateUpdate()
  {
    if (_material == null) Initialize();
    if (_material == null || GameRules.Data == null) return;

    int maxBubbles = Mathf.Max(1, GameRules.Data.MaxBubbles);

    // Check if texture needs to be recreated
    if (_bubbleDataTexture == null || _bubbleDataTexture.height != maxBubbles)
    {
      CreateBubbleDataTexture(maxBubbles);
    }

    // Get active bubbles from static list or FindObjectsOfType in editor
    IReadOnlyList<Bubble> activeBubbles = Application.isPlaying ?
      Bubble.ActiveBubbles :
      GameObject.FindObjectsByType<Bubble>(FindObjectsSortMode.None);

    if (activeBubbles == null) return;

    int bubbleCount = Mathf.Min(activeBubbles.Count, maxBubbles);
    Color[] bubbleData = new Color[CHANNELS_PER_BUBBLE * maxBubbles];

    // Update texture data
    for (int i = 0; i < maxBubbles; i++)
    {
      if (i < bubbleCount && i < activeBubbles.Count)
      {
        var bubble = activeBubbles[i];
        if (bubble == null) continue;

        Vector3 worldPos = bubble.transform.position;
        float radius = bubble.Radius;

        // Calculate texture indices for each column
        int baseIndex = CHANNELS_PER_BUBBLE * i;

        // Column 0: Position, radius, and bubble index
        bubbleData[baseIndex] = new Color(worldPos.x, worldPos.y, radius, i);

        // Column 1: Hover state and hue
        BubbleVariant variantData = GameRules.BubbleVariantData(bubble.Variant);
        bubbleData[baseIndex + 1] = new Color(bubble.Hue, bubble.HoverT, 0, 0);

        // Column 2: Full HSV color data (hue, saturation, value)
        bubbleData[baseIndex + 2] = new Color(bubble.Hue, variantData.ColorSaturation, variantData.ColorValue, variantData.Opacity);

        // Column 3: Wave parameters (amplitude, count, rotation)
        float waveRotation = (Time.time * GameRules.Data.WaveRotationSpeed * 2 * Mathf.PI) % (2 * Mathf.PI);
        bubbleData[baseIndex + 3] = new Color(
            GameRules.Data.WaveAmplitude,
            GameRules.Data.WaveCount,
            waveRotation,
            0  // Reserved
        );
      }
      else
      {
        // Clear all columns for unused bubbles
        int baseIndex = CHANNELS_PER_BUBBLE * i;
        bubbleData[baseIndex] = Color.clear;     // Column 0
        bubbleData[baseIndex + 1] = Color.clear; // Column 1
        bubbleData[baseIndex + 2] = Color.clear; // Column 2
        bubbleData[baseIndex + 3] = Color.clear; // Column 3
      }
    }

    // Update texture
    _bubbleDataTexture.SetPixels(bubbleData);
    _bubbleDataTexture.Apply();

    // Update shader properties
    _material.SetFloat(BubbleCountProperty, bubbleCount);
    UpdateShaderProperties();
  }

  private void OnDestroy()
  {
    if (_bubbleDataTexture != null)
    {
      DestroyImmediate(_bubbleDataTexture);
    }
  }
}