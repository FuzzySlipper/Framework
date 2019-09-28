using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public class ImpactRadius : IComponent, ISerializable {
        public ImpactRadiusTypes Radius { get; }
        public bool LimitToEnemy { get; }

        public ImpactRadius(ImpactRadiusTypes radius, bool limitToEnemy) {
            Radius = radius;
            LimitToEnemy = limitToEnemy;
        }

        public ImpactRadius(SerializationInfo info, StreamingContext context) {
            Radius = info.GetValue(nameof(Radius), Radius);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Radius), Radius);
        }
    }
}
