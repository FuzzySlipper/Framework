using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class EntityLevelComponent : IComponent {
        public int Level;

        public EntityLevelComponent(){}

        public EntityLevelComponent(int level) {
            Level = level;
        }

        public EntityLevelComponent(SerializationInfo info, StreamingContext context) {
            Level = info.GetValue(nameof(Level), 1);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Level), Level);
        }
    }
}
