using System;
using System.Collections;
using System.Collections.Generic;
using PixelComrades;

public interface IShuffleChance {
    int Amount { get; }
}

public class ShuffleBag<T> : IList<T> {

    private List<T> _data = new List<T>();
    private int _cursor = 0;

    /// <summary>
    /// Get the next value from the ShuffleBag
    /// </summary>
    public T Next() {
        if (_cursor < 1) {
            _cursor = _data.Count - 1;
            if (_data.Count < 1) {
                return default(T);
            }

            return _data[0];
        }
        var grab = Game.Random.Next(_cursor + 1);
        //int grab = Mathf.FloorToInt(Random.value * (_cursor + 1));
        T temp = _data[grab];
        _data[grab] = _data[_cursor];
        _data[_cursor] = temp;
        _cursor--;
        return temp;
    }

    //This Constructor will let you do this: ShuffleBag<int> intBag = new ShuffleBag<int>(new int[] {1, 2, 3, 4, 5});
    public ShuffleBag(T[] initalValues) {
        for (int i = 0; i < initalValues.Length; i++) {
            Add(initalValues[i]);
        }
    }

    public ShuffleBag(T[] initalValues, int[] size) {
        for (int i = 0; i < initalValues.Length; i++) {
            if (i > size.Length - 1) {
                Add(initalValues[i]);
                continue;
            }
            Add(initalValues[i], size[i]);
        }
    }

    public void Add(T value, int sizeTotal) {
        for (int sizeCnt = 0; sizeCnt < sizeTotal; sizeCnt++) {
            Add(value);
        }
    }

    public ShuffleBag() { } //Constructor with no values

    public int IndexOf(T item) {
        return _data.IndexOf(item);
    }

    public void Insert(int index, T item) {
        _cursor = _data.Count;
        _data.Insert(index, item);
    }

    public void RemoveAt(int index) {
        _cursor = _data.Count - 2;
        _data.RemoveAt(index);
    }

    public T this[int index] {
        get {
            return _data[index];
        }
        set {
            _data[index] = value;
        }
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
        return _data.GetEnumerator();
    }

    public void Add(T item) {
        _data.Add(item);
        _cursor = _data.Count - 1;
    }

    public int Count {
        get {
            return _data.Count;
        }
    }

    public void Clear() {
        _data.Clear();
    }

    public bool Contains(T item) {
        return _data.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex) {
        foreach (T item in _data) {
            array.SetValue(item, arrayIndex);
            arrayIndex = arrayIndex + 1;
        }
    }

    public bool Remove(T item) {
        _cursor = _data.Count - 2;
        return _data.Remove(item);
    }

    public bool IsReadOnly {
        get {
            return false;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return _data.GetEnumerator();
    }
}

public class ShuffleBagSimple<T> : IList<T> where T : IShuffleChance {

    private List<T> _data = new List<T>();
    private int _cursor = 0;
    //private T _last;

    /// <summary>
    /// Get the next value from the ShuffleBag
    /// </summary>
    public T Next() {
        if (_cursor < 1) {
            _cursor = _data.Count - 1;
            if (_data.Count < 1) {
                return default(T);
            }

            return _data[0];
        }
        var grab = Game.Random.Next(_cursor + 1);
        //int grab = Mathf.FloorToInt(Random.value * (_cursor + 1));
        T temp = _data[grab];
        _data[grab] = _data[_cursor];
        _data[_cursor] = temp;
        _cursor--;
        return temp;
    }

    public ShuffleBagSimple(T[] initalValues) {
        for (int i = 0; i < initalValues.Length; i++) {
            for (int sizeCnt = 0; sizeCnt < initalValues[i].Amount; sizeCnt++) {
                Add(initalValues[i]);
            }
        }
        ValuesCount = initalValues.Length;
    }

    public int ValuesCount { get; private set; }

    public int IndexOf(T item) {
        return _data.IndexOf(item);
    }

    public void Insert(int index, T item) {
        _cursor = _data.Count;
        _data.Insert(index, item);
    }

    public void RemoveAt(int index) {
        _cursor = _data.Count - 2;
        _data.RemoveAt(index);
    }

    public T this[int index] {
        get {
            return _data[index];
        }
        set {
            _data[index] = value;
        }
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
        return _data.GetEnumerator();
    }

    public void Add(T item) {
        _data.Add(item);
        _cursor = _data.Count - 1;
    }

    public int Count {
        get {
            return _data.Count;
        }
    }

    public void Clear() {
        _data.Clear();
    }

    public bool Contains(T item) {
        return _data.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex) {
        foreach (T item in _data) {
            array.SetValue(item, arrayIndex);
            arrayIndex = arrayIndex + 1;
        }
    }

    public bool Remove(T item) {
        _cursor = _data.Count - 2;
        return _data.Remove(item);
    }

    public bool IsReadOnly {
        get {
            return false;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return _data.GetEnumerator();
    }
}