using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public class DamageImpact : IComponent {

        public string DamageType;
        public string TargetVital;
        public float NormalizedPercent;
        
        public DamageImpact(string damageType, string targetVital, float normalizedPercent) {
            DamageType = damageType;
            TargetVital = targetVital;
            NormalizedPercent = normalizedPercent;
        }

        public DamageImpact(SerializationInfo info, StreamingContext context) {
            DamageType = info.GetValue(nameof(DamageType), DamageType);
            TargetVital = info.GetValue(nameof(TargetVital), TargetVital);
            NormalizedPercent = info.GetValue(nameof(NormalizedPercent), NormalizedPercent);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(DamageType), DamageType);
            info.AddValue(nameof(TargetVital), TargetVital);
            info.AddValue(nameof(NormalizedPercent), NormalizedPercent);
        }
    }
}
