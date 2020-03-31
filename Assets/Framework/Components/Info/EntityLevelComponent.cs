using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class EntityLevelComponent : IComponent {
        public int Value;
        public EntityLevelComponent(){}

        public EntityLevelComponent(int value) {
            Value = value;
        }

        public EntityLevelComponent(SerializationInfo info, StreamingContext context) {
            Value = info.GetValue(nameof(Value), 1);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Value), Value);
        }
    }
}
