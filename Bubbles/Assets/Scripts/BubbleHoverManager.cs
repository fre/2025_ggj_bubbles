using UnityEngine;

public class BubbleHoverManager : MonoBehaviour
{
  private Bubble _currentlyHoveredBubble;
  private void Update()
  {
    // Convert mouse position to world space
    Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

    // Find closest bubble to mouse
    Bubble closestBubble = Bubble.FindClosestBubbleTo(mouseWorldPos);

    // Check if we're hovering over a bubble (distance < 1 means inside the bubble's radius)
    if (closestBubble != null && closestBubble.DistanceToPoint(mouseWorldPos) < 1)
    {
      // If we're hovering a new bubble
      if (_currentlyHoveredBubble != closestBubble)
      {
        // Unhover previous bubble
        if (_currentlyHoveredBubble != null)
        {
          _currentlyHoveredBubble.SetHovered(false);
        }

        // Hover new bubble
        _currentlyHoveredBubble = closestBubble;
        _currentlyHoveredBubble.SetHovered(true);
      }
    }
    else if (_currentlyHoveredBubble != null)
    {
      // If we're not hovering any bubble, unhover the previous one
      _currentlyHoveredBubble.SetHovered(false);
      _currentlyHoveredBubble = null;
    }
  }
}