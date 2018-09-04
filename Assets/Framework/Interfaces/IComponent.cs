using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace PixelComrades {
    //:ISerializable 
    public interface IComponent  {
        int Owner { get; set; }
        //TODO: mandatory Dispose and Serialize
    }

    public class CachedPosition {
        private CachedComponent<TransformComponent> _trComponent;
        private CachedComponent<PositionComponent> _positionComponent;
        private Entity _entity;

        public CachedPosition(Entity owner) {
            _entity = owner;
            if (owner.Get<TransformComponent>().Tr != null) {
                _trComponent = new CachedComponent<TransformComponent>(owner);
            }
            else {
                _positionComponent = new CachedComponent<PositionComponent>(owner);
            }
        }

        public Vector3 Position {
            get {
                if (_trComponent == null && _positionComponent == null) {
                    return Vector3.zero;
                }
                return _trComponent?.c.Tr.position ?? _positionComponent.c.Value.toVector3();
            }
        }

        public void Dispose() {
            _trComponent.Dispose();
            _trComponent = null;
            _positionComponent.Dispose();
            _positionComponent = null;
        }

        public static implicit operator Vector3(CachedPosition reference) {
            return reference.Position;
        }
    }

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

        public void Dispose() {
            _array = null;
            _index = -1;
        }

        public static implicit operator T(CachedComponent<T> reference) {
            return reference.c;
        }
    }
}