using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ArcMover : ComponentBase {

        public float Angle;

        public Vector3 MoveVector;
        public float ElapsedTime;
        public float Duration;

        public MoveSpeed Speed;

        public ArcMover(Entity owner, float angle = 15) {
            Owner = owner;
            Angle = angle;
            Speed = owner.Get<MoveSpeed>();
        }
    }
}
