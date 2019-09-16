using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class DamageComponent : IComponent {

        public DamageComponent(){}
        public DamageComponent(SerializationInfo info, StreamingContext context) {}
        public void GetObjectData(SerializationInfo info, StreamingContext context) {}
    }
}
