#define AStarPathfinding
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Priority_Queue;

namespace PixelComrades {

    public interface IPathfinder {
        PathfindingResult Run(PathfindingRequest request);
    }

    public abstract class AbstractPathfinder {

        public virtual void Clear(){}

        public abstract class PathNode : FastPriorityQueueNode {
            public float StartCost = float.MaxValue; //G
            public float EndCost = float.MaxValue; // H

            public virtual float TotalCost { get { return StartCost + EndCost; } } //F

            //public virtual float GetTravelCost(int travelAxis, float amt, PathNode parent) {
            //    return amt;
            //}


        }

        public abstract class PathNode<TV> : PathNode {

            public TV Value { get; protected set; }
            public PathNode<TV> Parent = null;

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
                Queue = null;
                StartCost = float.MaxValue;
                EndCost = float.MaxValue;
                Parent = null;
                Value = default(TV);
            }

            public abstract void Set(TV pos, TV end);
            public abstract float EndDistanceCost(TV other);
            public virtual float GetTravelCost(PathNode<TV> neighbor) {
                return 0;
            }
        }
    }

    public abstract class Pathfinder<T, TV> : AbstractPathfinder where T : AbstractPathfinder.PathNode<TV> {
        
        private const int MaxPathCheck = 5500;

        protected FastPriorityQueue<T> OpenSet = new FastPriorityQueue<T>(MaxPathCheck);
        protected Dictionary<TV,T> KeyedDict = new Dictionary<TV, T>(MaxPathCheck);
        protected HashSet<TV> ClosedSet = new HashSet<TV>();
        
        public TV Start;
        public TV End;

        protected WhileLoopLimiter _loopLimiter = new WhileLoopLimiter(MaxPathCheck);

        protected abstract T CreateNode(TV key);
        protected abstract bool CheckWalkable(TV pos, TV neighborPos);
        protected abstract IList<TV> GetSurrounding(PathNode<TV> centerNode);
        protected virtual bool IgnoreEndWalkable { get { return true; } }
        public abstract List<TV> GetPathTrace(T endNode, List<TV> existing);

        public virtual T FindPath() {
            Clear();
            var startNode = Get(Start) ?? CreateNode(Start);
            startNode.StartCost = 0;
            OpenSet.Enqueue(startNode, startNode.TotalCost);
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
                var surrounding = GetSurrounding(centerNode);
                for (int i = 0; i < surrounding.Count; i++) {
                    var neighborPos = surrounding[i];
                    if (neighborPos == null) {
                        continue;
                    }
                    if (ClosedSet.Contains(neighborPos) || !CheckWalkable(centerNode.Value, neighborPos)) {
                        continue;
                    }
                    var neighbor = Get(neighborPos) ?? CreateNode(neighborPos);
                    if (neighbor == null) {
                        if (neighborPos.Equals(End)) {
                            if (IgnoreEndWalkable) {
                                return centerNode;
                            }
                        }
                        continue;
                    }
                    var newStartCost = centerNode.StartCost + neighbor.GetTravelCost(centerNode);
                    if (newStartCost < neighbor.StartCost) {
                        neighbor.Parent = centerNode;
                        neighbor.StartCost = newStartCost;
                        if (OpenSet.Contains(neighbor)) {
                            OpenSet.UpdatePriority(neighbor, neighbor.TotalCost);
                        }
                        else {
                            OpenSet.Enqueue(neighbor, neighbor.TotalCost);
                        }
                    }
                    if (neighborPos.Equals(End)) {
                        neighbor.Parent = centerNode;
                        return neighbor;
                    }
                }
            }
            return null;
        }
        
        public override void Clear() {
            OpenSet.Clear();
            ClosedSet.Clear();
            KeyedDict.Clear();
        }

        protected virtual T GetLowest() {
            return OpenSet.Dequeue();
            //var lowest = OpenSet[0];
            //for (int i = 1; i < OpenSet.Count; i++) {
            //    if (OpenSet[i].TotalCost < lowest.TotalCost) {
            //        lowest = OpenSet[i];
            //        continue;
            //    }
            //    var totalDiff = Math.Abs(OpenSet[i].TotalCost - lowest.TotalCost);
            //    if (totalDiff > Accuracy) {
            //        continue;
            //    }
            //    var endDiff = Math.Abs(OpenSet[i].EndCost - lowest.EndCost);
            //    if (endDiff > Accuracy) {
            //        if (OpenSet[i].EndCost < lowest.EndCost) {
            //            lowest = OpenSet[i];
            //        }
            //        continue;
            //    }
            //    if (OpenSet[i].GetTravelCost(null, Start, End) < lowest.GetTravelCost(null, Start, End)) {
            //        lowest = OpenSet[i];
            //    }
            //}
            //return lowest;
        }

        protected T Get(TV pos) {
            //for (int i = 0; i < OpenSet.Count; i++) {
            //    if (OpenSet[i].Value.Equals(pos)) {
            //        return OpenSet[i];
            //    }
            //}
            //return null;
            return KeyedDict.TryGetValue(pos, out var node) ? node : null;
        }

        protected static float CrossMagnitude(Vector2 a, Vector2 b) {
            return a.x * b.y - b.x * a.y;
        }

        protected static float CrossMagnitude(Point3 a, Point3 b) {
            return a.x * b.y - b.x * a.y;
        }
    }
}