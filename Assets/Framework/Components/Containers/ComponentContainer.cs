using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public class ComponentContainer<T> : IComponent where T : IComponent {

        public ComponentContainer(IList<T> values) {
            if (values != null) {
                List.AddRange(values);
            }
        }

        protected List<T> List = new List<T>();

        public int Owner { get; set; }

        public T this[int index] { get { return List[index]; } }
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

        public void Remove(T item) {
            List.Remove(item);
        }
    }

    public class GenericContainer<T> : IComponent {

        public GenericContainer(IList<T> values) {
            if (values != null) {
                List.AddRange(values);
            }
        }

        protected List<T> List = new List<T>();

        public int Owner { get; set; }

        public T this[int index] { get { return List[index]; } }
        public int Count { get { return List.Count; } }

        public void Add(T item) {
            List.Add(item);
        }

        public void Remove(T item) {
            List.Remove(item);
        }
    }
}
