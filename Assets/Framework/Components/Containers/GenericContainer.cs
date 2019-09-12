using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {

    [System.Serializable]
    public class GenericContainer<T> : IComponent {
        /// <summary>
        /// Warning Add runs before derived class constructor
        /// </summary>
        /// <param name="values"></param>
        public GenericContainer(IList<T> values) {
            if (values != null) {
                AddRange(values);
            }
        }
        public GenericContainer(){}

        protected List<T> List = new List<T>();

        public T this[int index] { get { return List[index]; } }
        public int Count { get { return List.Count; } }

        public virtual void Add(T item) {
            if (item == null) {
                return;
            }
            List.Add(item);
        }

        public virtual void Remove(T item) {
            List.Remove(item);
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
            List = info.GetValue(nameof(List), List);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(List), List);
        }
    }
}
