using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Serializable]
    public class SerializedTransform : IEquatable<SerializedTransform>, ISerializable {

        public static readonly SerializedTransform Identity = new SerializedTransform(Vector3.zero, Quaternion.identity, Vector3.one);

        [SerializeField] private Vector3 _position;
        [SerializeField] private Quaternion _rotation;
        [SerializeField] private Vector3 _scale;

        // If the matrix needs rebuilt, this will be true.  Used to delay expensive
        // matrix construction til necessary (since t/r/s can change a lot before a
        // matrix is needed).
        private bool _dirty = true;
        private Matrix4x4 _matrix;

        public SerializedTransform() {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            scale = Vector3.one;
            _matrix = Matrix4x4.identity;
            _dirty = false;
        }

        public SerializedTransform(Vector3 position, Quaternion rotation, Vector3 scale) {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;

            _matrix = Matrix4x4.TRS(position, rotation, scale);
            _dirty = false;
        }

        public SerializedTransform(Transform transform) {
            position = transform.localPosition;
            rotation = transform.localRotation;
            scale = transform.localScale;

            _matrix = Matrix4x4.TRS(position, rotation, scale);
            _dirty = false;
        }

        public SerializedTransform(SerializedTransform transform) {
            position = transform.position;
            rotation = transform.rotation;
            scale = transform.scale;

            _matrix = Matrix4x4.TRS(position, rotation, scale);
            _dirty = false;
        }

        public SerializedTransform(SerializationInfo info, StreamingContext context) {
            _position = (Vector3)info.GetValue("position", typeof(Vector3));
            _rotation = (Quaternion)info.GetValue("rotation", typeof(Quaternion));
            _scale = (Vector3)info.GetValue("scale", typeof(Vector3));
            _dirty = true;
        }

        public void Set(Transform trs) {
            position = trs.localPosition;
            rotation = trs.localRotation;
            scale = trs.localScale;
            _dirty = true;
        }

        public void Restore(Transform tr) {
            tr.localPosition = position;
            tr.localRotation = rotation;
            tr.localScale = scale;
        }

        public Vector3 position {
            get { return _position; }
            set {
                _dirty = true;
                _position = value;
            }
        }

        public Quaternion rotation {
            get { return _rotation; }
            set {
                _dirty = true;
                _rotation = value;
            }
        }

        public Vector3 scale {
            get { return _scale; }
            set {
                _dirty = true;
                _scale = value;
            }
        }

        public Vector3 up { get { return rotation * Vector3.up; } }
        public Vector3 forward { get { return rotation * Vector3.forward; } }
        public Vector3 right { get { return rotation * Vector3.right; } }

        public bool Equals(SerializedTransform rhs) {
            return Approx(position, rhs.position) &&
                   Approx(rotation, rhs.rotation) &&
                   Approx(scale, rhs.scale);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("position", _position, typeof(Vector3));
            info.AddValue("rotation", _rotation, typeof(Quaternion));
            info.AddValue("scale", _scale, typeof(Vector3));
        }

        private bool Approx(Vector3 lhs, Vector3 rhs) {
            return Mathf.Abs(lhs.x - rhs.x) < Mathf.Epsilon &&
                   Mathf.Abs(lhs.y - rhs.y) < Mathf.Epsilon &&
                   Mathf.Abs(lhs.z - rhs.z) < Mathf.Epsilon;
        }

        private bool Approx(Quaternion lhs, Quaternion rhs) {
            return Mathf.Abs(lhs.x - rhs.x) < Mathf.Epsilon &&
                   Mathf.Abs(lhs.y - rhs.y) < Mathf.Epsilon &&
                   Mathf.Abs(lhs.z - rhs.z) < Mathf.Epsilon &&
                   Mathf.Abs(lhs.w - rhs.w) < Mathf.Epsilon;
        }

        public override bool Equals(object rhs) {
            return rhs is SerializedTransform && Equals((SerializedTransform)rhs);
        }

        public override int GetHashCode() {
            return position.GetHashCode() ^ rotation.GetHashCode() ^ scale.GetHashCode();
        }

        public Matrix4x4 GetMatrix() {
            if (!_dirty) {
                return _matrix;
            }
            _dirty = false;
            _matrix = Matrix4x4.TRS(position, rotation, scale);
            return _matrix;
        }

        public static SerializedTransform operator +(SerializedTransform lhs, SerializedTransform rhs) {
            var t = new SerializedTransform();

            t.position = lhs.position + rhs.position;
            t.rotation = lhs.rotation * rhs.rotation;
            t.scale = new Vector3(lhs.scale.x * rhs.scale.x,
                lhs.scale.y * rhs.scale.y,
                lhs.scale.z * rhs.scale.z);

            return t;
        }

        public static SerializedTransform operator +(Transform lhs, SerializedTransform rhs) {
            var t = new SerializedTransform();

            t.position = lhs.position + rhs.position;
            t.rotation = lhs.localRotation * rhs.rotation;
            t.scale = new Vector3(lhs.localScale.x * rhs.scale.x,
                lhs.localScale.y * rhs.scale.y,
                lhs.localScale.z * rhs.scale.z);

            return t;
        }

        public static bool operator ==(SerializedTransform lhs, SerializedTransform rhs) {
            return ReferenceEquals(lhs, rhs) || lhs.Equals(rhs);
        }

        public static bool operator !=(SerializedTransform lhs, SerializedTransform rhs) {
            return !(lhs == rhs);
        }

        public static SerializedTransform operator -(SerializedTransform lhs, SerializedTransform rhs) {
            var t = new SerializedTransform();

            t.position = lhs.position - rhs.position;
            t.rotation = Quaternion.Inverse(rhs.rotation) * lhs.rotation;
            t.scale = new Vector3(lhs.scale.x / rhs.scale.x,
                lhs.scale.y / rhs.scale.y,
                lhs.scale.z / rhs.scale.z);

            return t;
        }
        
        public override string ToString() {
            return position.ToString("F2") + "\n" + rotation.ToString("F2") + "\n" + scale.ToString("F2");
        }
    }
}
