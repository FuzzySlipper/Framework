using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class CriticalHitCollider : IComponent {

        public Bounds CriticalHitBounds;
        public Vector3 LocalCenter;

        public bool IsCritical(TransformComponent ownerTr, Vector3 position) {
            if (CriticalHitBounds.size.x <= 0) {
                return false;
            }
            CriticalHitBounds.center = ownerTr.position + LocalCenter;
            return CriticalHitBounds.Contains(position);
        }

        public void Assign(Rect criticalRect, Vector2 size) {
            if (size.x <= 0) {
                CriticalHitBounds = new Bounds(Vector3.zero, Vector3.zero);
                return;
            }
            LocalCenter = new Vector3(Mathf.Lerp(-(size.x * 0.5f), (size.x * 0.5f), criticalRect.x), size.y * criticalRect.y, 0);
            var colliderSize = new Vector3(size.x * size.x,size.y * size.y, 0.5f);
            CriticalHitBounds = new Bounds(LocalCenter, colliderSize);
        }

        public CriticalHitCollider() {
            CriticalHitBounds = new Bounds(Vector3.zero, Vector3.zero);
        }
        
        public CriticalHitCollider(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
