using UnityEngine;
using System.IO;

[RequireComponent(typeof(MeshRenderer))]
public class TestBubblePattern : MonoBehaviour
{
  public int BubbleCount = 100;
  public int MaxBubbleCount = 100;
  private Material _material;
  private Texture2D _bubbleDataTexture;

  // World space ranges (for bubble generation only)
  [Header("World Space Settings")]
  public float MinWorldPos = -10f;
  public float MaxWorldPos = 10f;
  public float MinRadius = 0.4f;
  public float MaxRadius = 1.5f;

  private void Start()
  {
    _material = GetComponent<MeshRenderer>().material;
    Debug.Log($"Material: {_material.name}, Shader: {_material.shader.name}");

    // Create texture for bubble data
    _bubbleDataTexture = new Texture2D(MaxBubbleCount, 1, TextureFormat.RGBAFloat, false);
    _bubbleDataTexture.filterMode = FilterMode.Point;
    _bubbleDataTexture.wrapMode = TextureWrapMode.Clamp;

    GenerateRandomBubbles();

    // Save texture to file for inspection
    byte[] bytes = _bubbleDataTexture.EncodeToPNG();
    string filePath = Path.Combine(Application.dataPath, "bubble_data_texture.png");
    File.WriteAllBytes(filePath, bytes);
    Debug.Log($"Saved texture to: {filePath}");

    UpdateShaderProperties();
  }

  private void UpdateShaderProperties()
  {
    // Assign to shader
    _material.SetTexture("_BubbleData", _bubbleDataTexture);
    _material.SetFloat("_BubbleCount", BubbleCount);
  }

  private void GenerateRandomBubbles()
  {
    Color[] bubbleData = new Color[MaxBubbleCount];

    // Create bubbles with world space coordinates
    for (int i = 0; i < BubbleCount; i++)
    {
      // Generate world space values - use full range
      float worldX = Random.Range(MinWorldPos, MaxWorldPos);
      float worldY = Random.Range(MinWorldPos, MaxWorldPos);
      float radius = Random.Range(MinRadius, MaxRadius);
      float hue = Random.Range(0f, 1f);

      // Store raw world coordinates
      bubbleData[i] = new Color(worldX, worldY, radius, hue);
    }

    // Update texture
    _bubbleDataTexture.SetPixels(bubbleData);
    _bubbleDataTexture.Apply();
  }

  private void OnDestroy()
  {
    if (_bubbleDataTexture != null)
    {
      Destroy(_bubbleDataTexture);
    }
  }

  private void Update()
  {
    // Update shader properties every frame in case they changed in the editor
    UpdateShaderProperties();

    // Press Space to generate new random pattern
    if (Input.GetKeyDown(KeyCode.Space))
    {
      GenerateRandomBubbles();
    }
  }
}