using System;
using UnityEngine;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Priority_Queue;

namespace PixelComrades {

    public class AstarP3Pathfinder : IPathfinder {

        public static int TravelAxis { get; private set; } = 1;
        public static int OverrideY = 0;

        private static Point3[] _positions = new[] {
            new Point3(0, 1, 0), new Point3(1, 0, 0),
            new Point3(0, -1, 0), new Point3(-1, 0, 0)
        };
        private static Point3[] _diagonalPositions = new[] {
            new Point3(1, 1, 0), new Point3(1, -1, 0),
            new Point3(-1, -1, 0), new Point3(-1, 1, 0)
        };
        private static int[,] _diagonalDirections = new int[4,2] {
            {0, 1}, {1, 2},
            {2, 3}, {3, 0}
        };

        private const int MaxPathCheck = 5500;

        private FastPriorityQueue<P3Node> _openSet = new FastPriorityQueue<P3Node>(MaxPathCheck);
        private Dictionary<Point3, P3Node> _keyedDict = new Dictionary<Point3, P3Node>(MaxPathCheck);
        private HashSet<Point3> _closedSet = new HashSet<Point3>();
        private Point3 _start;
        private Point3 _end;
        private WhileLoopLimiter _loopLimiter = new WhileLoopLimiter(MaxPathCheck);
        private bool[] _validPos = new bool[4];
        private GenericPool<P3Node> _nodePool = new GenericPool<P3Node>(2500, node => node.Clear());
        private SimpleThreadSafeGrid _grid;
        private PathfindingRequest _request;
        private List<Point3> _neighbors = new List<Point3>();

        //Not thread safe either
        public Dictionary<Point3, P3Node> KeyedDict { get => _keyedDict; }
        
        /// <summary>
        /// Not thread safe
        /// </summary>
        /// <param name="axis"></param>
        public static void SetAxis(int axis) {
            TravelAxis = axis;
            _positions[0] = _positions[2] = Point3.zero;
            _positions[0][axis] = 1;
            _positions[2][axis] = -1;
            for (int i = 0; i < _diagonalPositions.Length; i++) {
                _diagonalPositions[i] = _positions[_diagonalDirections[i, 0]] + _positions[_diagonalDirections[i, 1]];
            }
        }

        public void Clear() {
            foreach (var p3Node in _keyedDict) {
                _nodePool.Store(p3Node.Value);
            }
            _openSet.Clear();
            _closedSet.Clear();
            _keyedDict.Clear();
            _grid = null;
        }

        public PathfindingResult Run(PathfindingRequest request) {
            Clear();
            _grid = request.Grid as SimpleThreadSafeGrid;
            _request = request;
            _start = request.Start;
            _end = request.End;
            _start.y = _end.y = OverrideY;
            if (_grid == null) {
                return PathfindingResult.ErrorInternal;
            }
            if (!_grid.IsWalkable(_start, request.IsOversized)) {
                return PathfindingResult.ErrorStartNotWalkable;
            }
            if (!_grid.IsWalkable(_end, request.IsOversized)) {
                if (!FindClosestWalkable(out var newEnd)) {
                    return PathfindingResult.ErrorEndOutOfBounds;
                }
                _end = newEnd;
            }
            if (request.Start == request.End) {
                return PathfindingResult.ErrorStartIsEnd;
            }
            return FindPath();
        }

        public PathfindingResult FindPath() {
            var startNode = CreateWalkableNode(_start);
            startNode.StartCost = 0;
            _openSet.Enqueue(startNode, startNode.TotalCost);
            _loopLimiter.Reset(MaxPathCheck);
            while (_loopLimiter.Advance()) {
                if (_openSet.Count == 0) {
                    break;
                }
                var centerNode = _openSet.Dequeue();
                if (centerNode.Value == _end) {
                    return ConstructFinalPath(centerNode);
                }
                _closedSet.Add(centerNode.Value);
                GetSurrounding(centerNode);
                for (int i = 0; i < _neighbors.Count; i++) {
                    var neighborPos = _neighbors[i];
                    if (_closedSet.Contains(neighborPos)) {
                        continue;
                    }
                    _keyedDict.TryGetValue(neighborPos, out var neighbor);
                    if (neighbor == null) {
                        if (_grid.IsWalkable(neighborPos, _request.IsOversized)) {
                            neighbor = CreateWalkableNode(neighborPos);
                        }
                        else {
                            _closedSet.Add(neighborPos);
                        }
                    }
                    if (neighborPos == _end) {
                        if (neighbor == null) {
                            return ConstructFinalPath(centerNode);
                        }
                        neighbor.Parent = centerNode;
                        return ConstructFinalPath(neighbor);
                    }
                    if (neighbor == null) {
                        continue;
                    }
                    var newStartCost = centerNode.StartCost + neighbor.Cost; // neighbor.GetTravelCost(centerNode);
                    if (newStartCost < neighbor.StartCost) {
                        neighbor.Parent = centerNode;
                        neighbor.StartCost = newStartCost;
                        if (_openSet.Contains(neighbor)) {
                            _openSet.UpdatePriority(neighbor, neighbor.TotalCost);
                        }
                        else {
                            _openSet.Enqueue(neighbor, neighbor.TotalCost);
                        }
                    }
                }
            }
            return PathfindingResult.ErrorPathNotFound;
        }

        private PathfindingResult ConstructFinalPath(P3Node endNode) {
            if (endNode == null) {
                return PathfindingResult.ErrorPathNotFound;
            }
            if (_grid.IsValidDestination(endNode.Value)) {
                PathTrace(endNode);
                return PathfindingResult.Successful;
            }
            if (endNode.Parent != null && _grid.IsValidDestination(endNode.Parent.Value)) {
                PathTrace(endNode.Parent);
                return PathfindingResult.Redirected;
            }
            var originalEnd = endNode;
            var endPos = endNode.Value;
            endNode = null;
            for (int i = 0; i < SpiralPoints.Length; i++) {
                var pos = endPos + SpiralPoints[i];
                if (!_keyedDict.TryGetValue(pos, out var newNode)) {
                    continue;
                }
                if (!_grid.IsValidDestination(newNode.Value) || newNode.Value == endPos) {
                    continue;
                }
                if (endNode == null) {
                    endNode = newNode;
                    continue;
                }
                if (newNode.TotalCost < endNode.TotalCost) {
                    endNode = newNode;
                }
            }
            if (endNode != null) {
                PathTrace(endNode);
                return PathfindingResult.Redirected;
            }
            for (int i = 0; i < DirectionsExtensions.DiagonalLength; i++) {
                var pos = endPos + ((DirectionsEight) i).ToP3();
                if (!_keyedDict.TryGetValue(pos, out var node)) {
                    continue;
                }
                if (!_openSet.Contains(node)) {
                    _openSet.Enqueue(node, node.TotalCost);
                }
            }
            if (!_openSet.Contains(originalEnd)) {
                _openSet.Enqueue(originalEnd, originalEnd.TotalCost);
            }
            _loopLimiter.Reset(150);
            while (_loopLimiter.Advance()) {
                if (_openSet.Count == 0) {
                    break;
                }
                var centerNode = _openSet.Dequeue();
                if (endNode == null && _grid.IsValidDestination(centerNode.Value)) {
                    endNode = centerNode;
                }
                if (endNode != null && centerNode.TotalCost < endNode.TotalCost) {
                    endNode = centerNode;
                }
                if (endNode != null && endNode.Value.IsNeighbor(endPos)) {
                    break;
                }
                GetSurrounding(centerNode);
                for (int i = 0; i < _neighbors.Count; i++) {
                    var neighborPos = _neighbors[i];
                    _keyedDict.TryGetValue(neighborPos, out var neighbor);
                    if (neighbor == null && _grid.IsWalkable(neighborPos, _request.IsOversized)) {
                        neighbor = CreateWalkableNode(neighborPos);
                    }
                    if (neighbor == null) {
                        continue;
                    }
                    var newStartCost = centerNode.StartCost + neighbor.Cost;// neighbor.GetTravelCost(centerNode);
                    if (newStartCost < neighbor.StartCost) {
                        neighbor.Parent = centerNode;
                        neighbor.StartCost = newStartCost;
                        if (_openSet.Contains(neighbor)) {
                            _openSet.UpdatePriority(neighbor, neighbor.TotalCost);
                        }
                        else {
                            _openSet.Enqueue(neighbor, neighbor.TotalCost);
                        }
                    }
                }
            }
            if (endNode == null) {
                return PathfindingResult.ErrorPathNotFound;
            }
            PathTrace(endNode);
            return PathfindingResult.Redirected;
        }


        protected P3Node CreateWalkableNode(Point3 pos) {
            var newNode = _nodePool.New();
            newNode.Set(pos, _end, _start,_grid.GetTraversalCost(pos));
            _keyedDict.Add(pos, newNode);
            return newNode;
        }

        public void PathTrace(P3Node endNode) {
            if (_request.Path == null) {
                _request.Path = new List<Point3>(25);
            }
            _request.Path.Clear();
            var currentTrace = endNode;
            while (currentTrace != null) {
                var pos = currentTrace.Value;
                _request.Path.Add(pos);
                if (pos == _start) {
                    break;
                }
                currentTrace = currentTrace.Parent;
            }
            if (_request.Path[_request.Path.Count-1] != _start) {
                _request.Path.Add(_start);
            }
            _request.Path.Reverse();
        }
        
        protected bool CanAdd(Point3 pos) {
            if (_closedSet.Contains(pos) || !_grid.IsWalkable(pos, _request.IsOversized)) {
                return false;
            }
            return true;
        }

        private Point3[] _spiralPoints;
        private Point3[] SpiralPoints {
            get {
                if (_spiralPoints == null) {
                    _spiralPoints = new Point3[50];
                    for (int i = 0; i < _spiralPoints.Length; i++) {
                        _spiralPoints[i] = GridExtension.GridSpiralP3(i);
                    }
                }
                return _spiralPoints;
            }
        }

        private bool FindClosestWalkable(out Point3 pos) {
            for (int i = 0; i < SpiralPoints.Length; i++) {
                pos = _end + SpiralPoints[i];
                if (_grid.IsWalkable(pos, _request.IsOversized)) {
                    return true;
                }
            }
            pos = _end;
            return false;
        }

        private void GetSurrounding(P3Node centerNode) {
            _neighbors.Clear();
            for (int i = 0; i < _positions.Length; i++) {
                var pos = centerNode.Value + _positions[i];
                _validPos[i] = CanAdd(pos);
                if (_validPos[i]) {
                   _neighbors.Add(pos);
                }
            }
            for (int i = 0; i < _diagonalPositions.Length; i++) {
                if (_validPos[_diagonalDirections[i, 0]] && _validPos[_diagonalDirections[i, 1]]) {
                    var pos = centerNode.Value + _diagonalPositions[i];
                    if (CanAdd(pos)) {
                        _neighbors.Add(pos);
                    }
                }
            }
        }

        public class P3Node : FastPriorityQueueNode, IComparable<P3Node>, IComparable {
            //private const float DiagonalCost = 1.41421356237f;
            //public static float OnAxisDiscount = 0.5f;
            //public static float MagnitudeAdjustment = 1.1f;
            public static float SimpleEndCostMulti = 1.5f;

            public float StartCost = float.MaxValue; //G
            public float EndCost = float.MaxValue; // H
            public Point3 Value { get; protected set; }
            public float Cost = 0;
            public P3Node Parent = null;

            public float TotalCost { get { return StartCost + EndCost; } } //F
            public bool Pooled { get; private set; }

            public void Clear() {
                StartCost = float.MaxValue;
                EndCost = float.MaxValue;
                Cost = 0;
                Parent = null;
                Value = default(Point3);
                Value = Point3.zero;
                Pooled = true;
            }

            public void Set(Point3 pos, Point3 end, Point3 start, float cost) {
                Pooled = false;
                Cost = cost;
                Value = pos;
                //EndCost = Abs(Value.x - end.x) + Abs(Value.y - end.y) + Abs(Value.z - end.z); //Manhattan
                EndCost = Math.Max(Abs(Value.x - end.x), Abs(Value[TravelAxis] - end[TravelAxis]));
                EndCost *= SimpleEndCostMulti;
                //var dx = Abs(Value.x - end.x);
                //var dy = Abs(Value[TravelAxis] - end[TravelAxis]);
                //var dx2 = Abs(Value.x - start.x);
                //var dy2 = Abs(Value[TravelAxis] - start[TravelAxis]);
                //EndCost -= Abs(dx * dy2 - dx2 * dy) * EndCostMulti;
                Parent = null;
            }

            //public float GetTravelCost(P3Node neighbor) {
            //    float baseCost = Cost;
            //    if (neighbor.Parent != null) {
            //        var travelDir = Value - neighbor.Value;
            //        var prevDir = neighbor.Value - neighbor.Parent.Value;
            //        if (travelDir == prevDir && travelDir.sqrMagnitude < MagnitudeAdjustment) {
            //            baseCost -= OnAxisDiscount;
            //        }
            //    }
            //    return baseCost;
            //}

            private static int Abs(int x) {
                if (x < 0) {
                    return -x;
                }
                return x;
            }
            
            public override bool Equals(object obj) {
                return obj is P3Node node && node.Value == Value;
            }

            public bool Equals(P3Node node) {
                return node.Value == Value;
            }

            public override int GetHashCode() {
                return Value.GetHashCode();
            }

            public int CompareTo(object obj) {
                return obj is P3Node pn ? CompareTo(pn) : -1;
            }

            public int CompareTo(P3Node other) {
                return TotalCost.CompareTo(other.TotalCost);
            }
        }
   
        //private static Point3[] _surrounding6Way = {
        //    new Point3(1, 0, 0), new Point3(-1, 0, 0),
        //    new Point3(0, 1, 0), new Point3(0, -1, 0),
        //    new Point3(0, 0, 1), new Point3(0, 0, -1),
        //};

        //private static Point3[] _surrounding4Way = {
        //    new Point3(1, 0, 0), new Point3(-1, 0, 0),
        //    new Point3(0, 0, 1), new Point3(0, 0, -1),
        //};

        //private static Point3[] _surrounding16Way = {
        //    //Top slice (Y=1)
        //    new Point3(-1, 1, 1), new Point3(0, 1, 1), new Point3(1, 1, 1), new Point3(-1, 1, 0), new Point3(0, 1, 0),
        //    new Point3(1, 1, 0), new Point3(-1, 1, -1), new Point3(0, 1, -1), new Point3(1, 1, -1),
        //    //Middle slice (Y=0)
        //    new Point3(-1, 0, 1), new Point3(0, 0, 1), new Point3(1, 0, 1), new Point3(-1, 0, 0), new Point3(1, 0, 0),
        //    new Point3(-1, 0, -1), new Point3(0, 0, -1), new Point3(1, 0, -1), //(0,0,0) is self
        //    //Bottom slice (Y=-1)
        //    new Point3(-1, -1, 1), new Point3(0, -1, 1), new Point3(1, -1, 1), new Point3(-1, -1, 0), new Point3(0, -1, 0),
        //    new Point3(1, -1, 0), new Point3(-1, -1, -1), new Point3(0, -1, -1), new Point3(1, -1, -1)
        //};
        //      public bool Linecast (Point3 from, Point3 to, List<PathNode> trace) {
		//	// Find the closest nodes to the start and end on the part of the segment which is on the graph
		//	if (!_checkWalkable(from, from)) {
		//		// Hit point is the point where the segment intersects with the graph boundary
		//		// or just #from if it starts inside the graph
		//		return true;
		//	}
			
  //          // Throw away components we don't care about (y)
		//	// Also subtract 0.5 because nodes have an offset of 0.5 (first node is at (0.5,0.5) not at (0,0))
		//	// And it's just more convenient to remove that term here.
		//	// The variable names #from and #to are unfortunately already taken, so let's use start and end.
		//	//var start = new Vector2(fromInGraphSpace.x - 0.5f, fromInGraphSpace.z - 0.5f);
		//	//var end = new Vector2(toInGraphSpace.x - 0.5f, toInGraphSpace.z - 0.5f);

		//	//// Couldn't find a valid node
		//	//// This shouldn't really happen unless there are NO nodes in the graph
		//	//if (startNode == null || endNode == null) {
		//	//	hit.node = null;
		//	//	hit.point = from;
		//	//	return true;
		//	//}

		//	var dir = to.toVector3() - from.toVector3();

		//	// Primary direction that we will move in
		//	// (e.g up and right or down and left)
		//	var sign = new Vector2(Mathf.Sign(dir.x), Mathf.Sign(dir.y));

		//	// How much further we move away from (or towards) the line when walking along #sign
		//	// This isn't an actual distance. It is a signed distance so it can be negative (other side of the line)
		//	// Also it includes an additional factor, but the same factor is used everywhere
		//	// and we only check for if the signed distance is greater or equal to zero so it is ok
		//	var primaryDirectionError = CrossMagnitude(dir, sign)*0.5f;

		//	/*            Y/Z
		//	 *             |
		//	 *  quadrant   |   quadrant
		//	 *     1              0
		//	 *             2
		//	 *             |
		//	 *   ----  3 - X - 1  ----- X
		//	 *             |
		//	 *             0
		//	 *  quadrant       quadrant
		//	 *     2       |      3
		//	 *             |
		//	 */

		//	// Some XORing to get the quadrant index as shown in the diagram above
		//	int quadrant = (dir.y >= 0 ? 0 : 3) ^ (dir.x >= 0 ? 0 : 1);
		//	// This will be (1,2) for quadrant 0 and (2,3) for quadrant 1 etc.
		//	// & 0x3 is just the same thing as % 4 but it is faster
		//	// This is the direction which moves further to the right of the segment (when looking from the start)
		//	int directionToReduceError = (quadrant + 1) & 0x3;
		//	// This is the direction which moves further to the left of the segment (when looking from the start)
		//	int directionToIncreaseError = (quadrant + 2) & 0x3;

		//	// Current node. Start at n1
		//	var current = startNode;

		//	while (current.NodeInGridIndex != endNode.NodeInGridIndex) {
		//		// We visited #current so add it to the trace
		//		if (trace != null) {
		//			trace.Add(current);
		//		}

		//		// Position of the node in 2D graph/node space
		//		// Here the first node in the graph is at (0,0)
		//		var p = new Vector2(current.XCoordinateInGrid, current.ZCoordinateInGrid);

		//		// Calculate the error
		//		// This is proportional to the distance between the line and the node
		//		var error = CrossMagnitude(dir, p-start);

		//		// How does the error change we take one step in the primary direction
		//		var nerror = error + primaryDirectionError;

		//		// Check if we need to reduce or increase the error (we want to keep it near zero)
		//		// and pick the appropriate direction to move in
		//		int ndir = nerror < 0 ? directionToIncreaseError : directionToReduceError;

		//		// Check we can move in that direction
		//		var other = current.GetNeighbourAlongDirection(ndir);
		//		if (other != null) {
		//			current = other;
		//		} 
		//		else {
		//			// Hit obstacle
		//			// We know from what direction we moved in
		//			// so we can calculate the line which we hit

		//			// Current direction and current direction ±90 degrees
		//			var d1 = new Vector2(neighbourXOffsets[ndir], neighbourZOffsets[ndir]);
		//			var d2 = new Vector2(neighbourXOffsets[(ndir-1+4) & 0x3], neighbourZOffsets[(ndir-1+4) & 0x3]);
		//			Vector2 lineDirection = new Vector2(neighbourXOffsets[(ndir+1) & 0x3], neighbourZOffsets[(ndir+1) & 0x3]);
		//			Vector2 lineOrigin = p + (d1 + d2) * 0.5f;

		//			// Find the intersection
		//			var intersection = VectorMath.LineIntersectionPoint(lineOrigin, lineOrigin+lineDirection, start, end);

		//			var currentNodePositionInGraphSpace = transform.InverseTransform((Vector3)current.position);

		//			// The intersection is in graph space (with an offset of 0.5) so we need to transform it to world space
		//			var intersection3D = new Vector3(intersection.x + 0.5f, currentNodePositionInGraphSpace.y, intersection.y + 0.5f);
		//			var lineOrigin3D = new Vector3(lineOrigin.x + 0.5f, currentNodePositionInGraphSpace.y, lineOrigin.y + 0.5f);

		//			hit.point = transform.Transform(intersection3D);
		//			hit.tangentOrigin = transform.Transform(lineOrigin3D);
		//			hit.tangent = transform.TransformVector(new Vector3(lineDirection.x, 0, lineDirection.y));
		//			hit.node = current;

		//			return true;
		//		}
		//	}

		//	// Add the last node to the trace
		//	if (trace != null) {
		//		trace.Add(current);
		//	}

		//	// No obstacles detected
		//	if (current == endNode) {
		//		hit.point = to;
		//		hit.node = current;
		//		return false;
		//	}

		//	// Reached node right above or right below n2 but we cannot reach it
		//	hit.point = (Vector3)current.position;
		//	hit.tangentOrigin = hit.point;
		//	return true;
		//}
    }
}
