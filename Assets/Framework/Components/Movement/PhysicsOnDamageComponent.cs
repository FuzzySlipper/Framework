using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class PhysicsOnDamageComponent : IComponent {
        public PhysicsOnDamageComponent(SerializationInfo info, StreamingContext context) {}
        public PhysicsOnDamageComponent() {}

        public void GetObjectData(SerializationInfo info, StreamingContext context) {}
    }
}
