using System;
using System.Collections.Generic;
using PixelComrades;
using UnityEngine;
using Action = System.Action;

[System.Serializable]
public class FloatValueHolder {
    
    public event Action OnResourceChanged;
    private float _value = 0f;
    private int _maxValue = 0;
    private int _minValue = 0;

    public float Value { get { return _value; } }
    public int MaxValue { get => _maxValue; }
    public int MinValue { get => _minValue; }
    public float CurrentPercent { get { return Value / MaxValue; } }

    public void ChangeValue(float newValue) {
        _value = newValue;
        if (_maxValue > 0) {
            _value = Mathf.Clamp(_value, _minValue, _maxValue);
        }
        if (OnResourceChanged != null) {
            OnResourceChanged();
        }
    }

    public void SetLimits(int min, int max) {
        _minValue = min;
        _maxValue = max;
    }

    public void SetMax() {
        _value = _maxValue;
        if (OnResourceChanged != null) {
            OnResourceChanged();
        }
    }

    public void AddToValue(float addChange) {
        _value += addChange;
        if (_maxValue > 0) {
            _value = Mathf.Clamp(_value, _minValue, _maxValue);
        }
        if (OnResourceChanged != null) {
            OnResourceChanged();
        }
    }

    public void ReduceValue(float addChange) {
        _value -= addChange;
        if (_maxValue > 0) {
            _value = Mathf.Clamp(_value, _minValue, _maxValue);
        }
        if (OnResourceChanged != null) {
            OnResourceChanged();
        }
    }

    public static implicit operator float(FloatValueHolder holder) {
        return holder.Value;
    }
}

[System.Serializable]
public class IntValueHolder {
    
    public event Action OnResourceChanged;

    private int _value = 0;
    private int _maxValue = 0;
    private int _minValue = 0;

    public int Value { get { return _value; } }
    public int MaxValue { get => _maxValue; }
    public int MinValue { get => _minValue; }
    public float CurrentPercent { get { return Value / (float) MaxValue; } }

    public void ChangeValue(int newValue) {
        _value = newValue;
        if (_maxValue > 0) {
            _value = Mathf.Clamp(_value, _minValue, _maxValue);
        }
        if (OnResourceChanged != null) {
            OnResourceChanged();
        }
    }

    public void SetLimits(int min, int max) {
        _minValue = min;
        _maxValue = max;
    }

    public void SetMax() {
        _value = _maxValue;
        if (OnResourceChanged != null) {
            OnResourceChanged();
        }
    }

    public void AddToValue(int addChange) {
        _value += addChange;
        if (_maxValue > 0) {
            _value = Mathf.Clamp(_value, _minValue, _maxValue);
        }
        if (OnResourceChanged != null) {
            OnResourceChanged();
        }
    }

    public void ReduceValue(int addChange) {
        _value -= addChange;
        if (_maxValue > 0) {
            _value = Mathf.Clamp(_value, _minValue, _maxValue);
        }
        if (OnResourceChanged != null) {
            OnResourceChanged();
        }
    }

    public static implicit operator int(IntValueHolder holder) {
        return holder.Value;
    }
}

[System.Serializable]
public class IntValueCollection {

    private Dictionary<string, IntValueHolder> _valueCollections = new Dictionary<string, IntValueHolder>();

    public IntValueHolder GetHolder(string id) {
        if (!_valueCollections.TryGetValue(id, out var holder)) {
            holder = new IntValueHolder();
            _valueCollections.Add(id, holder);
        }
        return holder;
    }

    public void ChangeValue(string collection, int newValue) {
        GetHolder(collection).ChangeValue(newValue);
    }

    public void SetLimits(string collection, int min, int max) {
        GetHolder(collection).SetLimits(min, max);
    }

    public void AddToValue(string collection, int addChange) {
        GetHolder(collection).AddToValue(addChange);
    }

    public void ReduceValue(string collection, int addChange) {
        GetHolder(collection).ReduceValue(addChange);
    }
}

[System.Serializable]
public class FloatValueCollection {

    private Dictionary<string, FloatValueHolder> _valueCollections = new Dictionary<string, FloatValueHolder>();

    public FloatValueHolder GetHolder(string id) {
        if (!_valueCollections.TryGetValue(id, out var holder)) {
            holder = new FloatValueHolder();
            _valueCollections.Add(id, holder);
        }
        return holder;
    }

    public void ChangeValue(string collection, int newValue) {
        GetHolder(collection).ChangeValue(newValue);
    }

    public void SetLimits(string collection, int min, int max) {
        GetHolder(collection).SetLimits(min, max);
    }

    public void AddToValue(string collection, int addChange) {
        GetHolder(collection).AddToValue(addChange);
    }

    public void ReduceValue(string collection, int addChange) {
        GetHolder(collection).ReduceValue(addChange);
    }
}

public class FixedSortedArray<T>  {

    private T[] _values;
    private int _index = 0;

    public T[] Values { get { return _values; } }

    public FixedSortedArray(int amt) {
        _values = new T[amt];
    }

    public void Add(T newValue) {
        _values[_index] = newValue;
        _index++;
        if (_index >= _values.Length) {
            _index = 0;
        }
    }

    public bool Contains(T value) {
        for (int i = 0; i < _values.Length; i++) {
            if (value.Equals(_values[i])) {
                return true;
            }
        }
        return false;
    }

    public void Clear() {
        for (int i = 0; i < _values.Length; i++) {
            _values[i] = default(T);
        }
    }
}

[System.Serializable]
public class ValueHolder<T> where T : struct {

    public event Action OnResourceChanged;

    public T Value { get; private set; }
    public T DefaultValue { get; private set; }

    public ValueHolder(T defaultValue) {
        DefaultValue = defaultValue;
        Value = defaultValue;
    }

    public ValueHolder(T defaultValue, System.Action del) {
        DefaultValue = defaultValue;
        Value = defaultValue;
        OnResourceChanged += del;
    }

    private Dictionary<string, T> _dictionary = new Dictionary<string, T>();
    private List<string> _keys = new List<string>();

    public List<string> Keys { get { return _keys; } }

    public string AddValue(T value) {
        SetValue(value);
        var id = System.Guid.NewGuid().ToString();
        _dictionary.Add(id, value);
        _keys.Add(id);
        return id;
    }

    public void AddValue(T value, string id) {
        SetValue(value);
        if (_dictionary.ContainsKey(id)) {
            return;
        }
        _dictionary.Add(id, value);
        _keys.Add(id);
    }

    private void SetValue(T value) {
        var old = Value;
        Value = value;
        if (OnResourceChanged != null && !old.Equals(value)) {
            OnResourceChanged();
        }
    }

    public void RemoveValue(string id) {
        for (int i = 0; i < _keys.Count; i++) {
            if (_keys[i] != id) {
                continue;
            }
            _keys.RemoveAt(i);
            _dictionary.Remove(id);
            if (i >= _keys.Count - 1) {
                SetValue(_keys.Count > 0 ? _dictionary[_keys.LastElement()] : DefaultValue);
            }
            break;
        }
    }

    public void RemoveValue(T val) {
        for (int i = _keys.Count - 1; i >= 0; i--) {
            if (!_dictionary[_keys[i]].Equals(val)) {
                continue;
            }
            var key = _keys[i];
            _keys.RemoveAt(i);
            _dictionary.Remove(key);
            if (i >= _keys.Count - 1) {
                SetValue(_keys.Count > 0 ? _dictionary[_keys.LastElement()] : DefaultValue);
            }
        }
    }

    public void Clear(T newValue) {
        _dictionary.Clear();
        _keys.Clear();
        SetValue(newValue);
    }

    public void Clear() {
        _dictionary.Clear();
        _keys.Clear();
        SetValue(DefaultValue);
    }

    public string Debug() {
        if (_keys.Count == 0) {
            return DefaultValue.ToString();
        }
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < _keys.Count; i++) {
            sb.Append(_keys[i]);
            T val;
            if (_dictionary.TryGetValue(_keys[i], out val)) {
                sb.NewLineAppend(val.ToString());
            }
            else {
                sb.NewLine();
            }
        }
        return sb.ToString();
    }

    public string LastId() {
        return _keys.Count == 0 ? "" : _keys.LastElement();
    }
}


[Serializable]
public class FloatHolder {
    private System.Action<float> _del;
    private float _value;
    public float Value {
        get { return _value; }
        set {
            if (Math.Abs(value - _value) < 0.001f) {
                return;
            }
            _value = value;
            _del(_value);
        }
    }
    public float value { get { return _value; } }

    public FloatHolder(Action<float> del, float value = 0) {
        _del = del;
        _value = value;
    }

    public static implicit operator float(FloatHolder reference) {
        return reference.Value;
    }
}

[Serializable]
public class IntHolder {
    private System.Action<int> _del;
    private int _value;
    public int Value {
        get { return _value; }
        set {
            if (value == _value) {
                return;
            }
            _value = value;
            _del(_value);
        }
    }
    public int value { get { return _value; } }

    public IntHolder(Action<int> del, int value = 0) {
        _del = del;
        _value = value;
    }

    public static implicit operator int(IntHolder reference) {
        return reference.Value;
    }
}