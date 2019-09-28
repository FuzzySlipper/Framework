using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    
    
    [System.Serializable]
    public class ApplyTagImpact : IComponent {

        public int Tag { get; }
        public float Chance { get; }
        public float Length { get; }
        public string Description { get; }

        public ApplyTagImpact(int tag, float chance, float length, string description) {
            Tag = tag;
            Chance = chance;
            Length = length;
            Description = description;
        }

        public ApplyTagImpact(SerializationInfo info, StreamingContext context) {
            Length = info.GetValue(nameof(Length), Length);
            Chance = info.GetValue(nameof(Chance), Chance);
            Description = info.GetValue(nameof(Description), Description);
            Tag = info.GetValue(nameof(Tag), Tag);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Length), Length);
            info.AddValue(nameof(Chance), Chance);
            info.AddValue(nameof(Description), Description);
            info.AddValue(nameof(Tag), Tag);
        }
    }
}
