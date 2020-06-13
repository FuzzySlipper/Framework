using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    //Should be thread safe
    public interface IPathfindingGrid : IPathfindingSource {
        bool CanAgentEnter(int id, Point3 pos, bool finalDestination);
        void Exit(int id, Point3 pos);
        void SetWalkable(LevelCell c);
        void ClearLocks();
        void SetStationaryAgent(Point3 pos, int id, bool status);
        void SetAgentCurrentPath(Point3 pos, int id, bool status);
        int CellsCount { get; }
        int GetTraversalCost(Point3 pos);
        void RunActionOnCells(System.Action<Point3, IPathfindingGridCell> del);
        void FinishPathfindingSetup();
    }

    public interface IPathfindingSource {
        void Scan();
        void ClearAll();
        void SetWalkable(Bounds bounds, bool status);
        void SetPlayerPosition(Point3 pos, int playerID, int impassableSize, int occupiedSize);
        bool IsWalkable(Vector3 pos, bool isOversized);
        bool IsValidDestination(Vector3 pos);
        Vector3 FindOpenPosition(Vector3 origin, float dist);
        void SetCellSize(float size);
    }

    public interface IPathfindingGridCell {
        int TraversalCost { get; }
        bool IsWalkable { get; }
        bool IsEdge { get; }
    }
}
