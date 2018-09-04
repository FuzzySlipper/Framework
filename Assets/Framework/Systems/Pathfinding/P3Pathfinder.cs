using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class P3Pathfinder : Pathfinder<P3Pathfinder.P3Node, Point3> {

        private static GenericPool<P3Node> _nodePool = new GenericPool<P3Node>(100, node => node.Clear());

        private Point3[] _surrounding;
        private Point3[] _curroundSurrounding;
        public Func<Point3, Point3, bool> WalkableDel;
        

        protected override P3Node AddNode(Point3 pos) {
            var newNode = _nodePool.New();
            newNode.Set(pos, End);
            AllNodes.Add(newNode);
            return newNode;
        }

        protected override bool CheckWalkable(Point3 pos, Point3 neighborPos) {
            return WalkableDel(pos, neighborPos);
        }

        protected override IList<Point3> GetSurrounding(PathNode<Point3> centerNode) {
            for (int i = 0; i < _curroundSurrounding.Length; i++) {
                _curroundSurrounding[i] = centerNode.Value + _surrounding[i];
            }
            return _curroundSurrounding;
        }

        public void SetConfig(Pathfinding.Config config) {
            switch (config) {
                case Pathfinding.Config.Cardinal:
                    _surrounding = _surrounding4Way;
                    break;
                case Pathfinding.Config.Diagonal:
                    _surrounding = _surrounding16Way;
                    break;
                case Pathfinding.Config.Vertical:
                    _surrounding = _surrounding6Way;
                    break;
            }
            _curroundSurrounding = new Point3[_surrounding.Length];
        }

        public override void Clear() {
            base.Clear();
            for (int i = 0; i < AllNodes.Count; i++) {
                _nodePool.Store(AllNodes[i] as P3Node);
            }
        }

        public override List<Point3> GetPathTrace(P3Node endNode) {
            var finalPath = new List<Point3>();
            P3Node currentTrace = endNode;
            while (currentTrace != null) {
                finalPath.Add(currentTrace.Value);
                if (currentTrace.Value == Start) {
                    break;
                }
                //if (currentTrace.Parent != null && !currentTrace.Parent.Pos.IsNeighbor(currentTrace.Pos)) {
                //    Debug.LogFormat("at pos {0} had parent at {1} that was not a neighbor",
                //        currentTrace.Pos, currentTrace.Parent.Pos);
                //}
                currentTrace = currentTrace.Parent as P3Node;
            }
            finalPath.Reverse();
            if (finalPath[0] != Start) {
                finalPath.Insert(0, Start);
            }
            return finalPath;
        }

        public static bool MapCellWalkable(Point3 currentPos, Point3 nextPos) {
            var cell = World.Get<MapSystem>().GetCell(nextPos);
            var current = World.Get<MapSystem>().GetCell(currentPos);
            if (cell == null || current == null) {
                return false;
            }
            if (!cell.Walkable) {
                return false;
            }
            return current.CanReach(cell);
        }

        public static bool MapCellWalkableUnitFree(Point3 currentPos, Point3 nextPos) {
            var cell = World.Get<MapSystem>().GetCell(nextPos);
            var current = World.Get<MapSystem>().GetCell(currentPos);
            if (cell == null || current == null) {
                return false;
            }
            if (!cell.Walkable || cell.HasActor()) {
                return false;
            }
            return current.CanReach(cell);
        }

        public static bool AnyCell(Point3 currentPos, Point3 nextPos) {
            return true;
        }

        public static bool MapCellExists(Point3 currentPos, Point3 nextPos) {
            return World.Get<MapSystem>().LevelPositionIsFree(nextPos);
        }

        private static Point3[] _surrounding6Way = {
            new Point3(1, 0, 0), new Point3(-1, 0, 0),
            new Point3(0, 1, 0), new Point3(0, -1, 0),
            new Point3(0, 0, 1), new Point3(0, 0, -1),
        };

        private static Point3[] _surrounding4Way = {
            new Point3(1, 0, 0), new Point3(-1, 0, 0),
            new Point3(0, 0, 1), new Point3(0, 0, -1),
        };

        private static Point3[] _surrounding16Way = {
            //Top slice (Y=1)
            new Point3(-1, 1, 1), new Point3(0, 1, 1), new Point3(1, 1, 1), new Point3(-1, 1, 0), new Point3(0, 1, 0),
            new Point3(1, 1, 0), new Point3(-1, 1, -1), new Point3(0, 1, -1), new Point3(1, 1, -1),
            //Middle slice (Y=0)
            new Point3(-1, 0, 1), new Point3(0, 0, 1), new Point3(1, 0, 1), new Point3(-1, 0, 0), new Point3(1, 0, 0),
            new Point3(-1, 0, -1), new Point3(0, 0, -1), new Point3(1, 0, -1), //(0,0,0) is self
            //Bottom slice (Y=-1)
            new Point3(-1, -1, 1), new Point3(0, -1, 1), new Point3(1, -1, 1), new Point3(-1, -1, 0), new Point3(0, -1, 0),
            new Point3(1, -1, 0), new Point3(-1, -1, -1), new Point3(0, -1, -1), new Point3(1, -1, -1)
        };

        public class P3Node : PathNode<Point3>, IComparable<P3Node>, IComparable {

            private const float OffAxisCost = 0.5f;
        
            public override void Clear() {
                base.Clear();
                Value = Point3.zero;
            }

            public override void Set(Point3 pos, Point3 end) {
                Value = pos;
                EndCost = DistanceCost(end);
                Parent = null;
            }

            //public override float GetTravelCost(int travelAxis, float amt, PathNode nodeParent) {
            //    var parent = (P3Node) nodeParent;

            //    return base.GetTravelCost(travelAxis, amt, parent);
            //}

            public override float GetCostToStart() {
                if (Parent == null) {
                    return -1;
                }
                float amt = 0;
                var travelAxis = GetTravelAxis();
                if (travelAxis < 0) {
                    return -1;
                }
                var currentTrace = this;
                while (currentTrace != null) {
                    if (currentTrace.Parent == null) {
                        break;
                    }
                    if (currentTrace.Value[travelAxis] == currentTrace.NodeParent.Value[travelAxis]) {
                        amt += OffAxisCost * 0.25f;
                    }
                    else {
                        break;
                    }
                    currentTrace = currentTrace.NodeParent as P3Node;
                }
                return amt;
            }

            public override int DistanceCost(Point3 other) {
                return (System.Math.Abs(Value.x - other.x) + System.Math.Abs(Value.y - other.y) +
                        System.Math.Abs(Value.z - other.z));
            }

            public override float GetTravelCost(PathNode<Point3> center, Point3 start, Point3 end) {
                var centerNode = center as P3Node;
                if (centerNode != null) {
                    var travelAxis = centerNode.GetTravelAxis();
                    var travelCost = centerNode.GetCostToStart();
                    if (travelAxis >= 0 && travelCost >= 0) {
                        if (Value[travelAxis] == centerNode.Value[travelAxis]) {
                            return 0;
                        }
                        return travelCost;
                        //return GetTravelCost(travelAxis, travelCost, centerNode);
                    }
                }
                var dx1 = Value.x - end.x;
                var dz1 = Value.z - end.z;
                var dx2 = start.x - end.x;
                var dz2 = start.z - end.z;
                var cross = System.Math.Abs(dx1 * dz2 - dx2 * dz1);
                return (cross * (OffAxisCost));
            }

            public int GetTravelAxis() {
                if (Parent == null) {
                    return -1;
                }
                if (Value[0] == NodeParent.Value[0]) {
                    return 0;
                }
                if (Value[2] == NodeParent.Value[2]) {
                    return 2;
                }
                return -1;
            }


            public override bool Equals(object obj) {
                var node = obj as P3Node;
                return node != null && node.Value == Value;
            }

            public bool Equals(P3Node node) {
                return node.Value == Value;
            }

            public override int GetHashCode() {
                return Value.GetHashCode();
            }

            public int CompareTo(object obj) {
                var pn = obj as P3Node;
                return pn != null ? CompareTo(pn) : -1;
            }

            public int CompareTo(P3Node other) {
                return TotalCost.CompareTo(other.TotalCost);
            }
        }
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
