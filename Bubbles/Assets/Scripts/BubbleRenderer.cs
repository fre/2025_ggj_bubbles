using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer))]
public class BubbleRenderer : MonoBehaviour
{
  private static readonly int BubbleDataProperty = Shader.PropertyToID("BubbleData");
  private static readonly int BubbleCountProperty = Shader.PropertyToID("BubbleCount");

  private Material _material;
  private Texture2D _bubbleDataTexture;
  private List<Bubble> _activeBubbles = new List<Bubble>();

  public int MaxBubbles = 100;

  private void Awake()
  {
    _material = GetComponent<MeshRenderer>().material;

    // Create texture for bubble data
    _bubbleDataTexture = new Texture2D(MaxBubbles, 1, TextureFormat.RGBAFloat, false);
    _bubbleDataTexture.filterMode = FilterMode.Point;
    _bubbleDataTexture.wrapMode = TextureWrapMode.Clamp;

    // Initialize with empty data
    Color[] initialData = new Color[MaxBubbles];
    for (int i = 0; i < MaxBubbles; i++)
    {
      initialData[i] = Color.clear;
    }
    _bubbleDataTexture.SetPixels(initialData);
    _bubbleDataTexture.Apply();

    // Assign texture and world space ranges
    _material.SetTexture(BubbleDataProperty, _bubbleDataTexture);
  }

  private void LateUpdate()
  {
    // Find all active bubbles
    _activeBubbles.Clear();
    _activeBubbles.AddRange(FindObjectsOfType<Bubble>());

    // Update bubble data
    int bubbleCount = Mathf.Min(_activeBubbles.Count, MaxBubbles);
    Color[] bubbleData = new Color[MaxBubbles];

    // Update texture data
    for (int i = 0; i < MaxBubbles; i++)
    {
      if (i < bubbleCount)
      {
        Vector3 worldPos = _activeBubbles[i].transform.position;
        float radius = _activeBubbles[i].BubbleRadius;
        float hue = _activeBubbles[i].Hue;

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

    // Update bubble count
    _material.SetFloat(BubbleCountProperty, bubbleCount);
  }

  private void OnDestroy()
  {
    if (_bubbleDataTexture != null)
    {
      Destroy(_bubbleDataTexture);
    }
  }
}