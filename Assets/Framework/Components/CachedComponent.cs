using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {

    public abstract class CachedComponent : IDisposable {

        public abstract void Clear();
        public abstract void Set(Entity owner);

        public void Dispose() {
            Clear();
        }
    }

    [System.Serializable]
    public class CachedComponent<T> : CachedComponent, ISerializable where T: IComponent {
        private int _index = -1;
        private int _entity = -1;
        private ManagedArray<T> _array;
        private T _component;
        public T Value {
            get {
                if (_index < 0) {
                    var cref = EntityController.GetEntity(_entity).GetComponentReference(typeof(T));
                    if (cref != null) {
                        _index = cref.Value.Index;
                        _array = (ManagedArray<T>) cref.Value.Array;
                    }
                }
                if (_index < 0 && _component != null) {
                    Set(_component.GetEntity());
                }
                return _index < 0 ? default(T) : _array[_index];
            }
        }
        
        public bool IsValid { get { return _index >= 0; } }

        public CachedComponent(){}

        public CachedComponent(Entity owner) {
            Set(owner);
        }

        public CachedComponent(T component) {
            _component = component;
            Set(component.GetEntity());
        }
        
        public CachedComponent(Entity owner, Dictionary<Type, ComponentReference> list) {
            _entity = owner;
            var type = typeof(T);
            if (list.TryGetValue(type, out var cref)) {
                _index = cref.Index;
                _array = (ManagedArray<T>) cref.Array;
            }
        }

        public CachedComponent(SerializationInfo info, StreamingContext context) {
            _index = info.GetValue(nameof(_index), _index);
            _entity = info.GetValue(nameof(_entity), _entity);
            _array = EntityController.GetComponentArray<T>();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (_index < 0) {
                Set(_component.GetEntity());
            }
            info.AddValue(nameof(_index), _index);
            info.AddValue(nameof(_entity), _entity);
        }

        public override void Set(Entity entity) {
            _entity = entity;
            if (entity == null) {
                ClearEntity();
                return;
            }
            _array = EntityController.GetComponentArray<T>();
            var arrRef = EntityController.GetEntity(_entity).GetComponentReference(typeof(T));
            if (arrRef != null) {
                _index = arrRef.Value.Index;
            }
        }

        public void Set(T component) {
            _component = component;
            Set(component.GetEntity());
        }

        /// <summary>
        /// This can null reference exception as there's no way to ref return null.
        /// Check IsValid before accessing
        /// </summary>
        /// <returns></returns>
        public ref T GetReference() {
            return ref _array[_index];
        }

        public override void Clear() {
            _component = default(T);
            ClearEntity();
        }

        private void ClearEntity() {
            _array = null;
            _index = -1;
            _entity = -1;
        }
        
        public static implicit operator T(CachedComponent<T> reference) {
            if (reference == null) {
                return default(T);
            }
            return reference.Value;
        }
    }
}
