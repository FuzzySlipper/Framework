using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class RotationSpeed : IComponent {
        public float Speed;

        public RotationSpeed(float speed) {
            Speed = speed;
        }

        public static implicit operator float(RotationSpeed reference) {
            return reference.Speed;
        }

        public RotationSpeed(SerializationInfo info, StreamingContext context) {
            Speed = info.GetValue(nameof(Speed), Speed);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Speed), Speed);
        }
    }
}
