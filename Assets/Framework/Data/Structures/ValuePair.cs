using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
    public struct ValuePair<T> {
        public string ID;
        public T Value;

        public ValuePair(string id, T value) {
            ID = id;
            Value = value;
        }
    }

    [System.Serializable]
    public struct ValueTriple<T,TV> {
        public string ID;
        public T Value1;
        public TV Value2;

        public ValueTriple(string id, T value1, TV value2) {
            ID = id;
            Value1 = value1;
            Value2 = value2;
        }
    }

    public class ValuePairCollection<T> {
        private List<ValuePair<T>> _list = new List<ValuePair<T>>();
        
        public int Count { get { return _list.Count; } }
        public ValuePair<T> this[int index] {
            get {
                return _list[index];
            }
        }

        public T GetValue(string id) {
            for (int i = 0; i < _list.Count; i++) {
                if (_list[i].ID == id) {
                    return _list[i].Value;
                }
            }
            return default(T);
        }

        public ValuePairCollection(DataList data, string idField, string valueField) {
            for (int i = 0; i < data.Count; i++) {
                var line = data[i];
                _list.Add(new ValuePair<T>(line.GetValue<string>(idField), line.GetValue<T>(valueField)));
            }
        }
    }

    public class ValueTripleCollection<T,TV> {
        private List<ValueTriple<T,TV>> _list = new List<ValueTriple<T,TV>>();

        public int Count { get { return _list.Count; } }
        public ValueTriple<T,TV> this[int index] { get { return _list[index]; } }

        public ValueTripleCollection(DataList data, string idField, string valueField1, string valueField2) {
            for (int i = 0; i < data.Count; i++) {
                var line = data[i];
                _list.Add(new ValueTriple<T,TV>(line.GetValue<string>(idField), line.GetValue<T>(valueField1), line.GetValue<TV>
                (valueField2)));
            }
        }
    }
}
