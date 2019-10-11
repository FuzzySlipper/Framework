using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class SpawnPivotComponent : IComponent {

        [SerializeField] private CachedTransform _tr;

        public void SetNewChild(Transform tr) {
            tr.SetParentResetPos(_tr.Tr);
        }
        
        public Vector3 position { get { return _tr.Tr.position; } }
        public Quaternion rotation { get { return _tr.Tr.rotation; } }
        
        public SpawnPivotComponent(Transform tr) {
            _tr = new CachedTransform(tr);
        }
        
        public SpawnPivotComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
