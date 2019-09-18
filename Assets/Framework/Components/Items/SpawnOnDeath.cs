using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class SpawnPrefabOnDeath : IComponent {

        public string Prefab { get; }
        public IntRange CountRange { get; }
        public float Radius { get; }

        public SpawnPrefabOnDeath(string prefab, IntRange count, float radius) {
            Prefab = prefab;
            CountRange = count;
            Radius = radius;
        }

        public SpawnPrefabOnDeath(SerializationInfo info, StreamingContext context) {
            Prefab = info.GetValue(nameof(Prefab), Prefab);
            CountRange = info.GetValue(nameof(CountRange), CountRange);
            Radius = info.GetValue(nameof(Radius), Radius);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Prefab), Prefab);
            info.AddValue(nameof(CountRange), CountRange);
            info.AddValue(nameof(Radius), Radius);
        }
    }

    
    [System.Serializable]
	public sealed class DisableTrOnDeath : IComponent {

        public DisableTrOnDeath() {}

        public DisableTrOnDeath(SerializationInfo info, StreamingContext context) {}

        public void GetObjectData(SerializationInfo info, StreamingContext context) {}
    }
}
