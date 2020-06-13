using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class PlayerCameraComponent : IComponent {

        public Transform FollowTr;
        public Camera Cam;
        
        public Transform CamTr;
        public Spring MoveSpring = new Spring();
        public Spring RotationSpring = new Spring();
        public Spring FovSpring = new Spring();
        public float RotationX, RotationY;
        public float OriginalFov = 55;
        public bool Active = true;
        
        public PlayerCameraComponent(Transform followTr, Camera cam) {
            FollowTr = followTr;
            Cam = cam;
            CamTr = cam.transform;
            OriginalFov = cam.fieldOfView;
        }

        public PlayerCameraComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
