using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class WeaponBobComponent : IComponent {
        
        public Transform ArmsPivot = null;
        public Vector3 ResetPoint;
        public float BobTime = 0;
        [Range(0, 0.1f)] [SerializeField] public float VerticalSwayAmount = 0.025f;
        [Range(0, 0.1f)] [SerializeField] public float HorizontalSwayAmount = 0.075f;
        [Range(0, 15f)] [SerializeField] public float SwaySpeed = 3f;

        public WeaponBobComponent(Transform pivot ) {
            ArmsPivot = pivot;
            ResetPoint = ArmsPivot.localPosition;

        }
        
        public WeaponBobComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
