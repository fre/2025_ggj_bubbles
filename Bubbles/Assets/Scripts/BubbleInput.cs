using UnityEngine;

public class BubbleInput : MonoBehaviour
{
  private Camera _mainCamera;
  private static readonly Plane _gamePlane = new Plane(Vector3.forward, 0);

  private void Start()
  {
    _mainCamera = Camera.main;
  }

  private void Update()
  {
    // Check for mouse button being held
    if (Input.GetMouseButton(0))  // Changed from GetMouseButtonDown to GetMouseButton
    {
      Vector2? worldPoint = GetMouseWorldPoint();
      if (worldPoint.HasValue)
      {
        if (Bubble.TryPopAtPoint(worldPoint.Value))
        {
          LevelStats.Instance.BubblesPoppedByClick.Increment();
        }
      }
    }
  }

  private Vector2? GetMouseWorldPoint()
  {
    Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
    if (_gamePlane.Raycast(ray, out float distance))
    {
      Vector3 worldPoint = ray.GetPoint(distance);
      return new Vector2(worldPoint.x, worldPoint.y);
    }
    return null;
  }
}