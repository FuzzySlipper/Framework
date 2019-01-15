using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PathfindMoverNode : BaseNode {

        public CachedComponent<SimplePathfindingAgent> Pathfinder = new CachedComponent<SimplePathfindingAgent>();
        public CachedComponent<MoveSpeed> MoveSpeed = new CachedComponent<MoveSpeed>();
        public CachedComponent<RotationSpeed> RotationSpeed = new CachedComponent<RotationSpeed>();
        public CachedComponent<MoveTarget> Target = new CachedComponent<MoveTarget>();
        public CachedComponent<PathfindingDebugging> Debugging = new CachedComponent<PathfindingDebugging>();
        

        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            Pathfinder, MoveSpeed, RotationSpeed, Target, Debugging
        };
        
        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(SimplePathfindingAgent), 
                typeof(MoveTarget),
            };
        }

        public float GetMoveSpeed { get { return MoveSpeed.c?.Speed ?? 1; } }
    }
}
