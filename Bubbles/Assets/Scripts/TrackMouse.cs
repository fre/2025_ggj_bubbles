using UnityEngine;

public class TrackMouse : MonoBehaviour
{
  [Header("Tracking Settings")]
  [SerializeField] private float _maxForce = 20f;
  [SerializeField] private float _maxSpeed = 10f;
  [SerializeField] private float _slowdownDistance = 2f;

  private Rigidbody2D _rigidbody;
  private Camera _mainCamera;

  private void Awake()
  {
    _rigidbody = GetComponent<Rigidbody2D>();
    _mainCamera = Camera.main;
  }

  private void FixedUpdate()
  {
    Vector2 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
    Vector2 currentPosition = transform.position;

    // Calculate direction and distance to target
    Vector2 toTarget = mousePosition - currentPosition;
    float distance = toTarget.magnitude;

    if (distance > 0.01f)
    {
      // Calculate desired velocity (slower when closer to target)
      float targetSpeed = _maxSpeed;
      if (distance < _slowdownDistance)
      {
        targetSpeed *= distance / _slowdownDistance;
      }

      Vector2 desiredVelocity = toTarget.normalized * targetSpeed;

      // Calculate steering force
      Vector2 steeringForce = desiredVelocity - _rigidbody.linearVelocity;
      steeringForce = Vector2.ClampMagnitude(steeringForce, _maxForce);

      // Apply force
      _rigidbody.AddForce(steeringForce);

      // Limit velocity
      if (_rigidbody.linearVelocity.magnitude > _maxSpeed)
      {
        _rigidbody.linearVelocity = _rigidbody.linearVelocity.normalized * _maxSpeed;
      }
    }
  }
}