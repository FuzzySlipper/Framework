using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {

    public abstract class CachedComponent : IDisposable {

        public abstract void Clear();
        public abstract void Set(Entity owner, SortedList<Type, ComponentReference> list);

        public void Dispose() {
            Clear();
        }
    }

    public class CachedComponent<T> : CachedComponent, ISerializable where T: IComponent {
        private int _index = -1;
        private int _entity = -1;
        private ManagedArray<T> _array;

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

        public CachedComponent(){}

        public CachedComponent(Entity owner) {
            _entity = owner;
            _array = EntityController.GetComponentArray<T>();
            var arrRef = EntityController.GetEntity(_entity).GetComponentReference(typeof(T));
            if (arrRef != null) {
                _index = arrRef.Value.Index;
            }
        }

        public CachedComponent(Entity owner, SortedList<Type, ComponentReference> list) {
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
            info.AddValue(nameof(_index), _index);
            info.AddValue(nameof(_entity), _entity);
        }

        public void Set(Entity entity) {
            _entity = entity;
            _array = EntityController.GetComponentArray<T>();
            var arrRef = EntityController.GetEntity(_entity).GetComponentReference(typeof(T));
            if (arrRef != null) {
                _index = arrRef.Value.Index;
            }
        }

        public override void Set(Entity owner, SortedList<Type, ComponentReference> list) {
            _entity = owner;
            var type = typeof(T);
            if (list.TryGetValue(type, out var cref)) {
                _index = cref.Index;
                _array = (ManagedArray<T>) cref.Array;
            }
        }

        public override void Clear() {
            _array = null;
            _index = -1;
            _entity = -1;
        }
        
        public static implicit operator T(CachedComponent<T> reference) {
            return reference.c;
        }
    }
}
