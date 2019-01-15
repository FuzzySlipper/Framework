using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

        private int _owner = -1;
        public int Owner {
            get { return _owner; }
            set {
                if (_owner == value) {
                    return;
                }
                var oldOwner = value;
                _owner = value;
                OwnerChanged(oldOwner);
            }
        }

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

        protected virtual void OwnerChanged(int old) {
            for (int i = 0; i < List.Count; i++) {
                var item = List[i] as IComponent;
                if (item?.Owner == old) {
                    item.Owner = _owner;
                }
            }
        }
    }
}
