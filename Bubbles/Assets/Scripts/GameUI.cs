using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(UIDocument))]
public class GameUI : MonoBehaviour
{
  private UIDocument _document;
  private Label _bubblesClicked;
  private Label _bubblesTotal;
  private Label _bubblesLeft;
  private Label _bubblesTarget;
  private Label _winnerText;
  private Label _timeCount;
  private Label _levelText;
  private Button _prevButton;
  private Button _nextButton;
  private Button _resetButton;
  private VisualElement _timeContainer;
  private VisualElement _clicksContainer;
  private VisualElement _poppedContainer;
  private VisualElement _leftContainer;

  private void Awake()
  {
    _document = GetComponent<UIDocument>();
  }

  private void OnEnable()
  {
    var root = _document.rootVisualElement;
    _bubblesClicked = root.Q<Label>("bubbles-clicked-count");
    _bubblesTotal = root.Q<Label>("bubbles-total-count");
    _bubblesLeft = root.Q<Label>("bubbles-left-count");
    _bubblesTarget = root.Q<Label>("bubbles-target-count");
    _winnerText = root.Q<Label>("winner-text");
    _timeCount = root.Q<Label>("time-count");
    _levelText = root.Q<Label>("level-text");

    _timeContainer = root.Q<VisualElement>("time-container");
    _clicksContainer = root.Q<VisualElement>("clicks-container");
    _poppedContainer = root.Q<VisualElement>("popped-container");
    _leftContainer = root.Q<VisualElement>("left-container");

    // Get navigation buttons
    _prevButton = root.Q<Button>("prev-button");
    _nextButton = root.Q<Button>("next-button");
    _resetButton = root.Q<Button>("reset-button");

    // Wait for next frame to ensure LevelStats is initialized
    StartCoroutine(InitializeStats());
  }

  private IEnumerator InitializeStats()
  {
    // Wait until LevelStats.Instance is available
    yield return new WaitUntil(() => LevelStats.Instance != null);

    // Subscribe to events
    LevelStats.Instance.BubblesPoppedByClick.OnValueChanged += UpdateClickedCount;
    LevelStats.Instance.BubblesPopped.OnValueChanged += UpdateTotalCount;

    // Subscribe to button clicks
    if (_resetButton != null) _resetButton.clicked += OnResetClicked;
    if (_prevButton != null) _prevButton.clicked += OnPrevClicked;
    if (_nextButton != null) _nextButton.clicked += OnNextClicked;

    // Initialize UI state
    UpdateUIState();
    UpdateTargetCount();
    UpdateWinnerText();
    UpdateLevelText();
    UpdateNavigationButtons();
  }

  private void Update()
  {
    UpdateTimeDisplay();
    UpdateBubblesLeft();
    UpdateWinnerText();
  }

  private void OnDisable()
  {
    if (LevelStats.Instance != null)
    {
      LevelStats.Instance.BubblesPoppedByClick.OnValueChanged -= UpdateClickedCount;
      LevelStats.Instance.BubblesPopped.OnValueChanged -= UpdateTotalCount;
    }

    // Unsubscribe from button clicks - using stored references
    if (_resetButton != null) _resetButton.clicked -= OnResetClicked;
    if (_prevButton != null) _prevButton.clicked -= OnPrevClicked;
    if (_nextButton != null) _nextButton.clicked -= OnNextClicked;
  }

  private void UpdateUIState()
  {
    bool isPoppedCondition = GameRules.Data.WinCondition == WinConditionType.BubblesPopped;
    bool isTimeSuccess = GameRules.Data.SuccessMeasure == SuccessMeasure.Time;

    // Show/hide relevant containers
    _poppedContainer.style.display = isPoppedCondition ? DisplayStyle.Flex : DisplayStyle.None;
    _leftContainer.style.display = !isPoppedCondition ? DisplayStyle.Flex : DisplayStyle.None;
    _timeContainer.style.display = isTimeSuccess ? DisplayStyle.Flex : DisplayStyle.None;
    _clicksContainer.style.display = !isTimeSuccess ? DisplayStyle.Flex : DisplayStyle.None;
  }

  private void UpdateTimeDisplay()
  {
    if (_timeCount != null)
    {
      _timeCount.text = $"{LevelStats.Instance.TimeElapsed:F1}s";
    }
  }

  private void UpdateClickedCount(float value)
  {
    if (_bubblesClicked != null)
    {
      _bubblesClicked.text = value.ToString("N0");
    }
  }

  private void UpdateTotalCount(float value)
  {
    if (_bubblesTotal != null)
    {
      _bubblesTotal.text = value.ToString("N0");
    }
  }

  private void UpdateBubblesLeft()
  {
    if (_bubblesLeft != null)
    {
      _bubblesLeft.text = Bubble.ActiveBubbles.Count.ToString("N0");
    }
  }

  private void UpdateTargetCount()
  {
    if (_bubblesTarget != null)
    {
      _bubblesTarget.text = GameRules.Data.TargetBubbleCount.ToString("N0");
    }
  }

  private void UpdateWinnerText()
  {
    if (_winnerText != null)
    {
      bool hasWon = LevelStats.Instance.HasWon;
      _winnerText.style.display = hasWon ? DisplayStyle.Flex : DisplayStyle.None;

      if (hasWon)
      {
        // Highlight the success measure in gold
        bool isTimeSuccess = GameRules.Data.SuccessMeasure == SuccessMeasure.Time;
        if (isTimeSuccess)
        {
          _timeCount.style.color = new StyleColor(new Color(1f, 0.843f, 0f)); // Gold
          _timeCount.text = $"{LevelStats.Instance.FinalTime:F1}s";
        }
        else
        {
          _bubblesClicked.style.color = new StyleColor(new Color(1f, 0.843f, 0f)); // Gold
          _bubblesClicked.text = LevelStats.Instance.FinalClicks.ToString("N0");
        }

        _winnerText.text = $"WINNER!";
      }
    }
  }

  private void UpdateLevelText()
  {
    if (_levelText != null)
    {
      int currentLevel = SceneManager.GetActiveScene().buildIndex + 1;
      _levelText.text = $"{currentLevel}";
    }
  }

  private void UpdateNavigationButtons()
  {
    if (_prevButton != null && _nextButton != null)
    {
      int currentLevel = SceneManager.GetActiveScene().buildIndex;
      int sceneCount = SceneManager.sceneCountInBuildSettings;

      _prevButton.SetEnabled(currentLevel > 0);
      _nextButton.SetEnabled(currentLevel < sceneCount - 1);

      // Optional: Hide buttons if they're not usable
      _prevButton.style.display = currentLevel > 0 ? DisplayStyle.Flex : DisplayStyle.None;
      _nextButton.style.display = currentLevel < sceneCount - 1 ? DisplayStyle.Flex : DisplayStyle.None;
    }
  }

  private void OnPrevClicked()
  {
    int currentLevel = SceneManager.GetActiveScene().buildIndex;
    if (currentLevel > 0)
    {
      SceneManager.LoadScene(currentLevel - 1);
    }
  }

  private void OnNextClicked()
  {
    int currentLevel = SceneManager.GetActiveScene().buildIndex;
    int sceneCount = SceneManager.sceneCountInBuildSettings;
    if (currentLevel < sceneCount - 1)
    {
      SceneManager.LoadScene(currentLevel + 1);
    }
  }

  private void OnResetClicked()
  {
    LevelStats.Instance.ResetAllStats();

    // Reset UI colors
    if (_timeCount != null) _timeCount.style.color = Color.white;
    if (_bubblesClicked != null) _bubblesClicked.style.color = Color.white;

    // Reload current scene
    Scene currentScene = SceneManager.GetActiveScene();
    SceneManager.LoadScene(currentScene.name);
  }
}