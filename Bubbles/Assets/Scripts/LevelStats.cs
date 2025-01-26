using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LevelStats : SerializedMonoBehaviour
{
  public static LevelStats Instance { get; private set; }

  [FoldoutGroup("Statistics")]
  [ShowInInspector]
  public readonly Counter BubblesPoppedByClick = new Counter();

  [FoldoutGroup("Statistics")]
  [ShowInInspector]
  public readonly Counter BubblesPopped = new Counter();

  [FoldoutGroup("Statistics")]
  [ShowInInspector]
  private Dictionary<int, int> _bubblesByVariant = new Dictionary<int, int>();

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
    UpdateBubbleVariantCounts();
    _isInitialized = true;
  }

  private void Update()
  {
    if (!HasWon)
    {
      TimeElapsed += Time.deltaTime;
      if (_isInitialized)
      {
        UpdateBubbleVariantCounts();
        CheckVictoryCondition();
      }
    }
  }

  private void UpdateBubbleVariantCounts()
  {
    _bubblesByVariant.Clear();
    foreach (var bubble in Bubble.ActiveBubbles)
    {
      if (!_bubblesByVariant.ContainsKey(bubble.Variant))
      {
        _bubblesByVariant[bubble.Variant] = 0;
      }
      _bubblesByVariant[bubble.Variant]++;
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
      case WinConditionType.MinBubblesOfEachVariantLeft:
        isWon = true;
        for (int i = 0; i < GameRules.Data.VariantCount; i++)
        {
          int count = _bubblesByVariant.ContainsKey(i) ? _bubblesByVariant[i] : 0;
          if (count < GameRules.Data.TargetBubbleCount)
          {
            isWon = false;
            break;
          }
        }
        break;
      case WinConditionType.MaxBubblesOfEachVariantLeft:
        isWon = true;
        for (int i = 0; i < GameRules.Data.VariantCount; i++)
        {
          int count = _bubblesByVariant.ContainsKey(i) ? _bubblesByVariant[i] : 0;
          if (count > GameRules.Data.TargetBubbleCount)
          {
            isWon = false;
            break;
          }
        }
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
    _bubblesByVariant.Clear();
    HasWon = false;
    TimeElapsed = 0f;
    FinalTime = 0f;
    FinalClicks = 0f;
    _isInitialized = false;
    StartCoroutine(InitializeWithDelay());
  }

  public int GetBubbleCountForVariant(int variant)
  {
    return _bubblesByVariant.ContainsKey(variant) ? _bubblesByVariant[variant] : 0;
  }
}