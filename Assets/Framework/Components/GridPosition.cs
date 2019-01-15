using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct GridPosition : IComponent {
        public int Owner { get; set; }
        public Point3 Position;

        public GridPosition(Point3 position) : this() {
            Position = position;
        }

        public static implicit operator Point3(GridPosition reference) {
            return reference.Position;
        }
    }
}
