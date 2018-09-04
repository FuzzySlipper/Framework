using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class LimitedUses : IComponent {
        public int Owner { get; set; }
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
    }
}
