using UnityEngine;

[CreateAssetMenu(fileName = "GameRulesData", menuName = "Bubbles/Game Rules Data")]
public class GameRulesData : ScriptableObject
{
  [Header("Global")]
  public int MaxBubbles = 100;
  public Vector2 WorldSize = new Vector2(20f, 12f);

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
  public float PopForce = 10f;
  public float PopRadius = 3f;

  [Header("Rendering")]
  public float WobbleSpeed = 2f;
  public float WobbleAmount = 0.1f;
  public float ColorIntensity = 1f;
  public float BubbleTransparency = 0.8f;
  public float OutlineThickness = 0.05f;
}