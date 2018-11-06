using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PathfindMoverNode : INode {
        public Entity Entity { get; private set; }

        public CachedComponent<AstarPathfinderData> Pathfinder = new CachedComponent<AstarPathfinderData>();
        public CachedComponent<MoveSpeed> MoveSpeed = new CachedComponent<MoveSpeed>();
        public CachedComponent<RotationSpeed> RotationSpeed = new CachedComponent<RotationSpeed>();
        public CachedComponent<TransformComponent> Tr = new CachedComponent<TransformComponent>();
        public CachedComponent<MoveTarget> Target = new CachedComponent<MoveTarget>();

        public PathfindMoverNode(Entity entity, Dictionary<System.Type, ComponentReference> list) {
            Register(entity, list);
        }

        public PathfindMoverNode() {
        }

        public void Register(Entity entity, Dictionary<System.Type, ComponentReference> list) {
            Entity = entity;
            Pathfinder.Set(entity, list);
            MoveSpeed.Set(entity, list);
            RotationSpeed.Set(entity, list);
            Tr.Set(entity, list);
            Target.Set(entity, list);
        }

        public void Dispose() {
            Pathfinder.Dispose();
            MoveSpeed.Dispose();
            RotationSpeed.Dispose();
            Tr.Dispose();
            Target.Dispose();
        }

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(AstarPathfinderData), 
                typeof(MoveTarget),
            };
        }
    }
}
