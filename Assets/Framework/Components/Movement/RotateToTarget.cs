using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class RotateToTarget : IComponent {

        public Vector3 Position;
        public CachedTransform TargetTr;
        public float RotationSpeed;

        public Vector3 GetTarget {
            get {
                if (TargetTr != null) {
                    return TargetTr.Tr.position;
                }
                return Position;
            }
        }

        public RotateToTarget(Vector3 position, Transform targetTr, float rotationSpeed) {
            Position = position;
            TargetTr = new CachedTransform(targetTr);
            RotationSpeed = rotationSpeed;
        }

        public RotateToTarget(SerializationInfo info, StreamingContext context) {
            Position = info.GetValue(nameof(Position), Position);
            RotationSpeed = info.GetValue(nameof(RotationSpeed), RotationSpeed);
            TargetTr = info.GetValue(nameof(TargetTr), TargetTr);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Position), Position);
            info.AddValue(nameof(RotationSpeed), RotationSpeed);
            info.AddValue(nameof(TargetTr), TargetTr);
        }
    }
}
