using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct MoveInputMessage : IEntityMessage {
        public Vector3 Move;
        public Vector3 Look;

        public MoveInputMessage(Vector3 move, Vector3 look) {
            Move = move;
            Look = look;
        }
    }

    public struct PhysicsInputMessage : IEntityMessage {
        public Vector3 Force;

        public PhysicsInputMessage(Vector3 force) {
            Force = force;
        }
    }
    public class PathfinderMoverSystem : SystemBase, IMainSystemUpdate {

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
            if (_debugAgents) {
                var spawned = ItemPool.Spawn(UnityDirs.System, "PathfindDebug", Vector3.zero, Quaternion.identity, false, false);
                spawned.transform.SetParent(entity.Tr);
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
                entity.Add(new AstarPathfindingAgent(entity.Tr.GetComponent<AstarRvoController>()));
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
                if (node.Debugging.c != null) {
                    node.Debugging.c.UpdateStatus(pathfinder);
                }
                if (node.Target.c == null) {
                    node.Entity.Post(new MoveInputMessage(Vector3.zero, Vector3.zero));
                    continue;
                }
                if (!node.Target.c.IsValidMove) {
                    node.Pathfinder.ReachedDestination();
                    node.Entity.Post(new MoveInputMessage(Vector3.zero, Vector3.zero));
                    continue;
                }
                var currentMoveTarget = node.Target.c.GetTargetPosition.toPoint3();
                if (pathfinder.End != currentMoveTarget) {
                    pathfinder.SetEnd(currentMoveTarget);
                    node.Entity.Post(new MoveInputMessage(Vector3.zero, Vector3.zero));
                    continue;
                }
                switch (pathfinder.CurrentStatus) {
                    case PathfindingStatus.NoPath:
                        if (pathfinder.Redirected && pathfinder.CanRepath) {
                            pathfinder.SearchPath();
                        }
                        node.Entity.Post(new MoveInputMessage(Vector3.zero, Vector3.zero));
                        continue;
                    case PathfindingStatus.InvalidPath:
                    case PathfindingStatus.Created:
                    case PathfindingStatus.WaitingOnPath:
                        continue;
                    case PathfindingStatus.PathReceived:
                    case PathfindingStatus.WaitingOnNode:
                        if (pathfinder.CurrentStatus == PathfindingStatus.PathReceived) {
                            if (node.Debugging.c != null) {
                                node.Debugging.c.SetPath(pathfinder.CurrentNodePath);
                            }
                            _grid.SetStationaryAgent(pathfinder.CurrentPos, pathfinder.Entity, false);
                        }
                        if (!node.Pathfinder.TryEnterNextNode()) {
                            node.Entity.Post(new MoveInputMessage(Vector3.zero, Vector3.zero));
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
                var diff = (pos - node.Entity.Tr.position);
                var nextRotation = dir != Vector3.zero ? Quaternion.LookRotation(dir, Vector3.up) : pathfinder.Entity.Tr.rotation;
                //var rot = Quaternion.RotateTowards(pathfinder.Entity.Tr.rotation, nextRotation, _pathfinderRotationSpeed * dt);
                Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(nextRotation * Vector3.forward, Vector3.up).normalized;
                node.Entity.Post(new MoveInputMessage(diff.normalized, cameraPlanarDirection));
                if (progress < 1) {
                    continue;
                }
                _grid.Exit(node.Entity, pathfinder.GridTarget);
                pathfinder.CurrentPos = pathfinder.GridTarget;
                if (pathfinder.CurrentIndex >= pathfinder.CurrentNodePath.Count - 1) {
                    node.Pathfinder.ReachedDestination();
                    node.Target.c.Complete();
                    if (node.Debugging.c != null) {
                        node.Debugging.c.ClearPath();
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
                if (node.Debugging.c != null) {
                    nav.UpdateDebug(node.Debugging.c);
                }
                if (node.Target.c == null || node.Entity.Tags.Contain(EntityTags.CantMove)) {
                    nav.ProcessNoMove();
                    continue;
                }
                if (!node.Target.c.IsValidMove) {
                    nav.ReachedDestination();
                    nav.ProcessNoMove();
                    continue;
                }
                var currentMoveTarget = node.Target.c.GetTargetPosition;
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
                        node.Target.c.Complete();
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
                    node.Target.c.Complete();
                    continue;
                }
                nav.UpdateMovement();
                var pos = node.Entity.Tr.position.toPoint3();
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
                            DebugExtension.DebugCircle(node.Entity.Tr.position, Color.red, 2f, 5f);
                            node.Entity.Post(new SetMoveTarget(null, nav.GetWanderPoint()));
                        }
                    }
                }
            }
        }
    }
}
