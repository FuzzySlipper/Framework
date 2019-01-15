using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SimplerMover : IComponent {
        public int Owner { get; set; }
        public RotationSpeed RotationSpeed;
        public MoveSpeed MoveSpeed;
        public SimplerMover(Entity owner) {
            Owner = owner;
            RotationSpeed = owner.Get<RotationSpeed>();
            MoveSpeed = owner.Get<MoveSpeed>();
        }
    }
}
