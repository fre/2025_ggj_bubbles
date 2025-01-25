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
    if (Input.GetMouseButtonDown(0))  // Left click
    {
      Vector2? worldPoint = GetMouseWorldPoint();
      if (worldPoint.HasValue)
      {
        Bubble.TryPopAtPoint(worldPoint.Value);
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