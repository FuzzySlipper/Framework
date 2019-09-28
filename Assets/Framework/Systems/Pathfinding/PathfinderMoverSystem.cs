using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct PhysicsInputMessage : IEntityMessage {
        public Vector3 Force { get; }
        public Entity Target { get; }

        public PhysicsInputMessage(Entity target,Vector3 force) {
            Force = force;
            Target = target;
        }
    }
    
    [AutoRegister]
    public class PathfinderMoverSystem : SystemBase, IMainSystemUpdate, IReceive<ChangePositionEvent> {

        public static bool UseSimple = false;

        private GameOptions.CachedFloat _pathfinderRotationSpeed = new GameOptions.CachedFloat("PathfinderRotationSpeed");
        private GameOptions.CachedFloat _pathfinderMoveSpeed = new GameOptions.CachedFloat("PathfinderMoveSpeed");
        private GameOptions.CachedFloat _pathfinderRepathTime = new GameOptions.CachedFloat("PathfinderRepathTime");
        private GameOptions.CachedBool _debugAgents = new GameOptions.CachedBool("DebugPathfindAgents");

        private const float MaxStuckTime = 1.5f;

        private NodeList<SimplePathfindMoverNode> _simpleNodeList;
        private NodeList<AstarPathfindMoverNode> _astarNodeList;
        private IPathfindingGrid _grid;

        public PathfinderMoverSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(AstarPathfindingAgent), typeof(SimplePathfindingAgent),
            }));
            if (UseSimple) {
                NodeFilter<SimplePathfindMoverNode>.Setup(SimplePathfindMoverNode.GetTypes());
                _simpleNodeList = EntityController.GetNodeList<SimplePathfindMoverNode>();
            }
            else {
                NodeFilter<AstarPathfindMoverNode>.Setup(AstarPathfindMoverNode.GetTypes());
                _astarNodeList = EntityController.GetNodeList<AstarPathfindMoverNode>();
            }
        }

        public override void Dispose() {
            base.Dispose();
            if (_simpleNodeList != null) {
                _simpleNodeList.Clear();
            }
            if (_astarNodeList != null) {
                _astarNodeList.Clear();
            }
        }

        public void SetupPathfindEntity(Entity entity, bool isOversized) {
            var tr = entity.Get<TransformComponent>();
            if (_debugAgents) {
                var spawned = ItemPool.Spawn(UnityDirs.System, "PathfindDebug", Vector3.zero, Quaternion.identity, false, false);
                tr.SetChild(spawned.transform);
                spawned.transform.localPosition = new Vector3(0, 2, 0);
                spawned.transform.localRotation = Quaternion.Euler(90, 0 ,0);
                entity.Add(new PathfindingDebugging(spawned.GetComponentInChildren<LineRenderer>(), spawned.GetComponentInChildren<TextMesh>()));
            }
            if (UseSimple) {
                if (!entity.HasComponent<MoveSpeed>()) {
                    entity.Add(new MoveSpeed(_pathfinderMoveSpeed));
                }
                var simpleAgent = entity.Add(new SimplePathfindingAgent(World.Get<PathfindingSystem>().Grid, _pathfinderRepathTime));
                simpleAgent.IsOversized = isOversized;
            }
            else {
                entity.Add(new AstarPathfindingAgent(tr.gameObject.GetComponent<AstarRvoController>()));
            }
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            if (UseSimple) {
                UpdateSimpleNodeList(dt);
            }
            else {
                UpdateAstarNodeList(dt);
            }
        }

        private void UpdateSimpleNodeList(float dt) {
            if (_grid == null) {
                _grid = World.Get<PathfindingSystem>().Grid;
            }
            if (Game.Paused) {
                return;
            }
            _simpleNodeList.Run(UpdateNodeList);
        }

        private void UpdateNodeList(ref SimplePathfindMoverNode node) {
            var pathfinder = node.Pathfinder;
            if (node.Debugging != null) {
                node.Debugging.UpdateStatus(pathfinder);
            }
            if (node.Target == null) {
                node.Steering.Reset();
                return;
            }
            if (!node.Target.IsValidMove) {
                node.Pathfinder.ReachedDestination();
                node.Steering.Reset();
                return;
            }
            var currentMoveTarget = node.Target.GetTargetPosition.toPoint3();
            if (pathfinder.End != currentMoveTarget) {
                pathfinder.SetEnd(currentMoveTarget);
                node.Steering.Reset();
                return;
            }
            switch (pathfinder.CurrentStatus) {
                case PathfindingStatus.NoPath:
                    if (pathfinder.Redirected && pathfinder.CanRepath) {
                        pathfinder.SearchPath();
                    }
                    node.Steering.Reset();
                    return;
                case PathfindingStatus.InvalidPath:
                case PathfindingStatus.Created:
                case PathfindingStatus.WaitingOnPath:
                    return;
                case PathfindingStatus.PathReceived:
                case PathfindingStatus.WaitingOnNode:
                    if (pathfinder.CurrentStatus == PathfindingStatus.PathReceived) {
                        if (node.Debugging != null) {
                            node.Debugging.SetPath(pathfinder.CurrentNodePath);
                        }
                        _grid.SetStationaryAgent(pathfinder.CurrentPos, pathfinder.GetEntity(), false);
                    }
                    if (!node.Pathfinder.TryEnterNextNode()) {
                        node.Steering.Reset();
                        return;
                    }
                    break;
            }
            var moveSpeed = node.GetMoveSpeed;
            if (pathfinder.IsLastIndex) {
                moveSpeed *= 0.5f;
            }
            pathfinder.MovementLerp += TimeManager.DeltaTime * moveSpeed;
            float dst = pathfinder.GetCurrentDistance();
            float progress = Mathf.Clamp01(pathfinder.MovementLerp / dst);
            var pos = Vector3.Lerp(pathfinder.PreviousTarget, pathfinder.CurrentTarget, progress);
            var dir = pathfinder.CurrentTarget - pathfinder.PreviousTarget;
            var diff = (pos - node.Tr.position);
            var nextRotation = dir != Vector3.zero ? Quaternion.LookRotation(dir, Vector3.up) : node.Tr.rotation;
            //var rot = Quaternion.RotateTowards(pathfinder.Entity.Tr.rotation, nextRotation, _pathfinderRotationSpeed * dt);
            Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(nextRotation * Vector3.forward, Vector3.up).normalized;
            node.Steering.Set(diff.normalized, cameraPlanarDirection);
            if (progress < 1) {
                return;
            }
            _grid.Exit(node.Entity, pathfinder.GridTarget);
            pathfinder.CurrentPos = pathfinder.GridTarget;
            if (pathfinder.CurrentIndex >= pathfinder.CurrentNodePath.Count - 1) {
                node.Pathfinder.ReachedDestination();
                node.Target.Complete();
                if (node.Debugging != null) {
                    node.Debugging.ClearPath();
                }
                return;
            }
            pathfinder.AdvancePath();
            if (pathfinder.CanRepath) {
                pathfinder.SearchPath();
                return;
            }
            pathfinder.TryEnterNextNode();
        }

        private void UpdateAstarNodeList(float dt) {
            if (Game.Paused) {
                return;
            }
            _astarNodeList.Run(UpdateNodeList);
        }

        private void UpdateNodeList(ref AstarPathfindMoverNode node) {
            var nav = node;
            nav.StartUpdate();
            if (node.Debugging != null) {
                nav.UpdateDebug(node.Debugging);
            }
            if (node.Target == null || node.Entity.Tags.Contain(EntityTags.CantMove)) {
                nav.ProcessNoMove();
                return;
            }
            if (!node.Target.IsValidMove) {
                nav.ReachedDestination();
                nav.ProcessNoMove();
                return;
            }
            var currentMoveTarget = node.Target.GetTargetPosition;
            if (nav.DestinationP3 != currentMoveTarget.toPoint3()) {
                nav.SetDestination(currentMoveTarget);
                nav.ProcessNoMove();
                return;
            }
            switch (nav.CurrentStatus) {
                case PathfindingStatus.NoPath:
                    nav.ProcessNoMove();
                    return;
                case PathfindingStatus.InvalidPath:
                    node.Target.Complete();
                    nav.Stop();
                    return;
                case PathfindingStatus.Created:
                case PathfindingStatus.WaitingOnPath:
                    nav.ProcessNoMove();
                    return;
                case PathfindingStatus.PathReceived:
                case PathfindingStatus.WaitingOnNode:
                    nav.CheckPathWaiting();
                    break;
            }
            if (nav.CurrentStatus != PathfindingStatus.Moving) {
                nav.ProcessNoMove();
                return;
            }
            if (nav.IsPathFinished) {
                nav.ReachedDestination();
                node.Target.Complete();
                return;
            }
            nav.UpdateMovement();
            if (node.Tr == null) {
                Debug.LogWarningFormat("Entity {0} has no TR but has a pathfinding node {1}", node.Entity.DebugId, EntityController.GetNode<
                    AstarPathfindMoverNode>(node.Entity) != null);
                return;
            }
            var pos = node.Tr.position.toPoint3();
            if (pos != nav.Pathfinder.LastPosition) {
                nav.Pathfinder.LastPosition = pos;
                nav.Pathfinder.LastPositionTime = TimeManager.TimeUnscaled;
            }
            else {
                if (TimeManager.TimeUnscaled - nav.Pathfinder.LastPositionTime > MaxStuckTime) {
                    nav.Pathfinder.StuckPathCount++;
                    if (nav.Pathfinder.StuckPathCount < 4) {
                        nav.SetDestination(nav.Pathfinder.Destination);
                    }
                    else {
                        nav.ProcessNoMove();
                        DebugExtension.DebugCircle(node.Tr.position, Color.red, 2f, 5f);
                        node.Entity.Post(new SetMoveTarget(node.Entity,null, nav.GetWanderPoint()));
                    }
                }
            }
        }

        public void Handle(ChangePositionEvent arg) {
            var astarPathfinding = arg.Target.Get<AstarPathfindingAgent>();
            if (astarPathfinding != null) {
                astarPathfinding.Controller.Teleport(arg.Position);
                return;
            }
            var simple = arg.Target.Get<SimplePathfindingAgent>();
            if (simple != null) {
                arg.Target.Post(new SetTransformPosition(arg.Target.Get<TransformComponent>(), arg.Position 
                    + new Vector3(0, -(Game.MapCellSize * 0.5f))));
                simple. SetPosition(arg.Position.toPoint3());
            }
        }
    }
}
