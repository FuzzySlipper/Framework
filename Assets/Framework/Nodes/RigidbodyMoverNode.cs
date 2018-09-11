using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class RigidbodyMoverNode : INode {
        public Entity Entity { get; }

        public CachedComponent<RigidbodyComponent> Rb;
        public CachedComponent<MoveSpeed> MoveSpeed;
        public CachedComponent<RotationSpeed> RotationSpeed;
        public CachedComponent<TransformComponent> Tr;
        public CachedComponent<MoveTarget> Target;
        public CachedComponent<VelocityMover> Mover;

        public RigidbodyMoverNode(Entity entity, Dictionary<System.Type, ComponentReference> list) {
            Entity = entity;
            Rb = new CachedComponent<RigidbodyComponent>(entity, list);
            MoveSpeed = new CachedComponent<MoveSpeed>(entity, list);
            RotationSpeed = new CachedComponent<RotationSpeed>(entity, list);
            Tr = new CachedComponent<TransformComponent>(entity, list);
            Target = new CachedComponent<MoveTarget>(entity, list);
            Mover = new CachedComponent<VelocityMover>(entity, list);
        }

        public void Dispose() {
            Rb.Dispose();
            Rb = null;
            MoveSpeed.Dispose();
            MoveSpeed = null;
            RotationSpeed.Dispose();
            RotationSpeed = null;
            Tr.Dispose();
            Tr = null;
            Target.Dispose();
            Target = null;
            Mover.Dispose();
            Mover = null;
        }

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(RigidbodyComponent),
                typeof(MoveSpeed),
                typeof(TransformComponent),
                typeof(VelocityMover),
                typeof(MoveTarget),
            };
        }
    }
}
