using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelComrades {
    [Serializable]
    public abstract class Template : ScriptableObject {
        protected Dictionary<int, object> Components = new Dictionary<int, object>();

        public void Dispose() {
            //	initialized = false;
            Components.Clear();
        }
        //public bool initialized;

        public void Add<T>(T component) {
            var awakeComponent = component as IAwake;
            if (awakeComponent != null) {
                awakeComponent.OnAwake();
            }
            Components.Add(component.GetType().GetHashCode(), component);
        }

        public object Get(Type t) {
            object obj;
            Components.TryGetValue(t.GetHashCode(), out obj);
            return obj;
        }

        public T Get<T>() where T : class {
            object obj;
            if (Components.TryGetValue(typeof(T).GetHashCode(), out obj)) {
                return (T) obj;
            }
            return null;
        }

        public virtual void Setup() {
        }
    }
}