using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class SimplerMover : IComponent {
        public CachedComponent<RotationSpeed> RotationSpeed;
        public CachedComponent<MoveSpeed> MoveSpeed;
        public SimplerMover(Entity owner) {
            RotationSpeed = new CachedComponent<RotationSpeed>(owner);
            MoveSpeed = new CachedComponent<MoveSpeed>(owner);
        }

        public SimplerMover(SerializationInfo info, StreamingContext context) {
            RotationSpeed = info.GetValue(nameof(RotationSpeed), RotationSpeed);
            MoveSpeed = info.GetValue(nameof(MoveSpeed), MoveSpeed);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(RotationSpeed), RotationSpeed);
            info.AddValue(nameof(MoveSpeed), MoveSpeed);
        }
    }
}
