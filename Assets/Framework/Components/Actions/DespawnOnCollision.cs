using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class DespawnOnCollision : IComponent {

        public DespawnOnCollision() {}

        public DespawnOnCollision(SerializationInfo info, StreamingContext context) {}

        public void GetObjectData(SerializationInfo info, StreamingContext context) {}
    }
}
