using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Lowest)]
    [AutoRegister]
    public sealed class TransformSystem : SystemBase, IReceive<DeathEvent>, IReceiveGlobalArray<SetTransformPosition>, 
    IReceiveGlobalArray<SetTransformRotation>, IReceiveGlobalArray<MoveTransform>, IReceiveGlobalArray<SetLocalTransformPosition>,
    IReceiveGlobalArray<SetLocalTransformRotation> {

        public TransformSystem() {
            EntityController.RegisterReceiver<DisableTrOnDeath>(this);
        }
        
        public void Handle(DeathEvent arg) {
            if (arg.Target.Entity.HasComponent<DisableTrOnDeath>()) {
                arg.Target.Tr.gameObject.SetActive(false);
            }
        }

        public void HandleGlobal(BufferedList<MoveTransform> arg) {
            for (int i = 0; i < arg.Count; i++) {
                arg[i].Transform.SetPosition(arg[i].Transform.position + arg[i].Velocity);
            }
        }

        public void HandleGlobal(BufferedList<SetTransformPosition> arg) {
            for (int i = 0; i < arg.Count; i++) {
                arg[i].Transform.SetPosition(arg[i].Position);
            }
        }

        public void HandleGlobal(BufferedList<SetTransformRotation> arg) {
            for (int i = 0; i < arg.Count; i++) {
                arg[i].Transform.SetRotation(arg[i].Rotation);
            }
        }

        public void HandleGlobal(BufferedList<SetLocalTransformPosition> arg) {
            for (int i = 0; i < arg.Count; i++) {
                arg[i].Transform.SetLocalPosition(arg[i].Position);
            }
        }

        public void HandleGlobal(BufferedList<SetLocalTransformRotation> arg) {
            for (int i = 0; i < arg.Count; i++) {
                arg[i].Transform.SetLocalRotation(arg[i].Rotation);
            }
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
