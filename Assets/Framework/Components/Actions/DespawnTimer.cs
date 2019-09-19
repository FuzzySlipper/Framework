using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class DespawnTimer : IComponent {
        public float Length { get; set; }
        public bool Unscaled { get; }
        public float FinishItem;

        public DespawnTimer(float length, bool unscaled) {
            Length = length;
            Unscaled = unscaled;
        }
        
        public DespawnTimer(SerializationInfo info, StreamingContext context) {
            Length = info.GetValue(nameof(Length), Length);
            Unscaled = info.GetValue(nameof(Unscaled), Unscaled);
            FinishItem = info.GetValue(nameof(FinishItem), FinishItem);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Length), Length);
            info.AddValue(nameof(Unscaled), Unscaled);
            info.AddValue(nameof(FinishItem), FinishItem);
        }
    }
}
