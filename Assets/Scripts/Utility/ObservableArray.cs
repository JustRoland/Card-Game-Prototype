using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public interface IObservableArray<T>
{
    event Action<T[]> OnAnyValueChanged;
        
    int Count { get; }
    T this[int index] { get; }
        
    void Swap(int index1, int index2);
    void Clear();
    bool TryAdd(T item);
    bool TryRemove(T item);
}

[Serializable]
public class ObservableArray<T> : IObservableArray<T>
{
    private T[] _array;
    
    public event Action<T[]> OnAnyValueChanged = delegate { };
    public int Count => _array.Count(i => i != null);
    public T this[int index] => _array[index];

    public ObservableArray(int capacity = 10,  IList<T> initialList = null)
    {
        _array = new T[capacity];
        if (initialList != null)
        {
            initialList.Take(capacity).ToArray().CopyTo(_array, 0);
            Invoke();
        }
    }
    
    void Invoke() => OnAnyValueChanged.Invoke(_array);
    
    public void Swap(int index1, int index2)
    {
        (_array[index1], _array[index2]) = (_array[index2], _array[index1]);
        Invoke();
    }

    public void Clear()
    {
        _array = new T[_array.Length];
    }

    public bool TryAdd(T item)
    {
        for (int i = 0; i < _array.Length; i++)
        {
            if (_array[i] != null) continue;
            _array[i] = item;
            Invoke();
            return true;
        }
        return false;
    }

    public bool TryRemove(T item)
    {
        for (int i = 0; i < _array.Length; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(_array[i], item)) continue;
            _array[i] = default;
            Invoke();
            return true;
        }
        return false;
    }
}

