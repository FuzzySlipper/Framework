using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class SpriteColliderComponent : IComponent, IDisposable {
        
        private CachedUnityComponent<SpriteCollider> _spriteCollider;
        
        public SpriteCollider Value { get => _spriteCollider; }

        public SpriteColliderComponent(SpriteCollider spriteCollider) {
            _spriteCollider = new CachedUnityComponent<SpriteCollider>(spriteCollider);

        }
        
        public SpriteColliderComponent(SerializationInfo info, StreamingContext context) {
            _spriteCollider = info.GetValue(nameof(_spriteCollider), _spriteCollider);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_spriteCollider), _spriteCollider);
        }

        public void Dispose() {
            _spriteCollider?.Dispose();
            _spriteCollider = null;
        }
    }
}
