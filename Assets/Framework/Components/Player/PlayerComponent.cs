using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class PlayerComponent : IComponent {
        public PlayerComponent(SerializationInfo info, StreamingContext context) {
        }

        public PlayerComponent() {}

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
        }
    }
}
