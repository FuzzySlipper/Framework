using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class LimitedUses : IComponent {
        public int Max { get; }
        public int Current { get; private set; }

        public LimitedUses(int max) {
            Max = max;
            Current = max;
        }

        public void Use() {
            Current--;
        }

        public void Recharge() {
            Current = Max;
        }

        public LimitedUses(SerializationInfo info, StreamingContext context) {
            Max = info.GetValue(nameof(Max), Max);
            Current = info.GetValue(nameof(Current), Current);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Max), Max);
            info.AddValue(nameof(Current), Current);
        }
    }
}
