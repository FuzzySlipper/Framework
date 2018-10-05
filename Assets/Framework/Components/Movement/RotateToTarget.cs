using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class RotateToTarget : IComponent {
        public int Owner { get; set; }

        public Vector3 Position;
        public Transform TargetTr;
        public float RotationSpeed;

        public Vector3 GetTarget {
            get {
                if (TargetTr != null) {
                    return TargetTr.position;
                }
                return Position;
            }
        }

        public Vector3 GetCurrent {
            get {
                if (Rb != null) {
                    return Rb.position;
                }
                if (RotateTr != null) {
                    return RotateTr.position;
                }
                return this.GetEntity().GetPosition();
            }
        }

        public Rigidbody Rb { get; }
        public Transform RotateTr { get; }

        public RotateToTarget(Entity owner, Vector3 position, Transform targetTr, float rotationSpeed) {
            Position = position;
            TargetTr = targetTr;
            Owner = owner;
            RotationSpeed = rotationSpeed;
            var rb = owner.Get<RigidbodyComponent>();
            if (rb != null) {
                Rb = rb.Rb;
            }
            if (Rb == null) {
                RotateTr = owner.Get<TransformComponent>().Tr;
            }
        }
    }
}
