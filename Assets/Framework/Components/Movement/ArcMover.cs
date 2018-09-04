using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ArcMover : IComponent {

        public int Owner { get; set; }
        public float Angle;

        public Vector3 MoveVector;
        public float ElapsedTime;
        public float Duration;

        public TransformComponent Transform;
        public MoveSpeed Speed;

        public ArcMover(Entity owner, float angle = 15) {
            Owner = owner;
            Angle = angle;
            Transform = owner.Get<TransformComponent>();
            Speed = owner.Get<MoveSpeed>();
        }

        
    }
}
