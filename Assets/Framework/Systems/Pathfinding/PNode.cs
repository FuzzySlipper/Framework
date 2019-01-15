using System;
using PixelComrades;
using Priority_Queue;

namespace PixelComrades {
    public class PNode : FastPriorityQueueNode, IEquatable<PNode> {
        public int x;
        public int y;
        public int z;
        public PNode Parent;

        public bool Equals(PNode other) {
            return other != null && x == other.x && y == other.y && z == other.z;
        }

        public override bool Equals(object obj) {
            var other = (PNode) obj;
            return other != null && x == other.x && y == other.y && z == other.z;
        }

        public override int GetHashCode() {
            unchecked {
                return (x.GetHashCode() * 397) ^ z.GetHashCode() ^ y.GetHashCode();
            }
        }

        public override string ToString() {
            return "(" + x + ", " + y + ")";
        }

        public Point3 ToP3() {
            return new Point3(x,y,z);
        }
    }
}