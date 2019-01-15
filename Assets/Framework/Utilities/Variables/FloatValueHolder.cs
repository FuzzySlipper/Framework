using System;
using System.Collections.Generic;
using PixelComrades;
using UnityEngine;

[System.Serializable]
public class FloatValueHolder {
    
    public event Action OnResourceChanged;
    private float _value = 0f;

    public float Value { get { return _value; } }

    public void ChangeValue(float newValue) {
        _value = newValue;
        if (OnResourceChanged != null) {
            OnResourceChanged();
        }
    }

    public void AddToValue(float addChange) {
        _value += addChange;
        if (OnResourceChanged != null) {
            OnResourceChanged();
        }
    }

    public void ReduceValue(int addChange) {
        _value -= addChange;
        if (OnResourceChanged != null) {
            OnResourceChanged();
        }
    }
}

[System.Serializable]
public class IntValueHolder {
    
    public event Action OnResourceChanged;

    private int _value = 0;
    private int _maxValue = 0;
    private int _minValue = 0;

    public int Value { get { return _value; } }

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