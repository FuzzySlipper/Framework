#define AStarPathfinding
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if AStarPathfinding
using Pathfinding.Cooperative;

namespace PixelComrades {
    public class AstarMoverSystem : SystemBase, IMainSystemUpdate {

        private CooperativeContext _cooperativeContext;

        //private const int _maxPathUpdates = 10;
        private const float SlowdownDistance = 1f;
        private const float PathSmoothing = 1;
        private const float RotationSpeed = 120;
        private const float RepathTime = 1f;
        private const float EarlierOffset = 0.1f;
        public const float GoalAccuracy = 1.5f;

        //private int _updateIndex = 0;
        //private UnscaledTimer _repathTimer = new UnscaledTimer(1f);
        
        
        
        private List<PathfindMoverNode> _nodeList;

        public override void Dispose() {
            base.Dispose();
            _nodeList.Clear();
        }

        public void OnSystemUpdate(float dt) {
            if (_cooperativeContext == null) {
                _cooperativeContext = UnityEngine.Object.FindObjectOfType<CooperativeContext>();
            }
            if (_nodeList == null) {
                _nodeList = EntityController.GetNodeList<PathfindMoverNode>();
            }
            if (_nodeList == null || Game.Paused) {
                return;
            }
            for (int i = 0; i < _nodeList.Count; i++) {
                var node = _nodeList[i];
                var pathfinder = node.Pathfinder.c;
                if (pathfinder.Agent == null) {
                    pathfinder.LastRepath = TimeManager.Time + (1 + Random.value) * RepathTime;
                    pathfinder.SetAgent(_cooperativeContext.AddAgent(node.Tr.c.position));
                }
                if (!node.Target.c.IsValid) {
                    if (node.Entity.Tags.Contain(EntityTags.Moving)) {
                        node.Entity.Tags.Remove(EntityTags.Moving);
                    }
                    continue;
                }
                if (pathfinder.Seeker.IsDone()) {
                    // Request a new path if at least [repathInterval] seconds have
                    // passed since the last path request and the seeker is done calculating
                    // the last path request.
                    // OR if we have traversed at least half of the path we are currently following
                    if (TimeManager.Time - pathfinder.LastRepath > RepathTime || (TimeManager.Time - pathfinder.LastRepath > 0.1f && pathfinder.Agent.wantsToRecalculatePath)) {
                        SearchPath(node);
                    }
                }
                var distanceToEnd = RemainingDistance(pathfinder.Agent);
                float slowdown = distanceToEnd < SlowdownDistance ? Mathf.Sqrt(distanceToEnd / SlowdownDistance) : 1;
                // Interpolate our position along the path
                var nextPosition = pathfinder.Agent.SampleSmoothPath(_cooperativeContext.Time - PathSmoothing, _cooperativeContext.Time);
                var earlierPosition = pathfinder.Agent.SampleSmoothPath(_cooperativeContext.Time - PathSmoothing - EarlierOffset, _cooperativeContext.Time - EarlierOffset);
                var dir = nextPosition - earlierPosition;
                var nextRotation = dir != Vector3.zero ? Quaternion.LookRotation(dir, Vector3.up) : node.Tr.c.rotation;
                
                var currentRotationSpeed = (RotationSpeed) * Mathf.Max(0, (slowdown - 0.3f) / 0.7f);
                node.Tr.c.Tr.rotation = Quaternion.RotateTowards(node.Tr.c.rotation, nextRotation, currentRotationSpeed * dt);
                node.Tr.c.Tr.position = nextPosition;
                bool reachedEnd = (_cooperativeContext.Time - PathSmoothing * 0.5f) >= pathfinder.Agent.endOfPathTime || node.Tr.c.position.WorldToGenericGrid(GoalAccuracy) == pathfinder.CurrentTarget;
                // || (pathfinder.Agent.path.reserved && pathfinder.Agent.node.ContainsConnection(pathfinder.Agent.path.endNode))
                // || pathfinder.Agent.AtLastReachableNode(_cooperativeContext.Tick)
                if (reachedEnd) {
                    node.Entity.Tags.Remove(EntityTags.Moving);
                    if (node.Tr.c.position.WorldToGenericGrid(GoalAccuracy) == pathfinder.CurrentTarget) {
                        node.Target.c.Clear();
                        //pathfinder.Agent.enabled = false;
                        pathfinder.Seeker.CancelCurrentPathRequest();
                        Debug.LogFormat(
                            "{0} has reached end at {1} Going to {2} Path Time {3} EndOfPathTime {4}", node.Tr.c.Tr.name, node.Tr.c.position.WorldToGenericGrid(GoalAccuracy), pathfinder.CurrentTarget,
                            (_cooperativeContext.Time - PathSmoothing * 0.5f), pathfinder.Agent.endOfPathTime);
                    }
                }
                else if (!node.Entity.Tags.Contain(EntityTags.Moving)) {
                    node.Entity.Tags.Add(EntityTags.Moving);
                }
            }
        }

        private void SearchPath(PathfindMoverNode node) {
            //node.Pathfinder.c.Agent.enabled = true;
            node.Pathfinder.c.LastRepath = TimeManager.Time;
            node.Pathfinder.c.Seeker.StartPath(node.Pathfinder.c.Agent.GetNewPath(node.Target.c.GetTargetPosition));
        }

        private float RemainingDistance(CooperativeContext.Agent agent) {
            return agent.MeasureDistance(_cooperativeContext.Time - PathSmoothing * 0.5f, agent.endOfPathTime);
        }

        private Vector3 GetVelocity(CooperativeContext.Agent agent) {
            float dt = 0.1f;
            var p1 = agent.SampleSmoothPath(_cooperativeContext.Time - PathSmoothing - dt, _cooperativeContext.Time - dt);
            var p2 = agent.SampleSmoothPath(_cooperativeContext.Time - PathSmoothing, _cooperativeContext.Time);
            return (p2 - p1) / dt;
        }

        //public GraphNode node { get { return agent.GetNode(cooperativeContext.Tick); } }
    }
}
#endif