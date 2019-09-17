using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class TransformComponent : IComponent {

        private CachedTransform _cachedTransform;
        public Transform Value { get => _cachedTransform.Tr; }

        public TransformComponent(Transform tr) {
            _cachedTransform = new CachedTransform(tr);
        }

        public void Set(Transform tr) {
            _cachedTransform.Set(tr);
        }
        
        public TransformComponent(SerializationInfo info, StreamingContext context) {
            _cachedTransform = info.GetValue(nameof(_cachedTransform), _cachedTransform);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_cachedTransform), _cachedTransform);
        }

        public static implicit operator Transform(TransformComponent component) {
            if (component == null) {
                return null;
            }
            return component.Value;
        }
    }
}
