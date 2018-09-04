using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace PixelComrades {
    [Serializable]
    public struct Point3 : IComparable<Point3>, IEquatable<Point3>, IEqualityComparer<Point3>, IXmlSerializable , ISerializable {

        [SerializeField] public int x;
        [SerializeField] public int y;
        [SerializeField] public int z;

        public Point3(int singleValue) {
            this.x = singleValue;
            this.y = singleValue;
            this.z = singleValue;
        }

        public Point3(int x, int y, int z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Point3(float x, float y, float z) {
            this.x = (int) Math.Round((double) x);
            this.y = (int) Math.Round((double) y);
            this.z = (int) Math.Round((double) z);
        }

        public Point3(Vector3 v) {
            x = (int) Math.Round(v.x);
            y = (int) Math.Round(v.y);
            z = (int) Math.Round(v.z);
        }

        public int SqrDistance(Point3 point) {
            int dx = x - point.x;
            int dy = y - point.y;
            int dz = z - point.z;
            return (dx * dx) + (dy * dy) + (dz * dz);
        }

        public int Distance(Point3 p2) {
            return Mathf.Abs(x - p2.x) + Mathf.Abs(y - p2.y) + Mathf.Abs(z - p2.z);
        }

        public int DistanceDiagonal(Point3 p2) {
            return Math.Max(Math.Abs(p2.x - x), Math.Abs(p2.z - z));
        }

        public bool IsNeighbor(Point3 point) {
            if (Mathf.Abs((point.x - x) + (point.y - y) + (point.z - z)) > 1) {
                return false;
            }
            return true;
        }

        public System.Xml.Schema.XmlSchema GetSchema() {
            return null;
        }

        public void ReadXml(XmlReader reader) {
            reader.MoveToContent();
            reader.ReadStartElement();
            x = reader.ReadElementContentAsInt();
            y = reader.ReadElementContentAsInt();
            z = reader.ReadElementContentAsInt();
            reader.ReadEndElement();

        }

        public void WriteXml(XmlWriter writer) {
            writer.WriteElementString("x", x.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("y", y.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("z", z.ToString(CultureInfo.InvariantCulture));
        }

        public int sqrMagnitudeXZ { get { return (x * x) + (z * z); } }
        public int sqrMagnitude { get { return (x * x) + (y * y) + (z * z); } }
        public int highestAxis { get { return MathEx.Max(x, y, z); } }

        public override string ToString() {
            return string.Format("{0}, {1}, {2}", x, y, z);
        }

        public string ToString(Char separator) {
            return string.Format("{1}{0}{2}{0}{3}", separator, x, y, z);
        }

        public bool Equals(Point3 other) {
            return x == other.x && y == other.y && z == other.z;
        }

        public override bool Equals(object obj) {
            if (obj is Point3) {
                var other = (Point3) obj;
                return x == other.x && y == other.y && z == other.z;
            }
            return false;
        }

        public int CompareTo(Point3 other) {
            return x.CompareTo(other.x) + z.CompareTo(other.y) + y.CompareTo(other.z);
        }

        public bool Equals(Point3 a, Point3 b) {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }

        public int GetHashCode(Point3 p) {
            unchecked {
                return (p.x.GetHashCode() * 397) ^ p.z.GetHashCode() ^ p.y.GetHashCode();
            }
        }

        public override int GetHashCode() {
            unchecked {
                return (x.GetHashCode() * 397) ^ z.GetHashCode() ^ y.GetHashCode();
            }
        }

        public static bool TryParse(string value, out Point3 result, char separator) {
            string[] values = value.Split(separator);
            int x;
            int z;
            int y;
            if (value.Length < 3) {
                result = zero;
                return false;
            }
            if (int.TryParse(values[0], out x) && int.TryParse(values[1], out y) && int.TryParse(values[2], out z)) {
                result = new Point3(x, y, z);
                return true;
            }
            result = zero;
            return false;
        }

        public static bool TryParse(string value, out Point3 result) {
            return TryParse(value, out result, ',');
        }

        public int this[int index] {
            get {
                switch (index) {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    default:
                        throw new IndexOutOfRangeException("Invalid P3 index!");
                }
            }
            set {
                switch (index) {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    case 2:
                        z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid P3 index!");
                }
            }
        }

        public Vector3 toVector3() {
            return new Vector3(x, y, z);
        }

        public Point3 PlusDirection(Directions dir) {
            switch (dir) {
                case Directions.Forward:
                    return this + forward;
                    ;
                case Directions.Right:
                    return this + right;
                case Directions.Left:
                    return this + left;
                case Directions.Back:
                    return this + back;
            }
            return this;
        }

        public int Count { get { return 2; } }
        public float magnitude { get { return Mathf.Sqrt(x * x + y * y + z * z); } }

        public static Point3 operator +(Point3 obj, Point3 obj2) {
            return new Point3(obj.x + obj2.x, obj.y + obj2.y, obj.z + obj2.z);
        }

        public static Point3 operator -(Point3 obj, Point3 obj2) {
            return new Point3(obj.x - obj2.x, obj.y - obj2.y, obj.z - obj2.z);
        }

        public static Point3 operator *(Point3 c1, int c2) {
            return new Point3(c1.x * c2, c1.y * c2, c1.z * c2);
        }

        public static Point3 operator *(Point3 c1, float c2) {
            return new Point3(c1.x * c2, c1.y * c2, c1.z * c2);
        }

        public static Point3 operator *(Point3 c1, Point3 c2) {
            return new Point3(c1.x * c2.x, c1.y * c2.y, c1.z * c2.z);
        }

        public static Point3 operator *(Point3 c1, Vector4 c2) {
            return new Point3(c1.x * c2.x, c1.y * c2.y, c1.z * c2.z);
        }

        public static Point3 operator *(int c1, Point3 c2) {
            return new Point3(c1 * c2.x, c1 * c2.y, c1 * c2.z);
        }

        public static bool operator ==(Point3 obj, Point3 obj2) {
            return obj.x == obj2.x && obj.y == obj2.y && obj.z == obj2.z;
        }

        public static bool operator !=(Point3 obj, Point3 obj2) {
            return !(obj == obj2);
        }

        public static Point3 operator /(Point3 a, int d) {
            return new Point3(a.x / d, a.y / d, a.z / d);
        }

        public static Point3 operator /(Point3 a, float d) {
            return new Point3(a.x / d, a.y / d, a.z / d);
        }

        public static readonly Point3 max = new Point3(int.MaxValue, int.MaxValue, int.MaxValue);
        public static readonly Point3 zero = new Point3(0, 0, 0);
        public static readonly Point3 one = new Point3(1, 1, 1);
        public static readonly Point3 forward = new Point3(0, 0, 1);
        public static readonly Point3 back = new Point3(0, 0, -1);
        public static readonly Point3 up = new Point3(0, 1, 0);
        public static readonly Point3 down = new Point3(0, -1, 0);
        public static readonly Point3 left = new Point3(-1, 0, 0);
        public static readonly Point3 right = new Point3(1, 0, 0);
        public static readonly Point3 forward_right = new Point3(1, 0, 1);
        public static readonly Point3 forward_left = new Point3(-1, 0, 1);
        public static readonly Point3 forward_up = new Point3(0, 1, 1);
        public static readonly Point3 forward_down = new Point3(0, -1, 1);
        public static readonly Point3 back_right = new Point3(1, 0, -1);
        public static readonly Point3 back_left = new Point3(-1, 0, -1);
        public static readonly Point3 back_up = new Point3(0, 1, -1);
        public static readonly Point3 back_down = new Point3(0, -1, -1);
        public static readonly Point3 up_right = new Point3(1, 1, 0);
        public static readonly Point3 up_left = new Point3(-1, 1, 0);
        public static readonly Point3 down_right = new Point3(1, -1, 0);
        public static readonly Point3 down_left = new Point3(-1, -1, 0);
        public static readonly Point3 forward_right_up = new Point3(1, 1, 1);
        public static readonly Point3 forward_right_down = new Point3(1, -1, 1);
        public static readonly Point3 forward_left_up = new Point3(-1, 1, 1);
        public static readonly Point3 forward_left_down = new Point3(-1, -1, 1);
        public static readonly Point3 back_right_up = new Point3(1, 1, -1);
        public static readonly Point3 back_right_down = new Point3(1, -1, -1);
        public static readonly Point3 back_left_up = new Point3(-1, 1, -1);
        public static readonly Point3 back_left_down = new Point3(-1, -1, -1);

        public static readonly Point3[] CardinalDirections = new Point3[] {
            left, right,
            back, forward,
            down, up,
        };

        public static readonly Point3[] AllDirections = new Point3[] {
            left,
            right,
            back,
            forward,
            down,
            up,
            forward_right,
            forward_left,
            forward_up,
            forward_down,
            back_right,
            back_left,
            back_up,
            back_down,
            up_right,
            up_left,
            down_right,
            down_left,
            forward_right_up,
            forward_right_down,
            forward_left_up,
            forward_left_down,
            back_right_up,
            back_right_down,
            back_left_up,
            back_left_down,
        };

        private static Point3[] _gridMoveDirections = new[] {
            new Point3(0, 0, 1), new Point3(1, 0, 0),
            new Point3(0, 0, -1), new Point3(-1, 0, 0),
            new Point3(0, 1, 0), new Point3(0, -1, 0),
        };

        public static Point3 MoveDirection(int index) {
            index = Mathf.Clamp(index, 0, _gridMoveDirections.Length - 1);
            return _gridMoveDirections[index];
        }

        public static Point3 MoveDirection(Directions dir) {
            int index = Mathf.Clamp((int) dir, 0, _gridMoveDirections.Length - 1);
            return _gridMoveDirections[index];
        }

        public Point3(SerializationInfo info, StreamingContext context) {
            x = info.GetValue(nameof(x), 0);
            y = info.GetValue(nameof(y), 0);
            z = info.GetValue(nameof(z), 0);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(x), x);
            info.AddValue(nameof(y), y);
            info.AddValue(nameof(z), z);
        }
    }
}