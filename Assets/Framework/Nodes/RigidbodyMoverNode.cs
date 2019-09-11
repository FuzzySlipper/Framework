using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class RigidbodyMoverNode : INode {
        public Entity Entity { get; private set; }

        public CachedComponent<RigidbodyComponent> Rb = new CachedComponent<RigidbodyComponent>();
        public CachedComponent<MoveSpeed> MoveSpeed = new CachedComponent<MoveSpeed>();
        public CachedComponent<RotationSpeed> RotationSpeed = new CachedComponent<RotationSpeed>();
        public CachedComponent<MoveTarget> Target = new CachedComponent<MoveTarget>();
        public CachedComponent<VelocityMover> Mover = new CachedComponent<VelocityMover>();

        public RigidbodyMoverNode(Entity entity, SortedList<System.Type, ComponentReference> list) {
            Register(entity, list);
        }

        public RigidbodyMoverNode(){}

        public void Register(Entity entity, SortedList<Type, ComponentReference> list) {
            Entity = entity;
            Rb.Set(entity, list);
            MoveSpeed.Set(entity, list);
            RotationSpeed.Set(entity, list);
            Target.Set(entity, list);
            Mover.Set(entity, list);
        }

        public void Dispose() {
            Rb.Dispose();
            MoveSpeed.Dispose();
            RotationSpeed.Dispose();
            Target.Dispose();
            Mover.Dispose();
        }

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(RigidbodyComponent),
                typeof(MoveSpeed),
                typeof(VelocityMover),
                typeof(MoveTarget),
            };
        }
    }
}
