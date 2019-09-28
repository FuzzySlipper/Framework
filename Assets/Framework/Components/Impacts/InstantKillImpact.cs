using System.Runtime.Serialization;
using UnityEngine;

namespace PixelComrades {
    [System.Serializable]
    public class InstantKillImpact : IComponent {

        public float Chance;

        public InstantKillImpact(float chance) {
            Chance = chance;
        }

        public InstantKillImpact(SerializationInfo info, StreamingContext context) {
            Chance = info.GetValue(nameof(Chance), Chance);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Chance), Chance);
        }
    }
}