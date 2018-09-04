using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class VelocityMover : IComponent {

        public int Owner { get; set; }
        public TransformComponent Transform;
        public MoveSpeed Speed;
        public RotationSpeed Rotation;
        public float Acceleration;
        public float CurrentSpeed;
        public RigidbodyComponent Rigidbody;
        public MoveTarget Target;
    }
}
