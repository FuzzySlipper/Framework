using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public class CollisionCheckForward : IComponent {

        public float RayDistance { get; }
        public Vector3? LastPos = null;

        public CollisionCheckForward(float rayDistance) {
            RayDistance = rayDistance;
        }

        public CollisionCheckForward(SerializationInfo info, StreamingContext context) {
            RayDistance = info.GetValue(nameof(RayDistance), RayDistance);
            LastPos = ((SerializedV3) info.GetValue(nameof(LastPos), typeof(SerializedV3))).Value;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(RayDistance), RayDistance);
            info.AddValue(nameof(LastPos), new SerializedV3(LastPos));
        }
    }
}
