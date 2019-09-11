using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ArcMover : ComponentBase {

        public float Angle;

        public Vector3 MoveVector;
        public float ElapsedTime;
        public float Duration;

        public ArcMover(float angle = 15) {
            Angle = angle;
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
