using UnityEngine;

[ExecuteAlways]
public class Level : MonoBehaviour
{
  [Header("Level Configuration")]
  [SerializeField] private GameRulesData _levelRules;
  public GameRulesData LevelRules => _levelRules;

  private void OnEnable()
  {
    if (_levelRules != null)
    {
      GameRules.RegisterLevelRules(_levelRules);
    }
  }

  private void OnDisable()
  {
    if (_levelRules != null)
    {
      GameRules.UnregisterLevelRules(_levelRules);
    }
  }

  private void OnValidate()
  {
    if (_levelRules != null)
    {
      GameRules.RegisterLevelRules(_levelRules);
    }
  }
}