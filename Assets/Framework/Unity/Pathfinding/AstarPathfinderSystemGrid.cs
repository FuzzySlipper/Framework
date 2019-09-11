#define AStarPathfinding
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

#if AStarPathfinding
namespace PixelComrades {
    public class AstarPathfinderSystemGrid : IPathfindingSource {

        private GraphUpdateObject[] _playerUpdateList;
        private int _currentIndex = 0;
        private GameOptions.CachedInt _playerCost = new GameOptions.CachedInt("PathfindGridPlayerCost");
        private GraphUpdateObject Current { get { return _playerUpdateList[_currentIndex]; } }
        private GraphUpdateObject Previous { get { return _playerUpdateList[_currentIndex == 0 ? 1 : 0]; } }
        private WhileLoopLimiter _findOpenLoop = new WhileLoopLimiter(50);

        public void ClearAll() {

        }

        public void SetCellSize(float size) {
            var graph = (RecastGraph) AstarPath.active.graphs[0];
            graph.cellSize = size;
            graph.characterRadius = Mathf.Max(1.25f, size * 2);
            graph.walkableHeight = graph.characterRadius * 2;
            graph.walkableClimb = graph.walkableHeight * 0.5f;
        }

        public void SetWalkable(Bounds bounds, bool status) {
            AstarPath.active.UpdateGraphs(new GraphUpdateObject(bounds) {
                modifyWalkability = true, setWalkability = status
            });

        }

        public Vector3 FindOpenPosition(Vector3 origin, float dist) {
            _findOpenLoop.Reset();
            while (_findOpenLoop.Advance()) {
                Vector3 randDirection = origin + UnityEngine.Random.insideUnitSphere * dist;
                randDirection.y = origin.y;
                GraphNode node = AstarPath.active.GetNearest(randDirection, NNConstraint.Default).node;
                if (node != null && node.Walkable) {
                    return node.RandomPointOnSurface();
                }
            }
            return origin;
        }

        public void SetPlayerPosition(Point3 pos, int playerID, int impassableSize, int occupiedSize) {
            if (_playerUpdateList == null) {
                _playerUpdateList = new GraphUpdateObject[2];
                _playerUpdateList[0] = new GraphUpdateObject();
                _playerUpdateList[1] = new GraphUpdateObject();
            }
            else {
                _currentIndex = _currentIndex == 0 ? 1 : 0;
                Previous.resetPenaltyOnPhysics = true;
               AstarPath.active.UpdateGraphs(Previous);
            }
            Current.bounds = new Bounds(pos.toVector3(), Vector3.one * occupiedSize);
            Current.addPenalty = _playerCost * 1000;
            AstarPath.active.UpdateGraphs(Current);
        }

        public bool IsWalkable(Vector3 pos, bool isOversized) {
            GraphNode node = AstarPath.active.GetNearest(pos, NNConstraint.Default).node;
            if (node != null && node.Walkable) {
                return true;
            }
            return false;
        }

        public bool IsValidDestination(Vector3 pos) {
            return IsWalkable(pos, false);
        }

        public void Scan() {
            for (int i = 0; i < AstarPath.active.graphs.Length; i++) {
                if (AstarPath.active.graphs[i] is RecastGraph recast) {
                    recast.SnapForceBoundsToScene();
                }
            }
            AstarPath.active.Scan();
        }
    }
}
#endif
