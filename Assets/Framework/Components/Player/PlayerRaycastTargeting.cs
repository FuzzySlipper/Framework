using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class PlayerRaycastTargeting : IComponent {
        
        public PlayerRaycastTargeting(){}
        
        public PlayerRaycastTargeting(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
