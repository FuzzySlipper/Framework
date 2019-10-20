using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public class HealImpact : IComponent {

        public string TargetVital;
        public float NormalizedPercent;
        public bool HealSelf;

        public HealImpact(string targetVital, float normalizedPercent, bool healSelf) {
            TargetVital = targetVital;
            NormalizedPercent = normalizedPercent;
            HealSelf = healSelf;
        }

        public HealImpact(SerializationInfo info, StreamingContext context) {
            HealSelf = info.GetValue(nameof(HealSelf), HealSelf);
            TargetVital = info.GetValue(nameof(TargetVital), TargetVital);
            NormalizedPercent = info.GetValue(nameof(NormalizedPercent), NormalizedPercent);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(HealSelf), HealSelf);
            info.AddValue(nameof(TargetVital), TargetVital);
            info.AddValue(nameof(NormalizedPercent), NormalizedPercent);
        }
    }
}