using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
    public class TransformState {
        public Vector3 Position;
        public Quaternion Rotation;

        public void Set(Transform tr) {
            Position = tr.localPosition;
            Rotation = tr.localRotation;
        }

        public void SetInverse(Transform tr, Transform other) {
            Debug.Log(tr.name + " " + other.name + ": " + other.position);
            Debug.DrawRay(other.position, other.forward, Color.red, 15f);
            Position = tr.InverseTransformPoint(other.position);
            Debug.Log(Position + " transformed " + tr.TransformPoint(Position));
            Rotation = Quaternion.Inverse(tr.parent.rotation) * other.rotation;
            Debug.DrawRay(tr.TransformPoint(Position), other.forward, Color.green, 15f);
        }

        public void Restore(Transform tr) {
            tr.localPosition = Position;
            tr.localRotation = Rotation;
        }
    }
}
