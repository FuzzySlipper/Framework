using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class ProjectileComponent : IComponent {

        public string Type;

        public ProjectileComponent(string type) {
            Type = type;
        }
        
        public ProjectileComponent(SerializationInfo info, StreamingContext context) {
            Type = info.GetValue(nameof(Type), Type);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Type), Type);
        }
    }
}
