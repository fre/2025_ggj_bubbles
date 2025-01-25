using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer))]
public class BubbleRenderer : MonoBehaviour
{
  private static readonly int BubbleDataProperty = Shader.PropertyToID("_BubbleData");
  private static readonly int BubbleCountProperty = Shader.PropertyToID("_BubbleCount");
  private static readonly int MaxBubbleCountProperty = Shader.PropertyToID("_MaxBubbleCount");
  private static readonly int OutlineThicknessProperty = Shader.PropertyToID("_OutlineThickness");

  private Material _material;
  private Texture2D _bubbleDataTexture;

  private void Awake()
  {
    _material = GetComponent<MeshRenderer>().material;

    // Create texture for bubble data
    _bubbleDataTexture = new Texture2D(GameRules.Data.MaxBubbles, 1, TextureFormat.RGBAFloat, false);
    _bubbleDataTexture.filterMode = FilterMode.Point;
    _bubbleDataTexture.wrapMode = TextureWrapMode.Clamp;

    // Initialize with empty data
    Color[] initialData = new Color[GameRules.Data.MaxBubbles];
    for (int i = 0; i < GameRules.Data.MaxBubbles; i++)
    {
      initialData[i] = Color.clear;
    }
    _bubbleDataTexture.SetPixels(initialData);
    _bubbleDataTexture.Apply();

    // Assign texture and world space ranges
    _material.SetTexture(BubbleDataProperty, _bubbleDataTexture);
    _material.SetFloat(MaxBubbleCountProperty, GameRules.Data.MaxBubbles);
    _material.SetFloat(OutlineThicknessProperty, GameRules.Data.OutlineThickness);
  }

  private void LateUpdate()
  {
    // Get active bubbles from static list
    var activeBubbles = Bubble.ActiveBubbles;
    int bubbleCount = Mathf.Min(activeBubbles.Count, GameRules.Data.MaxBubbles);
    Color[] bubbleData = new Color[GameRules.Data.MaxBubbles];

    // Update texture data
    for (int i = 0; i < GameRules.Data.MaxBubbles; i++)
    {
      if (i < bubbleCount)
      {
        Vector3 worldPos = activeBubbles[i].transform.position;
        float radius = activeBubbles[i].Size * 0.5f;
        float hue = activeBubbles[i].Hue;

        // Store raw world coordinates
        bubbleData[i] = new Color(worldPos.x, worldPos.y, radius, hue);
      }
      else
      {
        bubbleData[i] = Color.clear;
      }
    }

    // Update texture
    _bubbleDataTexture.SetPixels(bubbleData);
    _bubbleDataTexture.Apply();

    // Update bubble count and outline thickness
    _material.SetFloat(BubbleCountProperty, bubbleCount);
    _material.SetFloat(OutlineThicknessProperty, GameRules.Data.OutlineThickness);
  }

  private void OnDestroy()
  {
    if (_bubbleDataTexture != null)
    {
      Destroy(_bubbleDataTexture);
    }
  }
}