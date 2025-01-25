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
          _instance = CreateInstance<GameRules>();
        }
      }
      return _instance;
    }
  }

  [SerializeField] private GameRulesData _defaultRules;
  public GameRulesData DefaultRules
  {
    get
    {
      if (_defaultRules == null)
      {
        Debug.LogError("Default GameRulesData not set in GameRules asset!");
        _defaultRules = CreateInstance<GameRulesData>();
      }
      return _defaultRules;
    }
  }

  // Simplified access to rules
  public static GameRulesData Data => Instance.DefaultRules;
}