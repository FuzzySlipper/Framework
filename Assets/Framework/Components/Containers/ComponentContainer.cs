using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public class ComponentContainer<T> : IComponent where T : class, IComponent{

        /// <summary>
        /// Warning Add runs before derived class constructor
        /// </summary>
        /// <param name="values"></param>
        public ComponentContainer(IList<T> values) {
            if (values != null) {
                for (int i = 0; i < values.Count; i++) {
                    Add(values[i]);
                }
            }
        }

        protected List<T> List = new List<T>();
        private int _owner;
        public int Owner {
            get {
                return _owner;
            }
            set {
                if (_owner == value) {
                    return;
                }
                _owner = value;
                OwnerChanged();
            }
        }

        public T this[int index] {
            get {
                if (!List.HasIndex(index)) {
                    return null;
                }
                return List[index];
            }
        }
        public int Count { get { return List.Count; } }


        public virtual void Add(T item) {
            if (item.Owner < 0) {
                item.Owner = Owner;
            }
            List.Add(item);
        }

        public void AddRange(IList<T> items) {
            for (int i = 0; i < items.Count; i++) {
                Add(items[i]);
            }
        }

        public virtual void Remove(T item) {
            List.Remove(item);
        }

        protected virtual void OwnerChanged() {
            for (int i = 0; i < List.Count; i++) {
                List[i].Owner = _owner;
            }
        }
    }

    public class GenericContainer<T> : IComponent {
        /// <summary>
        /// Warning Add runs before derived class constructor
        /// </summary>
        /// <param name="values"></param>
        public GenericContainer(IList<T> values) {
            if (values != null) {
                for (int i = 0; i < values.Count; i++) {
                    Add(values[i]);
                }
            }
        }

        protected List<T> List = new List<T>();

        private int _owner;
        public int Owner {
            get { return _owner; }
            set {
                if (_owner == value) {
                    return;
                }
                _owner = value;
                OwnerChanged();
            }
        }

        public T this[int index] { get { return List[index]; } }
        public int Count { get { return List.Count; } }

        public virtual void Add(T item) {
            List.Add(item);
        }

        public virtual void Remove(T item) {
            List.Remove(item);
        }

        protected virtual void OwnerChanged() {}
    }
}
