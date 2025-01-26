using UnityEngine;

public class BubblePopEffect : MonoBehaviour
{
  private Material _material;
  private static readonly int OutlineColorProperty = Shader.PropertyToID("_OutlineColor");
  private static readonly int OutlineThicknessProperty = Shader.PropertyToID("_OutlineThickness");
  private static readonly int ColorProperty = Shader.PropertyToID("_Color");
  private static readonly int HueProperty = Shader.PropertyToID("_Hue");

  public void Initialize(Bubble sourceBubble)
  {
    _material = GetComponent<ParticleSystemRenderer>().material;

    // Set color based on bubble's hue
    Color bubbleColor = Color.HSVToRGB(sourceBubble.Hue, sourceBubble.Saturation, sourceBubble.Value)
      * Mathf.Max(1, GameRules.Data.EdgeOpacity);
    _material.SetColor(ColorProperty, bubbleColor);

    // Set outline parameters from game rules
    _material.SetColor(OutlineColorProperty, GameRules.Data.OutlineColor);
    _material.SetFloat(OutlineThicknessProperty, GameRules.Data.OutlineThickness);
    _material.SetFloat(HueProperty, sourceBubble.Hue);
    _material.SetColor(ColorProperty, bubbleColor);

    transform.localScale = Vector3.one * sourceBubble.Size;

    // Destroy the effect after a short duration
    Destroy(this, 2f);
  }

  private void OnDestroy()
  {
    // Clean up the instantiated material
    if (_material != null)
    {
      Destroy(_material);
    }
  }
}
