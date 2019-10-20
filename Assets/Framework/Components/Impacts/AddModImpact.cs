using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class AddModImpact : IComponent {

        public float Length;
        public string TargetStat;
        public float NormalizedPercent;
        public Sprite Icon;
        public string ID;
        private string _iconLocation;

        public AddModImpact(float length, string targetStat, float normalizedPercent, IconComponent icon) {
            Length = length;
            TargetStat = targetStat;
            NormalizedPercent = normalizedPercent;
            Icon = icon;
            _iconLocation = icon?.IconLocation;
            ID = System.Guid.NewGuid().ToString();
        }

        public AddModImpact(SerializationInfo info, StreamingContext context) {
            Length = info.GetValue(nameof(Length), Length);
            TargetStat = info.GetValue(nameof(TargetStat), TargetStat);
            NormalizedPercent = info.GetValue(nameof(NormalizedPercent), NormalizedPercent);
            ID = info.GetValue(nameof(ID), ID);
            _iconLocation = info.GetValue(nameof(_iconLocation), _iconLocation);
            ItemPool.LoadAsset<Sprite>(_iconLocation, a => Icon = a);
        }
        
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Length), Length);
            info.AddValue(nameof(TargetStat), TargetStat);
            info.AddValue(nameof(NormalizedPercent), NormalizedPercent);
            info.AddValue(nameof(ID), ID);
            info.AddValue(nameof(_iconLocation), _iconLocation);
        }
    }
}