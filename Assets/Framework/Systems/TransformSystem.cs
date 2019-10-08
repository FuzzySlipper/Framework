using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Lowest)]
    [AutoRegister]
    public sealed class TransformSystem : SystemBase, IReceive<DeathEvent>, IReceiveGlobalArray<SetTransformPosition>, 
    IReceiveGlobalArray<SetTransformRotation>, IReceiveGlobalArray<MoveTransform>, IReceiveGlobalArray<SetLocalTransformPosition>,
    IReceiveGlobalArray<SetLocalTransformRotation> {
        private ManagedArray<MoveTransform>.Delegate _moveDel;
        private ManagedArray<SetTransformPosition>.Delegate _setMoveDel;
        private ManagedArray<SetTransformRotation>.Delegate _setRotDel;
        private ManagedArray<SetLocalTransformPosition>.Delegate _setLocalMoveDel;
        private ManagedArray<SetLocalTransformRotation>.Delegate _setLocalRotDel;
        
        public TransformSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new []{ typeof(DisableTrOnDeath)}));
            _moveDel = RunUpdate;
            _setMoveDel = RunUpdate;
            _setRotDel = RunUpdate;
            _setLocalMoveDel = RunUpdate;
            _setLocalRotDel = RunUpdate;
        }
        
        public void Handle(DeathEvent arg) {
            if (arg.Target.Entity.HasComponent<DisableTrOnDeath>()) {
                arg.Target.Tr.gameObject.SetActive(false);
            }
        }

        public void HandleGlobal(BufferedList<MoveTransform> arg) {
            arg.Run(_moveDel);
        }

        private void RunUpdate(MoveTransform arg) {
            arg.Transform.SetPosition(arg.Transform.position + arg.Velocity);
        }

        public void HandleGlobal(BufferedList<SetTransformPosition> arg) {
            arg.Run(_setMoveDel);
        }

        private void RunUpdate(SetTransformPosition arg) {
            arg.Transform.SetPosition(arg.Position);
        }

        public void HandleGlobal(BufferedList<SetTransformRotation> arg) {
            arg.Run(_setRotDel);
        }

        private void RunUpdate(SetTransformRotation arg) {
            arg.Transform.SetRotation(arg.Rotation);
        }

        public void HandleGlobal(BufferedList<SetLocalTransformPosition> arg) {
            arg.Run(_setLocalMoveDel);
        }

        private void RunUpdate(SetLocalTransformPosition arg) {
            arg.Transform.SetLocalPosition(arg.Position);
        }
        
        public void HandleGlobal(BufferedList<SetLocalTransformRotation> arg) {
            arg.Run(_setLocalRotDel);
        }

        private void RunUpdate(SetLocalTransformRotation arg) {
            arg.Transform.SetLocalRotation(arg.Rotation);
        }
    }

    public struct MoveTransform : IEntityMessage {
        public Vector3 Velocity { get; }
        public TransformComponent Transform { get; }

        public MoveTransform(TransformComponent transform, Vector3 velocity) {
            Velocity = velocity;
            Transform = transform;
        }
    }

    public struct SetTransformPosition : IEntityMessage {
        public Vector3 Position { get; }
        public TransformComponent Transform { get; }

        public SetTransformPosition(TransformComponent transform, Vector3 position) {
            Position = position;
            Transform = transform;
        }
    }

    public struct SetTransformRotation : IEntityMessage {
        public Quaternion Rotation { get; }
        public TransformComponent Transform { get; }

        public SetTransformRotation(TransformComponent transform, Quaternion rotation) {
            Rotation = rotation;
            Transform = transform;
        }
    }

    public struct SetLocalTransformPosition : IEntityMessage {
        public Vector3 Position { get; }
        public TransformComponent Transform { get; }

        public SetLocalTransformPosition(TransformComponent transform, Vector3 position) {
            Position = position;
            Transform = transform;
        }
    }

    public struct SetLocalTransformRotation : IEntityMessage {
        public Quaternion Rotation { get; }
        public TransformComponent Transform { get; }

        public SetLocalTransformRotation(TransformComponent transform, Quaternion rotation) {
            Rotation = rotation;
            Transform = transform;
        }
    }
}
