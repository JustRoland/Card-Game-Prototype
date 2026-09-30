using System;
using System.Diagnostics;
using UnityEngine.Events;
using UnityEngine;

[Serializable]
public class ObservableProperty<T>
{
    [SerializeField] private T _value;
    [SerializeField] private T _oldValue;

    public ObservableProperty (T value)
    {
        value = _value;
    }

    public T Value
    {
        get => _value;
        set
        {
            if (Equals(_value, value)) return;
            _oldValue = value; _value = value; OnChanged.Invoke(_value);
        }
    }

    public ObservableProperty()
    {

    }

    public UnityEvent<T> OnChanged { get; } = new();
}
