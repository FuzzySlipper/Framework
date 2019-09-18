using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

namespace PixelComrades {
    [System.Serializable]
    public class CachedEntity : IDisposable, ISerializable {

        private int _entityId = -1;
        private Entity _entity;

        public int EntityId { get { return _entityId; } }
        public Entity Entity { 
            get {
                if (_entityId < 0) {
                    return null;
                }
                if (_entity == null || _entity.Id != _entityId) {
                    _entity = EntityController.Get(_entityId);
                }
                return _entity;
            } 
        }

        public void Clear() {
            _entity = null;
            _entityId = -1;
        }
        
        public void Dispose() {
            _entity = null;
        }

        public CachedEntity() {}
        
        public CachedEntity(int id) {
            _entityId = id;
        }

        public void Set(Entity entity) {
            _entity = entity;
            _entityId = entity?.Id ?? -1;
        }

        public static implicit operator Entity(CachedEntity reference) {
            if (reference == null) {
                return null;
            }
            return reference.Entity;
        }

        public CachedEntity(SerializationInfo info, StreamingContext context) {
            _entityId = info.GetValue(nameof(_entityId), _entityId);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_entityId), _entityId);
        }
    }
}
