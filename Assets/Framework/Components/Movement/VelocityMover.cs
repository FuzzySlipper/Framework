using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class VelocityMover : IComponent {

        public int Owner { get; set; }
        public float CurrentSpeed;
    }
}
