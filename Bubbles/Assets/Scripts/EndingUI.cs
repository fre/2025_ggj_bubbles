using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class EndingUI : MonoBehaviour
{
  private UIDocument _document;
  private Button _backButton;
  private Button _restartButton;

  private void OnEnable()
  {
    // Get the UIDocument component
    _document = GetComponent<UIDocument>();

    // Get the root element from the document
    var root = _document.rootVisualElement;

    // Find and store the buttons
    _backButton = root.Q<Button>("back-button");
    _restartButton = root.Q<Button>("restart-button");

    // Register click events
    _backButton?.RegisterCallback<ClickEvent>(OnBackClicked);
    _restartButton?.RegisterCallback<ClickEvent>(OnRestartClicked);
  }

  private void OnDisable()
  {
    // Always unregister callbacks when disabled
    _backButton?.UnregisterCallback<ClickEvent>(OnBackClicked);
    _restartButton?.UnregisterCallback<ClickEvent>(OnRestartClicked);
  }

  private void OnBackClicked(ClickEvent evt)
  {
    // Go back one scene
    int previousSceneIndex = SceneManager.GetActiveScene().buildIndex - 1;
    if (previousSceneIndex >= 0)
    {
      SceneManager.LoadScene(previousSceneIndex);
    }
  }

  private void OnRestartClicked(ClickEvent evt)
  {
    // Load the first scene
    SceneManager.LoadScene(0);
  }
}