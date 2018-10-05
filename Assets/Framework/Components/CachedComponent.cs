using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    
    public class CachedComponent<T> : IDisposable where T: IComponent {
        private int _index = -1;
        private ManagedArray<T> _array;
        private Entity _entity;

        public T c {
            get {
                if (_index < 0) {
                    var cref = EntityController.GetEntity(_entity).GetComponentReference(typeof(T));
                    if (cref != null) {
                        _index = cref.Value.Index;
                        _array = (ManagedArray<T>) cref.Value.Array;
                    }
                }
                return _index < 0 ? default(T) : _array[_index];
            }
        }

        /// <summary>
        /// Only use to assign value types since they can't have receivers
        /// </summary>
        /// <param name="component"></param>
        public void Assign(T component) {
            if (!typeof(T).IsValueType) {
                Debug.LogErrorFormat("Shouldn't assigned non value types like {0}", typeof(T).Name);
                return;
            }
            component.Owner = _entity;
            _array[_index] = component;
        }

        public CachedComponent(){}

        public CachedComponent(Entity owner) {
            _entity = owner;
            _array = EntityController.GetComponentArray<T>();
            var arrRef = EntityController.GetEntity(_entity).GetComponentReference(typeof(T));
            if (arrRef != null) {
                _index = arrRef.Value.Index;
            }
        }

        public CachedComponent(Entity owner, Dictionary<Type, ComponentReference> list) {
            _entity = owner;
            var type = typeof(T);
            if (list.TryGetValue(type, out var cref)) {
                _index = cref.Index;
                _array = (ManagedArray<T>) cref.Array;
            }
        }

        public void Set(Entity owner, Dictionary<Type, ComponentReference> list) {
            _entity = owner;
            var type = typeof(T);
            if (list.TryGetValue(type, out var cref)) {
                _index = cref.Index;
                _array = (ManagedArray<T>) cref.Array;
            }
        }

        public void Clear() {
            _array = null;
            _index = -1;
        }

        public void Dispose() {
            Clear();
        }

        public static implicit operator T(CachedComponent<T> reference) {
            return reference.c;
        }
    }
}
