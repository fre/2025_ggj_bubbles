using UnityEngine;

public class Level : MonoBehaviour
{
  [Header("Level Configuration")]
  [SerializeField] private GameRulesData _levelRules;
  public GameRulesData LevelRules => _levelRules;

  private void OnEnable()
  {
    GameRules.RegisterLevelRules(_levelRules);
  }

  private void OnDisable()
  {
    GameRules.UnregisterLevelRules(_levelRules);
  }
}