using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class VelocityMover : IComponent {

        public float CurrentSpeed;

        public VelocityMover(){}
        public VelocityMover(SerializationInfo info, StreamingContext context) {
            CurrentSpeed = info.GetValue(nameof(CurrentSpeed), CurrentSpeed);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(CurrentSpeed), CurrentSpeed);
        }
    }
}
