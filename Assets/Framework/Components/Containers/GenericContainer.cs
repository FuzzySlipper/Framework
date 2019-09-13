using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {

    [System.Serializable]
	public sealed class GenericContainer<T> : ISerializable {
       
        public GenericContainer(IList<T> values) {
            if (values != null) {
                AddRange(values);
            }
        }
        public GenericContainer(){}

        private List<T> _list = new List<T>();

        public T this[int index] { get { return _list[index]; } }
        public int Count { get { return _list.Count; } }
        public List<T> List { get { return _list; } }

        public void Add(T item) {
            if (item == null) {
                return;
            }
            _list.Add(item);
        }

        public void Remove(T item) {
            _list.Remove(item);
        }

        public void AddRange(IList<T> values) {
            if (values == null) {
                return;
            }
            for (int i = 0; i < values.Count; i++) {
                Add(values[i]);
            }
        }

        public GenericContainer(SerializationInfo info, StreamingContext context) {
            _list = info.GetValue(nameof(_list), _list);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_list), _list);
        }
    }
}
