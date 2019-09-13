using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class MoveSpeed : IComponent {
        public float Speed;

        public MoveSpeed(float speed) {
            Speed = speed;
        }

        public static implicit operator float(MoveSpeed reference) {
            return reference.Speed;
        }

        public MoveSpeed(SerializationInfo info, StreamingContext context) {
            Speed = info.GetValue(nameof(Speed), Speed);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Speed), Speed);
        }
    }
}
