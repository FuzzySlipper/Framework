using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//#define AStarPathfinding

namespace PixelComrades {

    public abstract class AbstractPathfinder {

        public virtual void Clear(){}

        public abstract class PathNode {
            public float StartCost = float.MaxValue; //G
            public float EndCost = float.MaxValue; // H

            public float TotalCost { get { return StartCost + EndCost; } } //F

            public abstract PathNode Parent { get; set; }

            //public virtual float GetTravelCost(int travelAxis, float amt, PathNode parent) {
            //    return amt;
            //}

            public virtual float GetCostToStart() {
                if (Parent == null) {
                    return -1;
                }
                float amt = 0;
                var currentTrace = this;
                while (currentTrace != null) {
                    if (currentTrace.Parent == null) {
                        break;
                    }
                    amt++;
                    currentTrace = currentTrace.Parent;
                }
                return amt;
            }


            public virtual void Clear() {
                StartCost = float.MaxValue;
                EndCost = float.MaxValue;
                Parent = null;
            }

        }

        public abstract class PathNode<TV> : PathNode {

            public TV Value { get; protected set; }
            public PathNode<TV> NodeParent = null;

            public override PathNode Parent { get { return NodeParent; } set { NodeParent = value as PathNode<TV>; } }

            public override void Clear() {
                base.Clear();
                Value = default(TV);
            }

            public abstract void Set(TV pos, TV end);
            public abstract int DistanceCost(TV other);
            public abstract float GetTravelCost(PathNode<TV> center, TV start, TV end);
        }
    }

    public abstract class Pathfinder<T, TV> : AbstractPathfinder where T : AbstractPathfinder.PathNode<TV> {
        
        private const int MaxPathCheck = 5500;
        private const float Accuracy = 0.0001f;
        
        protected List<T> OpenSet = new List<T>();
        protected List<T> AllNodes = new List<T>();
        protected HashSet<TV> ClosedSet = new HashSet<TV>();
        
        public TV Start;
        public TV End;

        private WhileLoopLimiter _loopLimiter = new WhileLoopLimiter(MaxPathCheck);

        protected abstract T AddNode(TV key);
        protected abstract bool CheckWalkable(TV pos, TV neighborPos);
        protected abstract IList<TV> GetSurrounding(PathNode<TV> centerNode);

        public abstract List<TV> GetPathTrace(T endNode);

        public virtual T FindPath() {
            var startNode = AddNode(Start);
            startNode.StartCost = 0;
            OpenSet.Add(startNode);
            _loopLimiter.Reset();
            while (_loopLimiter.Advance()) {
                if (OpenSet.Count == 0) {
                    break;
                }
                var centerNode = GetLowest();
                if (centerNode.Value.Equals(End)) {
                    return centerNode;
                }
                ClosedSet.Add(centerNode.Value);
                OpenSet.Remove(centerNode);
                var surrounding = GetSurrounding(centerNode);
                for (int i = 0; i < surrounding.Count; i++) {
                    var neighborPos = surrounding[i];
                    if (neighborPos == null) {
                        continue;
                    }
                    if (ClosedSet.Contains(neighborPos)) {
                        continue;
                    }
                    var neighbor = Get(neighborPos);
                    if (neighbor == null) {
                        if (CheckWalkable(centerNode.Value, neighborPos)) {
                            neighbor = AddNode(neighborPos);
                            OpenSet.Add(neighbor);
                        }
                        else {
                            ClosedSet.Add(neighborPos);
                        }
                    }
                    if (neighborPos.Equals(End)) {
                        if (neighbor != null) {
                            neighbor.Parent = centerNode;
                            return neighbor;
                        }
                        return centerNode;
                    }
                    if (neighbor == null) {
                        continue;
                    }
                    var newStartCost = centerNode.StartCost + neighbor.DistanceCost(centerNode.Value);
                    newStartCost += neighbor.GetTravelCost(centerNode, Start, End);
                    if (newStartCost < neighbor.StartCost) {
                        neighbor.Parent = centerNode;
                        neighbor.StartCost = newStartCost;
                    }
                }
            }
            return null;
        }
        
        public override void Clear() {
            OpenSet.Clear();
            AllNodes.Clear();
            ClosedSet.Clear();
        }

        protected virtual T GetLowest() {
            var lowest = OpenSet[0];
            for (int i = 1; i < OpenSet.Count; i++) {
                if (OpenSet[i].TotalCost < lowest.TotalCost) {
                    lowest = OpenSet[i];
                    continue;
                }
                var totalDiff = Math.Abs(OpenSet[i].TotalCost - lowest.TotalCost);
                if (totalDiff > Accuracy) {
                    continue;
                }
                var endDiff = Math.Abs(OpenSet[i].EndCost - lowest.EndCost);
                if (endDiff > Accuracy) {
                    if (OpenSet[i].EndCost < lowest.EndCost) {
                        lowest = OpenSet[i];
                    }
                    continue;
                }
                if (OpenSet[i].GetTravelCost(null, Start, End) < lowest.GetTravelCost(null, Start, End)) {
                    lowest = OpenSet[i];
                }
            }
            return lowest;
        }

        private T Get(TV pos) {
            for (int i = 0; i < OpenSet.Count; i++) {
                if (OpenSet[i].Value.Equals(pos)) {
                    return OpenSet[i];
                }
            }
            return null;
        }

        protected static float CrossMagnitude(Vector2 a, Vector2 b) {
            return a.x * b.y - b.x * a.y;
        }

        protected static float CrossMagnitude(Point3 a, Point3 b) {
            return a.x * b.y - b.x * a.y;
        }

        
    }
}