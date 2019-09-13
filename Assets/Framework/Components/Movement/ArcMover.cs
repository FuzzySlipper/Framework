using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class ArcMover : IComponent {

        public float Angle;

        public Vector3 MoveVector;
        public float ElapsedTime;
        public float Duration;

        public ArcMover(float angle = 15) {
            Angle = angle;
        }

        public ArcMover(SerializationInfo info, StreamingContext context) {
            Angle = info.GetValue(nameof(Angle), Angle);
            MoveVector = info.GetValue(nameof(MoveVector), MoveVector);
            ElapsedTime = info.GetValue(nameof(ElapsedTime), ElapsedTime);
            Duration = info.GetValue(nameof(Duration), Duration);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Angle), Angle);
            info.AddValue(nameof(MoveVector), MoveVector);
            info.AddValue(nameof(ElapsedTime), ElapsedTime);
            info.AddValue(nameof(Duration), Duration);
        }

        public static void CalculateFlight(Transform tr, float angle, Vector3 target, float speed, out Vector3 moveVector, out float duration) {
            float targetDistance = Vector3.Distance(tr.position, target);
            // Calculate the velocity needed to throw the object to the target at specified angle.
            float projectileVelocity = targetDistance / (Mathf.Sin(2 * angle* Mathf.Deg2Rad) / speed);
            moveVector = Vector3.zero;
            moveVector.z = Mathf.Sqrt(projectileVelocity) * Mathf.Cos(angle* Mathf.Deg2Rad);
            moveVector.y = Mathf.Sqrt(projectileVelocity) * Mathf.Sin(angle* Mathf.Deg2Rad);
            // Calculate flight time.
            duration = targetDistance / moveVector.z;
            // Rotate projectile to face the target.
            tr.rotation = Quaternion.LookRotation(target - tr.position);
        }
    }
}
