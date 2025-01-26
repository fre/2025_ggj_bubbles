using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// A simple float-based counter.
/// </summary>
[System.Serializable]
public class Counter
{
  [ShowInInspector, ReadOnly]
  private float _value;
  private readonly float _defaultValue;

  [ShowInInspector]
  public float Value => _value;

  public event System.Action<float> OnValueChanged;

  public Counter(float defaultValue = 0f)
  {
    _defaultValue = defaultValue;
    _value = defaultValue;
  }

  public void Increment()
  {
    _value += 1f;
    OnValueChanged?.Invoke(_value);
  }

  public void Decrement()
  {
    _value -= 1f;
    OnValueChanged?.Invoke(_value);
  }

  public void Add(float amount)
  {
    _value += amount;
    OnValueChanged?.Invoke(_value);
  }

  public void Reset()
  {
    _value = _defaultValue;
    OnValueChanged?.Invoke(_value);
  }

  public void Set(float value)
  {
    _value = value;
    OnValueChanged?.Invoke(_value);
  }

  public override string ToString()
  {
    return _value.ToString("N0");
  }
}