using System.Collections.Generic;

namespace PixelComrades {
    public class PathfindingRequest {

        private static Queue<PathfindingRequest> _pooled = new Queue<PathfindingRequest>();

        public Point3 Start { get; private set; }
        public Point3 End { get; private set; }
        public PathFound ReturnEvent { get; private set; }
        public int ID { get; private set; }
        public bool IsOversized { get; private set; }
        public IPathfindingGrid Grid { get; private set; }
        public List<Point3> Path;

        private PathfindingRequest(IPathfindingGrid grid, int id, Point3 start, Point3 end, PathFound foundEvent, bool overSized, List<Point3> path) {
            Start = start;
            End = end;
            ReturnEvent = foundEvent;
            Path = path;
            Grid = grid;
            ID = id;
            IsOversized = overSized;
        }

        public static PathfindingRequest Create(IPathfindingGrid grid, int id, Point3 start, Point3 end, PathFound foundEvent, bool overSized, List<Point3> path) {
            PathfindingRequest r;
            if (_pooled.Count > 0) {
                r = _pooled.Dequeue();
                r.Start = start;
                r.End = end;
                r.ReturnEvent = foundEvent;
                r.Path = path;
                r.ID = id;
                r.Grid = grid;
                r.IsOversized = overSized;
            }
            else {
                r = new PathfindingRequest(grid, id, start, end, foundEvent, overSized, path);
            }
            World.Get<PathfindingSystem>().Enqueue(r);
            return r;
        }

        public void Dispose() {
            ReturnEvent = null;
            Path = null;
            if (!_pooled.Contains(this)) {
                _pooled.Enqueue(this);
            }
        }
    }
}