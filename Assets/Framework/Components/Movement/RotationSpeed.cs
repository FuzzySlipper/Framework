using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class RotationSpeed : IComponent {
        public int Owner { get; set; }
        public float Speed;

        public RotationSpeed(int owner, float speed) {
            Owner = owner;
            Speed = speed;
        }

        public static implicit operator float(RotationSpeed reference) {
            return reference.Speed;
        }
    }
}
