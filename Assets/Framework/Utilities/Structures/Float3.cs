using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public struct Float3 : IComparable<Float3>, IEquatable<Float3>, IEqualityComparer<Float3>, ISerializable {
        /// <summary>
        /// x component
        /// </summary>
        public float x;

        /// <summary>
        /// y component
        /// </summary>
        public float y;

        /// <summary>
        /// z component
        /// </summary>
        public float z;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rX"></param>
        /// <param name="rY"></param>
        /// <param name="rZ"></param>
        public Float3(float rX, float rY, float rZ) {
            x = rX;
            y = rY;
            z = rZ;
        }

        /// <summary>
        /// Returns a string representation of the object
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return System.String.Format("[{0}, {1}, {2}]", x, y, z);
        }

        /// <summary>
        /// Automatic conversion from SerializableVector3 to Vector3
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator Vector3(Float3 rValue) {
            return new Vector3(rValue.x, rValue.y, rValue.z);
        }

        /// <summary>
        /// Automatic conversion from Vector3 to SerializableVector3
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator Float3(Vector3 rValue) {
            return new Float3(rValue.x, rValue.y, rValue.z);
        }

        public string ToString(Char separator) {
            return string.Format("{1}{0}{2}{0}{3}", separator, x, y, z);
        }

        public bool Equals(Float3 other) {
            return Math.Abs(x - other.x) < CompareTolerance && Math.Abs(y - other.y) < CompareTolerance && Math.Abs(z - other.z) < CompareTolerance;
        }

        public override bool Equals(object obj) {
            if (obj is Float3) {
                var other = (Float3) obj;
                return Math.Abs(x - other.x) < CompareTolerance && Math.Abs(y - other.y) < CompareTolerance && Math.Abs(z - other.z) < CompareTolerance;
            }
            return false;
        }

        public int CompareTo(Float3 other) {
            return x.CompareTo(other.x) + z.CompareTo(other.y) + y.CompareTo(other.z);
        }

        private const float CompareTolerance = 0.01f;

        public bool Equals(Float3 a, Float3 b) {
            return Math.Abs(a.x - b.x) < CompareTolerance && Math.Abs(a.y - b.y) < CompareTolerance && Math.Abs(a.z - b.z) < CompareTolerance;
        }

        public int GetHashCode(Float3 p) {
            unchecked {
                return (p.x.GetHashCode() * 397) ^ p.z.GetHashCode() ^ p.y.GetHashCode();
            }
        }

        public override int GetHashCode() {
            unchecked {
                return (x.GetHashCode() * 397) ^ z.GetHashCode() ^ y.GetHashCode();
            }
        }

        public static bool TryParse(string value, out Float3 result, char separator) {
            string[] values = value.Split(separator);
            int x;
            int z;
            int y;
            if (value.Length < 3) {
                result = zero;
                return false;
            }
            if (int.TryParse(values[0], out x) && int.TryParse(values[1], out y) && int.TryParse(values[2], out z)) {
                result = new Float3(x, y, z);
                return true;
            }
            result = zero;
            return false;
        }

        public static bool TryParse(string value, out Float3 result) {
            return TryParse(value, out result, ',');
        }

        public float this[int index] {
            get {
                switch (index) {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    default:
                        throw new IndexOutOfRangeException("Invalid F3 index!");
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
                        throw new IndexOutOfRangeException("Invalid F3 index!");
                }
            }
        }

        public Vector3 toVector3() {
            return new Vector3(x, y, z);
        }
        public int Count { get { return 2; } }
        public float magnitude { get { return Mathf.Sqrt(x * x + y * y + z * z); } }

        public Float3(SerializationInfo info, StreamingContext context) {
            x = info.GetValue(nameof(x), 0);
            y = info.GetValue(nameof(y), 0);
            z = info.GetValue(nameof(z), 0);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(x), x);
            info.AddValue(nameof(y), y);
            info.AddValue(nameof(z), z);
        }

        public static Float3 operator +(Float3 obj, Float3 obj2) {
            return new Float3(obj.x + obj2.x, obj.y + obj2.y, obj.z + obj2.z);
        }

        public static Float3 operator -(Float3 obj, Float3 obj2) {
            return new Float3(obj.x - obj2.x, obj.y - obj2.y, obj.z - obj2.z);
        }

        public static Float3 operator *(Float3 c1, int c2) {
            return new Float3(c1.x * c2, c1.y * c2, c1.z * c2);
        }

        public static Float3 operator *(Float3 c1, float c2) {
            return new Float3(c1.x * c2, c1.y * c2, c1.z * c2);
        }

        public static Float3 operator *(Float3 c1, Float3 c2) {
            return new Float3(c1.x * c2.x, c1.y * c2.y, c1.z * c2.z);
        }

        public static Float3 operator *(Float3 c1, Vector4 c2) {
            return new Float3(c1.x * c2.x, c1.y * c2.y, c1.z * c2.z);
        }

        public static Float3 operator *(int c1, Float3 c2) {
            return new Float3(c1 * c2.x, c1 * c2.y, c1 * c2.z);
        }

        public static bool operator ==(Float3 obj, Float3 obj2) {
            return obj.x == obj2.x && obj.y == obj2.y && obj.z == obj2.z;
        }

        public static bool operator !=(Float3 obj, Float3 obj2) {
            return !(obj == obj2);
        }

        public static Float3 operator /(Float3 a, int d) {
            return new Float3(a.x / d, a.y / d, a.z / d);
        }

        public static Float3 operator /(Float3 a, float d) {
            return new Float3(a.x / d, a.y / d, a.z / d);
        }

        public static readonly Float3 max = new Float3(int.MaxValue, int.MaxValue, int.MaxValue);
        public static readonly Float3 zero = new Float3(0, 0, 0);
        public static readonly Float3 one = new Float3(1, 1, 1);
        public static readonly Float3 forward = new Float3(0, 0, 1);
        public static readonly Float3 back = new Float3(0, 0, -1);
        public static readonly Float3 up = new Float3(0, 1, 0);
        public static readonly Float3 down = new Float3(0, -1, 0);
        public static readonly Float3 left = new Float3(-1, 0, 0);
        public static readonly Float3 right = new Float3(1, 0, 0);
        public static readonly Float3 forward_right = new Float3(1, 0, 1);
        public static readonly Float3 forward_left = new Float3(-1, 0, 1);
        public static readonly Float3 forward_up = new Float3(0, 1, 1);
        public static readonly Float3 forward_down = new Float3(0, -1, 1);
        public static readonly Float3 back_right = new Float3(1, 0, -1);
        public static readonly Float3 back_left = new Float3(-1, 0, -1);
        public static readonly Float3 back_up = new Float3(0, 1, -1);
        public static readonly Float3 back_down = new Float3(0, -1, -1);
        public static readonly Float3 up_right = new Float3(1, 1, 0);
        public static readonly Float3 up_left = new Float3(-1, 1, 0);
        public static readonly Float3 down_right = new Float3(1, -1, 0);
        public static readonly Float3 down_left = new Float3(-1, -1, 0);
        public static readonly Float3 forward_right_up = new Float3(1, 1, 1);
        public static readonly Float3 forward_right_down = new Float3(1, -1, 1);
        public static readonly Float3 forward_left_up = new Float3(-1, 1, 1);
        public static readonly Float3 forward_left_down = new Float3(-1, -1, 1);
        public static readonly Float3 back_right_up = new Float3(1, 1, -1);
        public static readonly Float3 back_right_down = new Float3(1, -1, -1);
        public static readonly Float3 back_left_up = new Float3(-1, 1, -1);
        public static readonly Float3 back_left_down = new Float3(-1, -1, -1);
    }
}
