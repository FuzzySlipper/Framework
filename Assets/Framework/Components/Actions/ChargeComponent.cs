using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class ChargeComponent : IComponent {

        public float CurrentCharge;

        public ChargeComponent() {}

        public ChargeComponent(SerializationInfo info, StreamingContext context) {
            CurrentCharge = info.GetValue(nameof(CurrentCharge), CurrentCharge);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(CurrentCharge), CurrentCharge);
        }
    }
}