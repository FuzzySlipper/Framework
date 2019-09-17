using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class RigidbodyMoverNode : BaseNode {
        
        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<VelocityMover> _velocityMover = new CachedComponent<VelocityMover>();
        private CachedComponent<MoveSpeed> _moveSpeed = new CachedComponent<MoveSpeed>();
        private CachedComponent<MoveTarget> _moveTarget = new CachedComponent<MoveTarget>();
        private CachedComponent<RotationSpeed> _rotationSpeed = new CachedComponent<RotationSpeed>();
        private CachedComponent<RigidbodyComponent> _rb = new CachedComponent<RigidbodyComponent>();
        public Transform Tr { get => _tr.Value; }
        public VelocityMover VelocityMover { get => _velocityMover; }
        public Rigidbody Rb { get => _rb.Value.Rb; }
        public MoveSpeed MoveSpeed { get => _moveSpeed; }
        public RotationSpeed RotationSpeed { get => _rotationSpeed; }
        public MoveTarget Target { get => _moveTarget; }
        
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _tr, _velocityMover, _rb, _moveSpeed, _moveTarget, _rotationSpeed
        };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(TransformComponent),
                typeof(RigidbodyComponent),
                typeof(MoveTarget),
                typeof(VelocityMover)
            };
        }
    }
}
