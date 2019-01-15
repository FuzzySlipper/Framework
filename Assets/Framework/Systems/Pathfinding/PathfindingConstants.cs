using System.Collections.Generic;

namespace PixelComrades {
    public enum PathfindingResult : byte {
        Successful,
        Cancelled,
        Redirected,
        ErrorStartOutOfBounds,
        ErrorEndOutOfBounds,
        ErrorStartIsEnd,
        ErrorPathTooLong,
        ErrorStartNotWalkable,
        ErrorEndNotWalkable,
        ErrorPathNotFound,
        ErrorInternal
    }

    public delegate void PathFound(PathfindingResult result, List<Point3> nodes);

    public struct PathReturn {
        public int ID;
        public PathfindingResult Result;
        public List<Point3> Path;
        public PathFound Callback;
        public IPathfindingGrid Grid;

        public PathReturn(int id, PathfindingResult result, List<Point3> path, PathFound callback, IPathfindingGrid grid) {
            ID = id;
            Result = result;
            Path = path;
            Callback = callback;
            Grid = grid;
        }
    }
}

