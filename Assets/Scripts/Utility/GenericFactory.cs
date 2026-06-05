using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;


public class GenericFactory<T> where T : Object
{
    private readonly int _maxItems;
    private readonly T _prefab;
    private readonly Func<T, bool> _filterPredicate;
    private readonly List<T> _loadedItems = new ();
    public List<T> LoadedItems => _loadedItems;
    
    public GenericFactory(T prefab, Func<T, bool> filterPredicate, int startBuffer = 0, int maxItems = 0 )
    {
        _maxItems = maxItems;
        _prefab = prefab;
        _filterPredicate = filterPredicate;

        for (int i = 0; i < startBuffer; i++)
        {
            CreateNew();
        }
    }

    public T GetItem()
    {
        var loaded = _loadedItems.FirstOrDefault(_filterPredicate);
    
        return loaded ? loaded : CreateNew();
    }
    
    public void UnloadItem(T item, Action action)
    {
        if (!item) return;
        action();
    }

    public void RemoveItem(T item, Action action = null)
    {
        if (!item) return;
        action?.Invoke();
        _loadedItems.Remove(item);
    }
    
    private T CreateNew()
    {
        if (_loadedItems.Count >= _maxItems) return null;
        var newObj = Object.Instantiate(_prefab);
        _loadedItems.Add(newObj);
        return newObj;
    }

}
