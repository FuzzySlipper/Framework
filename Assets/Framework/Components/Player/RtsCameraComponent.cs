using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class RtsCameraComponent : IComponent {

        public Camera Cam;
        public RtsCameraConfig Config;
        public bool Active = true;
        public Transform FollowTr;
        public Collider CameraLimitSpace;
        public Transform Tr { get; private set; }
        public RtsCameraComponent(Camera cam, RtsCameraConfig config) {
            Cam = cam;
            Config = config;
            Tr = cam.transform;
        }

        public RtsCameraComponent(){}
        
        public RtsCameraComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
