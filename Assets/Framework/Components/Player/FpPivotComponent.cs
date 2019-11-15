using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class FpPivotComponent : IComponent {

        private CachedTransform _tr;
        
        public Transform Tr { get { return _tr; } }

        public FpPivotComponent(Transform tr) {
            _tr = new CachedTransform(tr);
        }

        public FpPivotComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
