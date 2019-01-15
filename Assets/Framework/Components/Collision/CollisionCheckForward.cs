using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CollisionCheckForward : IComponent {

        public int Owner { get; set; }
        public float RayDistance { get; }
        public Vector3 LastPos { get; set; }

        public CollisionCheckForward(float rayDistance) {
            RayDistance = rayDistance;
        }
    }
}
