using UnityEngine;

[CreateAssetMenu(fileName = "GameRules", menuName = "Bubbles/Game Rules")]
public class GameRules : ScriptableObject
{
  private static GameRules _instance;
  public static GameRules Instance
  {
    get
    {
      if (_instance == null)
      {
        _instance = Resources.Load<GameRules>("GameRules");
        if (_instance == null)
        {
          Debug.LogError("GameRules not found in Resources folder! Please create one.");
        }
      }
      return _instance;
    }
  }

  [SerializeField] private GameRulesData _defaultRules;
  private static GameRulesData _levelRules;

  public static GameRulesData Data => _levelRules != null ? _levelRules : Instance._defaultRules;

  public static void RegisterLevelRules(GameRulesData levelRules)
  {
    _levelRules = levelRules;
  }

  public static void UnregisterLevelRules(GameRulesData levelRules)
  {
    if (_levelRules == levelRules)
    {
      _levelRules = null;
    }
  }

  public static BubbleVariant BubbleVariantData(int variant) => Data.GetVariantData(variant);
}