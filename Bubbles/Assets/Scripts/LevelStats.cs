using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

public class LevelStats : SerializedMonoBehaviour
{
  public static LevelStats Instance { get; private set; }

  [FoldoutGroup("Statistics")]
  [ShowInInspector]
  public readonly Counter BubblesPoppedByClick = new Counter();

  [FoldoutGroup("Statistics")]
  [ShowInInspector]
  public readonly Counter BubblesPopped = new Counter();

  [ShowInInspector]
  public bool HasWon { get; private set; }

  [ShowInInspector]
  public float TimeElapsed { get; private set; }

  [ShowInInspector]
  public float FinalTime { get; private set; }

  [ShowInInspector]
  public float FinalClicks { get; private set; }

  private bool _isInitialized = false;
  private float _initializationDelay = 0.5f; // Wait for bubbles to spawn

  private void Awake()
  {
    // If there is an instance, and it's not me, delete myself.
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    // Make this the singleton instance
    Instance = this;
    TimeElapsed = 0f;
    StartCoroutine(InitializeWithDelay());
  }

  private IEnumerator InitializeWithDelay()
  {
    yield return new WaitForSeconds(_initializationDelay);
    _isInitialized = true;
  }

  private void Update()
  {
    if (!HasWon)
    {
      TimeElapsed += Time.deltaTime;
      if (_isInitialized)
      {
        CheckVictoryCondition();
      }
    }
  }

  private void CheckVictoryCondition()
  {
    bool isWon = false;
    int currentBubbles = Bubble.ActiveBubbles.Count;

    switch (GameRules.Data.WinCondition)
    {
      case WinConditionType.BubblesPopped:
        isWon = BubblesPopped.Value >= GameRules.Data.TargetBubbleCount;
        break;
      case WinConditionType.MinBubblesLeft:
        isWon = currentBubbles >= GameRules.Data.TargetBubbleCount;
        break;
      case WinConditionType.MaxBubblesLeft:
        isWon = currentBubbles <= GameRules.Data.TargetBubbleCount;
        break;
    }

    if (!HasWon && isWon)
    {
      HasWon = true;
      FinalTime = TimeElapsed;
      FinalClicks = BubblesPoppedByClick.Value;
    }
  }

  [Button("Reset Stats")]
  public void ResetAllStats()
  {
    BubblesPoppedByClick.Reset();
    BubblesPopped.Reset();
    HasWon = false;
    TimeElapsed = 0f;
    FinalTime = 0f;
    FinalClicks = 0f;
    _isInitialized = false;
    StartCoroutine(InitializeWithDelay());
  }
}