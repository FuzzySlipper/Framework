#define AStarPathfinding
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

namespace PixelComrades {
    public class SimplePathfindMoverNode : BaseNode {

        private CachedComponent<SimplePathfindingAgent> _pathfinder = new CachedComponent<SimplePathfindingAgent>();
        public CachedComponent<MoveSpeed> MoveSpeed = new CachedComponent<MoveSpeed>();
        public CachedComponent<RotationSpeed> RotationSpeed = new CachedComponent<RotationSpeed>();
        public CachedComponent<MoveTarget> Target = new CachedComponent<MoveTarget>();
        public CachedComponent<PathfindingDebugging> Debugging = new CachedComponent<PathfindingDebugging>();
        
        public SimplePathfindingAgent Pathfinder { get { return _pathfinder.c; } }

        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _pathfinder, MoveSpeed, RotationSpeed, Target, Debugging
        };
        
        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(SimplePathfindingAgent), 
                typeof(MoveTarget),
            };
        }

        public float GetMoveSpeed { get { return MoveSpeed.c?.Speed ?? 1; } }
    }

    public class AstarPathfindMoverNode : BaseNode {
#if AStarPathfinding
        private CachedComponent<AstarPathfindingAgent> _pathfinder = new CachedComponent<AstarPathfindingAgent>();
#endif
        public CachedComponent<MoveTarget> Target = new CachedComponent<MoveTarget>();
        public CachedComponent<PathfindingDebugging> Debugging = new CachedComponent<PathfindingDebugging>();

        public AstarPathfindingAgent Pathfinder { get { return _pathfinder.c; } }
        public Point3 DestinationP3 { get { return Pathfinder.DestinationP3; } }
        public PathfindingStatus CurrentStatus { get { return Pathfinder.CurrentStatus; } }
        public bool IsPathFinished { get { return Pathfinder.Controller.ReachedEndOfPath; } }

        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            Target, Debugging,
#if AStarPathfinding
            _pathfinder,
#endif
        };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
#if AStarPathfinding
                typeof(AstarPathfindingAgent),
#endif
                typeof(MoveTarget),
            };
        }

        
        public void ProcessNoMove() {
            var look = Vector3.zero;
            if (Target.c.HasValidLook) {
                var dir = Target.c.GetLookTarget - Pathfinder.Controller.transform.position;
                var nextRotation = dir != Vector3.zero ? Quaternion.LookRotation(dir, Vector3.up) : Pathfinder.Controller.transform.rotation;
                look = Vector3.ProjectOnPlane(nextRotation * Vector3.forward, Vector3.up).normalized;
            }
            Entity.Post(new MoveInputMessage(Vector3.zero, look));
            Pathfinder.LastPositionTime = 0;
            Pathfinder.LastPosition = Point3.zero;
            Pathfinder.StuckPathCount = 0;
        }

        public void Stop() {
            if (Entity.Tags.Contain(EntityTags.Moving)) {
                Entity.Tags.Remove(EntityTags.Moving);
            }
            Pathfinder.Controller.Stop();
            Pathfinder.CurrentStatus = PathfindingStatus.NoPath;
            ProcessNoMove();
        }

        public void SetMoving() {
            Pathfinder.CurrentStatus = PathfindingStatus.Moving;
            if (!Entity.Tags.Contain(EntityTags.Moving)) {
                Entity.Tags.Add(EntityTags.Moving);
            }
        }

        public void ReachedDestination() {
            Stop();
        }

        public void SetDestination(Vector3 pos) {
            Pathfinder.CurrentStatus = PathfindingStatus.WaitingOnPath;
            Pathfinder.Controller.SetDestination(pos);
        }

        public void SearchPath() {
            Pathfinder.Controller.SearchPath();
        }

        public void CheckPathWaiting() {
            if (Pathfinder.Controller.HasPath) {
                SetMoving();
            }
        }
        

        public void UpdateDebug(PathfindingDebugging debug) {
            if (Pathfinder.Controller.Path == null) {
                debug.LineR.Component.positionCount = 0;
            }
            else {
                var cnt = Pathfinder.Controller.Path.Count;
                debug.LineR.Component.positionCount = cnt;
                for (int p = 0; p < cnt; p++) {
                    debug.LineR.Component.SetPosition(p, Pathfinder.Controller.Path[p]);
                }
            }
            debug.Tm.Component.text = Pathfinder.CurrentStatus.ToString();
            if (Pathfinder.IsPathFinished) {
                debug.Tm.Component.text += " At Goal";
            }
        }

        public Vector3 GetWanderPoint() {
            return RandomNavMeshPosition(Pathfinder.Controller.transform.position, 20);
        }

        public Vector3 RandomNavMeshPosition(Vector3 origin, float dist) {
            WhileLoopLimiter.ResetInstance();
            while (WhileLoopLimiter.InstanceAdvance()) {
                Vector3 randDirection = origin + UnityEngine.Random.insideUnitSphere * dist;
                GraphNode node = AstarPath.active.GetNearest(randDirection, NNConstraint.Default).node;
                if (node != null && node.Walkable) {
                    return node.RandomPointOnSurface();
                }
            }
            return Vector3.zero;
        }

        public void StartUpdate() {
            Pathfinder.Controller.FinalizeMovement(Pathfinder.Controller.transform.position, Pathfinder.Controller.transform.rotation);
            //_controller.nextPosition = Pathfinder.Controller.transform.position;
        }

        public void UpdateMovement() {
            Pathfinder.Controller.MovementUpdate(TimeManager.DeltaUnscaled, out var position);
            if (Target.c.HasValidLook) {
                position = Target.c.GetLookTarget;
            }
            var dir = position - Pathfinder.Controller.transform.position;
            var nextRotation = dir != Vector3.zero ? Quaternion.LookRotation(dir, Vector3.up) : Pathfinder.Controller.transform.rotation;
            var look = Vector3.ProjectOnPlane(nextRotation * Vector3.forward, Vector3.up).normalized;
            Entity.Post(new MoveInputMessage(Pathfinder.DesiredVelocity.normalized, look));
        }

    }

    public enum PathfindingStatus {
        Created,
        NoPath,
        WaitingOnPath,
        PathReceived,
        Moving,
        WaitingOnNode,
        InvalidPath,
    }
}
