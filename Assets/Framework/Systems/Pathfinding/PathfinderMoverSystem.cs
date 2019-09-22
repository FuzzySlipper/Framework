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

        private List<SimplePathfindMoverNode> _simpleNodeList;
        private List<AstarPathfindMoverNode> _astarNodeList;
        private IPathfindingGrid _grid;

        public PathfinderMoverSystem() {
            EntityController.RegisterReceiver<AstarPathfindingAgent>(this);
            EntityController.RegisterReceiver<SimplePathfindingAgent>(this);
            if (UseSimple) {
                NodeFilter<SimplePathfindMoverNode>.New(SimplePathfindMoverNode.GetTypes());
            }
            else {
                NodeFilter<AstarPathfindMoverNode>.New(AstarPathfindMoverNode.GetTypes());
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
            if (_simpleNodeList == null) {
                _simpleNodeList = EntityController.GetNodeList<SimplePathfindMoverNode>();
            }
            if (_grid == null) {
                _grid = World.Get<PathfindingSystem>().Grid;
            }
            if (_simpleNodeList == null || Game.Paused) {
                return;
            }
            for (int i = 0; i < _simpleNodeList.Count; i++) {
                var node = _simpleNodeList[i];
                var pathfinder = node.Pathfinder;
                if (node.Debugging.Value != null) {
                    node.Debugging.Value.UpdateStatus(pathfinder);
                }
                if (node.Target.Value == null) {
                    node.Steering.Reset();
                    continue;
                }
                if (!node.Target.Value.IsValidMove) {
                    node.Pathfinder.ReachedDestination();
                    node.Steering.Reset();
                    continue;
                }
                var currentMoveTarget = node.Target.Value.GetTargetPosition.toPoint3();
                if (pathfinder.End != currentMoveTarget) {
                    pathfinder.SetEnd(currentMoveTarget);
                    node.Steering.Reset();
                    continue;
                }
                switch (pathfinder.CurrentStatus) {
                    case PathfindingStatus.NoPath:
                        if (pathfinder.Redirected && pathfinder.CanRepath) {
                            pathfinder.SearchPath();
                        }
                        node.Steering.Reset();
                        continue;
                    case PathfindingStatus.InvalidPath:
                    case PathfindingStatus.Created:
                    case PathfindingStatus.WaitingOnPath:
                        continue;
                    case PathfindingStatus.PathReceived:
                    case PathfindingStatus.WaitingOnNode:
                        if (pathfinder.CurrentStatus == PathfindingStatus.PathReceived) {
                            if (node.Debugging.Value != null) {
                                node.Debugging.Value.SetPath(pathfinder.CurrentNodePath);
                            }
                            _grid.SetStationaryAgent(pathfinder.CurrentPos, pathfinder.GetEntity(), false);
                        }
                        if (!node.Pathfinder.TryEnterNextNode()) {
                            node.Steering.Reset();
                            continue;
                        }
                        break;
                }
                var moveSpeed = node.GetMoveSpeed;
                if (pathfinder.IsLastIndex) {
                    moveSpeed *= 0.5f;
                }
                pathfinder.MovementLerp += dt * moveSpeed;
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
                    continue;
                }
                _grid.Exit(node.Entity, pathfinder.GridTarget);
                pathfinder.CurrentPos = pathfinder.GridTarget;
                if (pathfinder.CurrentIndex >= pathfinder.CurrentNodePath.Count - 1) {
                    node.Pathfinder.ReachedDestination();
                    node.Target.Value.Complete();
                    if (node.Debugging.Value != null) {
                        node.Debugging.Value.ClearPath();
                    }
                    continue;
                }
                pathfinder.AdvancePath();
                if (pathfinder.CanRepath) {
                    pathfinder.SearchPath();
                    continue;
                }
                pathfinder.TryEnterNextNode();
            }
        }

        private void UpdateAstarNodeList(float dt) {
            if (_astarNodeList == null) {
                _astarNodeList = EntityController.GetNodeList<AstarPathfindMoverNode>();
            }
            if (_astarNodeList == null || Game.Paused) {
                return;
            }
            for (int i = 0; i < _astarNodeList.Count; i++) {
                var node = _astarNodeList[i];
                var nav = node;
                nav.StartUpdate();
                if (node.Debugging.Value != null) {
                    nav.UpdateDebug(node.Debugging.Value);
                }
                if (node.Target.Value == null || node.Entity.Tags.Contain(EntityTags.CantMove)) {
                    nav.ProcessNoMove();
                    continue;
                }
                if (!node.Target.Value.IsValidMove) {
                    nav.ReachedDestination();
                    nav.ProcessNoMove();
                    continue;
                }
                var currentMoveTarget = node.Target.Value.GetTargetPosition;
                if (nav.DestinationP3 != currentMoveTarget.toPoint3()) {
                    nav.SetDestination(currentMoveTarget);
                    nav.ProcessNoMove();
                    continue;
                }
                switch (nav.CurrentStatus) {
                    case PathfindingStatus.NoPath:
                        nav.ProcessNoMove();
                        continue;
                    case PathfindingStatus.InvalidPath:
                        node.Target.Value.Complete();
                        nav.Stop();
                        continue;
                    case PathfindingStatus.Created:
                    case PathfindingStatus.WaitingOnPath:
                        nav.ProcessNoMove();
                        continue;
                    case PathfindingStatus.PathReceived:
                    case PathfindingStatus.WaitingOnNode:
                        nav.CheckPathWaiting();
                        break;
                }
                if (nav.CurrentStatus != PathfindingStatus.Moving) {
                    nav.ProcessNoMove();
                    continue;
                }
                if (nav.IsPathFinished) {
                    nav.ReachedDestination();
                    node.Target.Value.Complete();
                    continue;
                }
                nav.UpdateMovement();
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
