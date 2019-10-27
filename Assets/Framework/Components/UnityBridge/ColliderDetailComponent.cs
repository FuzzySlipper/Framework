using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class ColliderDetailComponent : IComponent {

        private CachedUnityComponent<MeshCollider> _component;
        public MeshCollider Collider { get { return _component.Value; } }

        public ColliderDetailComponent(MeshCollider collider) {
            _component = new CachedUnityComponent<MeshCollider>(collider);
        }
        
        public ColliderDetailComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
