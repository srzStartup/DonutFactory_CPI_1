using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class StackManager<T> where T : IStackable
{
    public List<T> _items;

    public List<T> items => _items;
    public bool empty => _items.Count == 0;
    public int Count => _items.Count;
    public int stackCount;

    public StackManager() => _items = new List<T>();

    public T Pop()
    {
        if (_items.Count == 0)
            return default;

        T item =  _items[_items.Count - 1];
        _items.Remove(item);

        return item;
    }

    public void Push(T item)
    {
        _items.Add(item);
    }

    public List<T> FromRange(int index) => _items.GetRange(index, _items.Count - index);

    public T First() => empty ? default : _items[0];

    public T Last() => empty ? default : _items[_items.Count - 1];

    public int IndexOf(T item) => _items.IndexOf(item);

    public void RemoveAt(int index) => _items.RemoveAt(index);

    public void Remove(T item) => _items.Remove(item);

    public void Clear() => _items.Clear();
    public int GetHoldCount()
    {
        return _items.ConvertAll(item => item.holds).Sum();
    }
}

public interface IStackable
{
    public int holds { get; }
}
