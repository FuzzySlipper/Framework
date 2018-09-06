using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct RigidbodyComponent : IComponent {

        public int Owner { get; set; }
        public Rigidbody Rb;

        public RigidbodyComponent(Rigidbody rb) : this() {
            Rb = rb;
        }
    }
}
