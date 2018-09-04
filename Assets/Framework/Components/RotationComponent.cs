using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class RotationComponent : IComponent {
        public Float4 Rotation;
        public int Owner { get; set; }

        public RotationComponent(Quaternion rotation, int owner) {
            Rotation = rotation;
            Owner = owner;
        }
    }
}
