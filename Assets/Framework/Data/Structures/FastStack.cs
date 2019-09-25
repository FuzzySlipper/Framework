using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {

    public class FastStack<T> : ISerializable {
        private List<T> _list;
        private HashSet<T> _hash;

        public int Count { get { return _list.Count; } }

        public void Add(T value) {
            _list.Add(value);
            _hash.Add(value);
        }

        public void TryAdd(T value) {
            if (_hash.Contains(value)) {
                return;
            }
            _list.Add(value);
            _hash.Add(value);
        }

        public void Remove(T value) {
            _list.Remove(value);
            _hash.Remove(value);
        }

        public void TryRemove(T value) {
            if (!_hash.Contains(value)) {
                return;
            }
            _list.Remove(value);
            _hash.Remove(value);
        }

        public bool Contains(T value) {
            return _hash.Contains(value);
        }

        public T Pop() {
            var element = _list[_list.Count - 1];
            _list.RemoveAt(_list.Count - 1);
            _hash.Remove(element);
            return element;
        }

        public void Clear() {
            _list.Clear();
            _hash.Clear();
        }

        public FastStack() {
            _list = new List<T>();
            _hash = new HashSet<T>();
        }

        public FastStack(int cnt) {
            _list = new List<T>(cnt);
            _hash = new HashSet<T>();
        }

        public FastStack(SerializationInfo info, StreamingContext context) {
            _list = info.GetValue(nameof(_list), _list);
            _hash = info.GetValue(nameof(_hash), _hash);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_list), _list);
            info.AddValue(nameof(_hash), _hash);
        }
    }
}
