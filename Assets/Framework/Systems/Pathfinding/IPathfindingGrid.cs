using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    //Should be thread safe
    public interface IPathfindingGrid {
        bool CanAgentEnter(int id, Point3 pos, bool finalDestination);
        void Exit(int id, Point3 pos);
        void SetWalkable(Point3 pos, bool status);
        void SetWalkable(Bounds bounds, bool status);
        void SetWalkable(BaseCell c);
        void ClearAll();
        void ClearLocks();
        void SetStationaryAgent(Point3 pos, int id, bool status);
        bool IsWalkable(Point3 pos, bool isOversized);
        bool IsValidDestination(Point3 pos);
        bool HasCurrentAgentPath(Point3 pos);
        void SetAgentCurrentPath(Point3 pos, int id, bool status);
        int CellsCount { get; }
        int GetTraversalCost(Point3 pos);
        void SetPlayerPosition(Point3 pos, int playerID, int impassableSize, int occupiedSize);
        Point3 GetOpenWalkablePosition();
        void RunActionOnCells(System.Action<Point3, IPathfindingGridCell> del);
        void FinishPathfindingSetup();
    }

    public interface IPathfindingGridCell {
        int TraversalCost { get; }
        bool IsWalkable { get; }
        bool IsEdge { get; }
    }
}
