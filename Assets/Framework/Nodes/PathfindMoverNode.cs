#define AStarPathfinding
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

namespace PixelComrades {
    public class SimplePathfindMoverNode : BaseNode {

        private CachedComponent<SimplePathfindingAgent> _pathfinder = new CachedComponent<SimplePathfindingAgent>();
        private CachedComponent<MoveSpeed> _moveSpeed = new CachedComponent<MoveSpeed>();
        private CachedComponent<RotationSpeed> _rotationSpeed = new CachedComponent<RotationSpeed>();
        private CachedComponent<MoveTarget> _target = new CachedComponent<MoveTarget>();
        private CachedComponent<PathfindingDebugging> _debugging = new CachedComponent<PathfindingDebugging>();
        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<SteeringInput> _moveInput = new CachedComponent<SteeringInput>();
        
        public SimplePathfindingAgent Pathfinder { get { return _pathfinder.Value; } }
        public TransformComponent Tr { get => _tr.Value; }
        public SteeringInput Steering => _moveInput.Value;
        public MoveSpeed MoveSpeed { get => _moveSpeed; }
        public RotationSpeed RotationSpeed { get => _rotationSpeed; }
        public MoveTarget Target { get => _target; }
        public PathfindingDebugging Debugging { get => _debugging; }
        public SteeringInput MoveInput { get => _moveInput; }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _pathfinder, _moveSpeed, _rotationSpeed, _target, _debugging, _tr, _moveInput
        };
        
        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(SimplePathfindingAgent), 
                typeof(MoveTarget),
                typeof(TransformComponent),
                typeof(SteeringInput),
            };
        }

        public float GetMoveSpeed { get { return _moveSpeed.Value?.Speed ?? 1; } }
    }

    public class AstarPathfindMoverNode : BaseNode {
#if AStarPathfinding
        private CachedComponent<AstarPathfindingAgent> _pathfinder = new CachedComponent<AstarPathfindingAgent>();
#endif
        private CachedComponent<MoveTarget> _target = new CachedComponent<MoveTarget>();
        private CachedComponent<PathfindingDebugging> _debugging = new CachedComponent<PathfindingDebugging>();
        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<SteeringInput> _moveInput = new CachedComponent<SteeringInput>();
        
        public SteeringInput SteeringInput => _moveInput.Value;
        public TransformComponent Tr { get => _tr.Value; }
        public AstarPathfindingAgent Pathfinder { get { return _pathfinder.Value; } }
        public Point3 DestinationP3 { get { return Pathfinder.DestinationP3; } }
        public PathfindingStatus CurrentStatus { get { return Pathfinder.CurrentStatus; } }
        public MoveTarget Target { get => _target; }
        public PathfindingDebugging Debugging { get => _debugging; }
        public SteeringInput MoveInput { get => _moveInput; }
        public bool IsPathFinished { get { return Pathfinder.Controller.ReachedEndOfPath; } }

        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _target, _debugging, _tr, _moveInput,
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
                typeof(TransformComponent),
                typeof(SteeringInput),
            };
        }

        
        public void ProcessNoMove() {
            var look = Vector3.zero;
            if (_target.Value.HasValidLook) {
                var dir = _target.Value.GetLookTarget - Pathfinder.Controller.transform.position;
                var nextRotation = dir != Vector3.zero ? Quaternion.LookRotation(dir, Vector3.up) : Pathfinder.Controller.transform.rotation;
                look = Vector3.ProjectOnPlane(nextRotation * Vector3.forward, Vector3.up).normalized;
            }
            SteeringInput.Look = look;
            SteeringInput.Move = Vector3.zero;
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
                debug.LineR.Value.positionCount = 0;
            }
            else {
                var cnt = Pathfinder.Controller.Path.Count;
                debug.LineR.Value.positionCount = cnt;
                for (int p = 0; p < cnt; p++) {
                    debug.LineR.Value.SetPosition(p, Pathfinder.Controller.Path[p]);
                }
            }
            debug.Tm.Value.text = Pathfinder.CurrentStatus.ToString();
            if (Pathfinder.IsPathFinished) {
                debug.Tm.Value.text += " At Goal";
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
            if (_target.Value.HasValidLook) {
                position = _target.Value.GetLookTarget;
            }
            var dir = position - Pathfinder.Controller.transform.position;
            var nextRotation = dir != Vector3.zero ? Quaternion.LookRotation(dir, Vector3.up) : Pathfinder.Controller.transform.rotation;
            var look = Vector3.ProjectOnPlane(nextRotation * Vector3.forward, Vector3.up).normalized;
            SteeringInput.Look = look;
            SteeringInput.Move = Pathfinder.DesiredVelocity.normalized;
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
