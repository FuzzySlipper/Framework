using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelComrades {
    public class PositionComponent : IComponent {
        public Float3 Value;
        public int Owner { get; set; }

        public PositionComponent(Float3 value, int owner) {
            Value = value;
            Owner = owner;
        }

        public static implicit operator Float3(PositionComponent reference) {
            return reference.Value;
        }

        public static implicit operator Vector3(PositionComponent reference) {
            return reference.Value.toVector3();
        }
    }
}
