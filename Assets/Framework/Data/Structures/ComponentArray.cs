using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    
    public interface IComponentArray {
        Entity GetEntity(IComponent component);
        void RemoveByEntity(Entity entity);
    }

    public class ComponentArray<T> : ManagedArray<T>, IComponentArray where T : IComponent {

        private Dictionary<int, int> _entityToIndex = new Dictionary<int, int>();
        private Dictionary<T, int> _componentToEntity = new Dictionary<T, int>();

        public ComponentArray(int initialSize) : base(initialSize) {}
        public ComponentArray() {}

        public ComponentArray(SerializationInfo info, StreamingContext context) : base(info, context) {
            _entityToIndex = info.GetValue(nameof(_entityToIndex), _entityToIndex);
            _componentToEntity = info.GetValue(nameof(_componentToEntity), _componentToEntity);
        }
        
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue(nameof(_entityToIndex), _entityToIndex);
            info.AddValue(nameof(_componentToEntity), _componentToEntity);
        }

        public void Add(Entity entity, T newComponent) {
            if (_entityToIndex.TryGetValue(entity, out var index)) {
                Set(index, newComponent);
            }
            else {
                index = Add(newComponent);
                _entityToIndex.Add(entity, index);
                entity.AddReference(new ComponentReference(index, this));
            }
            _componentToEntity.AddOrUpdate(newComponent, entity);
        }

        public Entity GetEntity(IComponent component) {
            return GetEntity((T) component);
        }

        public Entity GetEntity(T component) {
            if (component == null) {
                return null;
            }
            if (_componentToEntity.TryGetValue(component, out var ent)) {
                return EntityController.GetEntity(ent);
            }
            return null;
        }

        public bool HasComponent(Entity index) {
            return _entityToIndex.ContainsKey(index);
        }

        public bool TryGet(Entity entity, out T value) {
            if (_entityToIndex.TryGetValue(entity, out var index)) {
                value = this[index];
                return true;
            }
            value = default(T);
            return false;
        }

//        public override void Remove(int index) {
//            base.Remove(index);
//        }

        public void RemoveByEntity(Entity entity) {
            if (!_entityToIndex.TryGetValue(entity, out var existing)) {
                return;
            }
            var component = this[existing];
            _componentToEntity.Remove(this[existing]);
            Remove(existing);
            _entityToIndex.Remove(entity);
            entity.Remove(ArrayType);
            if (component != null && component is IDisposable dispose) {
                dispose.Dispose();
            }
        }

        public T Get(Entity entity) {
            if (_entityToIndex.TryGetValue(entity, out var index)) {
                return this[index];
            }
            return default(T);
        }
    }
}
