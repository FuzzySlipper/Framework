using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class MoveSpeed : IComponent {
        public int Owner { get; set; }
        public float Speed;

        public MoveSpeed(float speed) {
            Speed = speed;
        }

        public static implicit operator float(MoveSpeed reference) {
            return reference.Speed;
        }
    }
}
