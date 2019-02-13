using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public struct Float4 : IComparable<Float4>, IEquatable<Float4>, IEqualityComparer<Float4>, ISerializable {
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
        /// z component
        /// </summary>
        public float w;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rX"></param>
        /// <param name="rY"></param>
        /// <param name="rZ"></param>
        public Float4(float rX, float rY, float rZ, float rW) {
            x = rX;
            y = rY;
            z = rZ;
            w = rW;
        }

        /// <summary>
        /// Returns a string representation of the object
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return System.String.Format("[{0}, {1}, {2}, {3}]", x, y, z, w);
        }

        public static implicit operator Quaternion(Float4 rValue) {
            return new Quaternion(rValue.x, rValue.y, rValue.z, rValue.w);
        }

        public static implicit operator Float4(Quaternion rValue) {
            return new Float4(rValue.x, rValue.y, rValue.z, rValue.w);
        }

        public string ToString(Char separator) {
            return string.Format("{1}{0}{2}{0}{3}", separator, x, y, z);
        }

        public bool Equals(Float4 other) {
            return Math.Abs(x - other.x) < CompareTolerance && Math.Abs(y - other.y) < CompareTolerance && Math.Abs(z - other.z) < CompareTolerance && Math.Abs(w - other.w) < CompareTolerance;
        }

        public override bool Equals(object obj) {
            if (obj is Float4) {
                var other = (Float4) obj;
                return Math.Abs(x - other.x) < CompareTolerance && Math.Abs(y - other.y) < CompareTolerance && Math.Abs(z - other.z) < CompareTolerance && Math.Abs(w - other.w) < CompareTolerance;
            }
            return false;
        }

        public int CompareTo(Float4 other) {
            return x.CompareTo(other.x) + z.CompareTo(other.y) + y.CompareTo(other.z) + w.CompareTo(other.w);
        }

        private const float CompareTolerance = 0.01f;

        public bool Equals(Float4 a, Float4 b) {
            return Math.Abs(a.x - b.x) < CompareTolerance && Math.Abs(a.y - b.y) < CompareTolerance && Math.Abs(a.z - b.z) < CompareTolerance && Math.Abs(a.w - b.w) < CompareTolerance;
        }

        public int GetHashCode(Float4 p) {
            unchecked {
                return (p.x.GetHashCode() * 397) ^ p.z.GetHashCode() ^ p.y.GetHashCode() ^ p.w.GetHashCode();
            }
        }

        public override int GetHashCode() {
            unchecked {
                return (x.GetHashCode() * 397) ^ z.GetHashCode() ^ y.GetHashCode() ^ w.GetHashCode();
            }
        }

        public static bool TryParse(string value, out Float4 result, char separator) {
            string[] values = value.Split(separator);
            if (value.Length < 4) {
                result = zero;
                return false;
            }
            if (float.TryParse(values[0], out var x) && float.TryParse(values[1], out var y) && float.TryParse(values[2], out var z) && float.TryParse(values[3], out var w)) {
                result = new Float4(x, y, z, w);
                return true;
            }
            result = zero;
            return false;
        }

        public static bool TryParse(string value, out Float4 result) {
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
                    case 3:
                        return w;
                    default:
                        throw new IndexOutOfRangeException("Invalid F4 index!");
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
                    case 3:
                        w = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid F4 index!");
                }
            }
        }

        public Vector3 toVector3() {
            return new Quaternion(x,y,z,w).eulerAngles;
        }

        public Quaternion toQuaternion() {
            return new Quaternion(x, y, z, w);
        }

        public int Count { get { return 3; } }
        public float magnitude { get { return Mathf.Sqrt(x * x + y * y + z * z + w * w); } }

        public Float4(SerializationInfo info, StreamingContext context) {
            x = info.GetValue(nameof(x), 0);
            y = info.GetValue(nameof(y), 0);
            z = info.GetValue(nameof(z), 0);
            w = info.GetValue(nameof(w), 0);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(x), x);
            info.AddValue(nameof(y), y);
            info.AddValue(nameof(z), z);
            info.AddValue(nameof(w), w);
        }

        public static readonly Float4 zero = new Float4(0, 0, 0, 0);
        public static readonly Float4 one = new Float4(1, 1, 1, 1);
    }
}
