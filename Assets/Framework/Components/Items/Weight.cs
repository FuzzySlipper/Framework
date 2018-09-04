using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class Weight : IComponent {
        public int Owner { get; set; }
        public float Amount { get; private set; }

        public Weight(float amount) {
            Amount = amount;
        }

        public void Adjust(float newAmount) {
            Amount = newAmount;
        }

        public static implicit operator float(Weight reference) {
            return reference.Amount;
        }
    }
}
