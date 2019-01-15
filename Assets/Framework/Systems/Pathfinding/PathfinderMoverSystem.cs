using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PathfinderMoverSystem : SystemBase, IMainSystemUpdate {

        //private const int _maxPathUpdates = 10;
        private GameOptions.CachedFloat _pathfinderRotationSpeed = new GameOptions.CachedFloat("PathfinderRotationSpeed");
        private GameOptions.CachedFloat _pathfinderMoveSpeed = new GameOptions.CachedFloat("PathfinderMoveSpeed");
        private GameOptions.CachedFloat _pathfinderRepathTime = new GameOptions.CachedFloat("PathfinderRepathTime");
        private GameOptions.CachedBool _debugAgents = new GameOptions.CachedBool("DebugPathfindAgents");

        private List<PathfindMoverNode> _nodeList;
        private IPathfindingGrid _grid;

        public PathfinderMoverSystem() {
            _grid = World.Get<PathfindingSystem>().Grid;
            NodeFilter<PathfindMoverNode>.New(PathfindMoverNode.GetTypes());
        }

        public override void Dispose() {
            base.Dispose();
            _nodeList.Clear();
        }

        public SimplePathfindingAgent SetupPathfindEntity(Entity entity) {
            if (!entity.HasComponent<MoveSpeed>()) {
                entity.Add(new MoveSpeed(_pathfinderMoveSpeed));
            }
            if (_debugAgents) {
                var spawned = ItemPool.Spawn(UnityDirs.System, "PathfindDebug", Vector3.zero, Quaternion.identity, false, false);
                spawned.transform.SetParent(entity.Tr);
                spawned.transform.localPosition = new Vector3(0, 2, 0);
                spawned.transform.localRotation = Quaternion.Euler(90, 0 ,0);
                entity.Add(new PathfindingDebugging(spawned.GetComponentInChildren<LineRenderer>(), spawned.GetComponentInChildren<TextMesh>()));
            }
            return entity.Add(new SimplePathfindingAgent(_grid, _pathfinderRepathTime));
        }

        public void OnSystemUpdate(float dt) {
            if (_nodeList == null) {
                _nodeList = EntityController.GetNodeList<PathfindMoverNode>();
            }
            if (_nodeList == null || Game.Paused) {
                return;
            }
            for (int i = 0; i < _nodeList.Count; i++) {
                var node = _nodeList[i];
                var pathfinder = node.Pathfinder.c;
                if (node.Debugging.c != null) {
                    node.Debugging.c.UpdateStatus(pathfinder);
                }
                if (!node.Target.c.IsValid) {
                    node.Pathfinder.c.ReachedDestination();
                    continue;
                }
                var currentMoveTarget = node.Target.c.GetTargetPosition.toPoint3();
                if (pathfinder.End != currentMoveTarget) {
                    pathfinder.SetEnd(currentMoveTarget);
                    continue;
                }
                switch (pathfinder.CurrentStatus) {
                    case SimplePathfindingAgent.Status.NoPath:
                        if (pathfinder.Redirected && pathfinder.CanRepath) {
                            pathfinder.SearchPath();
                        }
                        continue;
                    case SimplePathfindingAgent.Status.InvalidPath:
                    case SimplePathfindingAgent.Status.Created:
                    case SimplePathfindingAgent.Status.WaitingOnPath:
                        continue;
                    case SimplePathfindingAgent.Status.PathReceived:
                    case SimplePathfindingAgent.Status.WaitingOnNode:
                        if (pathfinder.CurrentStatus == SimplePathfindingAgent.Status.PathReceived) {
                            if (node.Debugging.c != null) {
                                node.Debugging.c.SetPath(pathfinder.CurrentNodePath);
                            }
                            _grid.SetStationaryAgent(pathfinder.CurrentPos, pathfinder.Entity, false);
                        }
                        if (!node.Pathfinder.c.TryEnterNextNode()) {
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
                node.Entity.Tr.position = Vector3.Lerp(pathfinder.PreviousTarget, pathfinder.CurrentTarget, progress);
                var dir = pathfinder.CurrentTarget - pathfinder.PreviousTarget;
                var nextRotation = dir != Vector3.zero ? Quaternion.LookRotation(dir, Vector3.up) : pathfinder.Entity.Tr.rotation;
                pathfinder.Entity.Tr.rotation = Quaternion.RotateTowards(pathfinder.Entity.Tr.rotation, nextRotation, _pathfinderRotationSpeed * dt);
                if (progress < 1) {
                    continue;
                }
                _grid.Exit(node.Entity, pathfinder.GridTarget);
                pathfinder.CurrentPos = pathfinder.GridTarget;
                if (pathfinder.CurrentIndex >= pathfinder.CurrentNodePath.Count - 1) {
                    node.Pathfinder.c.ReachedDestination();
                    node.Target.c.Clear();
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
    }
}
