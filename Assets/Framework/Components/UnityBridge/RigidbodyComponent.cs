using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct RigidbodyComponent : IComponent {

        public int Owner { get; set; }
        public Rigidbody Rb;

        public RigidbodyComponent(int owner, Rigidbody rb) {
            Owner = owner;
            Rb = rb;
        }
    }
}
