using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class IntroUI : MonoBehaviour
{
  private UIDocument _document;
  private Button _playButton;

  private void OnEnable()
  {
    // Get the UIDocument component
    _document = GetComponent<UIDocument>();

    // Get the root element from the document
    var root = _document.rootVisualElement;

    // Find and store the play button
    _playButton = root.Q<Button>("play-button");

    // Register click event
    _playButton?.RegisterCallback<ClickEvent>(OnPlayClicked);
  }

  private void OnDisable()
  {
    // Always unregister callbacks when disabled
    _playButton?.UnregisterCallback<ClickEvent>(OnPlayClicked);
  }

  private void OnPlayClicked(ClickEvent evt)
  {
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
  }
}