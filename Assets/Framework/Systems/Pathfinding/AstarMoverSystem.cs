#define AStarPathfinding
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if AStarPathfinding
using Pathfinding.Cooperative;

namespace PixelComrades {
    public class AstarMoverSystem : SystemBase{

    //    private CooperativeContext _cooperativeContext;
    //    private static GameOptions.CachedInt _maxUpdatesImmobile = new GameOptions.CachedInt("MaxUpdatesMoveImmobile");
    //    //private const int _maxPathUpdates = 10;
    //    private const float SlowdownDistance = 1f;
    //    private const float PathSmoothing = 1;
    //    private const float RotationSpeed = 120;
    //    private const float RepathTime = 1f;
    //    private const float EarlierOffset = 0.1f;
    //    public const float GoalAccuracy = 1.5f;

    //    //private int _updateIndex = 0;
    //    //private UnscaledTimer _repathTimer = new UnscaledTimer(1f);
        
    //    private List<PathfindMoverNode> _nodeList;

    //    public AstarMoverSystem() {
    //        _cooperativeContext = UnityEngine.Object.FindObjectOfType<CooperativeContext>();
    //    }

    //    public override void Dispose() {
    //        base.Dispose();
    //        _nodeList.Clear();
    //    }

    //    public void OnSystemUpdate(float dt) {
    //        if (_nodeList == null) {
    //            _nodeList = EntityController.GetNodeList<PathfindMoverNode>();
    //        }
    //        if (_cooperativeContext == null || _nodeList == null || Game.Paused) {
    //            return;
    //        }
    //        _cooperativeContext.UpdateTime(dt);
    //        for (int i = 0; i < _nodeList.Count; i++) {
    //            var node = _nodeList[i];
    //            var pathfinder = node.Pathfinder.c;
    //            if (pathfinder.Agent == null) {
    //                pathfinder.LastRepath = TimeManager.Time + (1 + Random.value) * RepathTime;
    //                pathfinder.SetAgent(_cooperativeContext.AddAgent(node.Entity.Tr.position));
    //            }
    //            if (!node.Target.c.IsValid) {
    //                if (node.Entity.Tags.Contain(EntityTags.Moving)) {
    //                    node.Entity.Tags.Remove(EntityTags.Moving);
    //                }
    //                continue;
    //            }
    //            var pos = node.Entity.Tr.position.toPoint3();
    //            if (pos == pathfinder.LastPosition) {
    //                pathfinder.UpdatesImmobile++;
    //                if (pathfinder.UpdatesImmobile > _maxUpdatesImmobile) {
    //                    pathfinder.UpdatesImmobile = 0;
    //                    node.Entity.Tags.Remove(EntityTags.Moving);
    //                    node.Target.c.Complete();
    //                    pathfinder.Seeker.CancelCurrentPathRequest();
    //                    continue;
    //                }
    //            }
    //            else {
    //                pathfinder.UpdatesImmobile = 0;
    //                pathfinder.LastPosition = pos;
    //            }
    //            if (pathfinder.Seeker.IsDone()) {
    //                // Request a new path if at least [repathInterval] seconds have
    //                // passed since the last path request and the seeker is done calculating
    //                // the last path request.
    //                // OR if we have traversed at least half of the path we are currently following
    //                if (TimeManager.Time - pathfinder.LastRepath > RepathTime || (TimeManager.Time - pathfinder.LastRepath > 0.1f && pathfinder.Agent.wantsToRecalculatePath)) {
    //                    SearchPath(node);
    //                }
    //            }
    //            var distanceToEnd = RemainingDistance(pathfinder.Agent);
    //            float slowdown = distanceToEnd < SlowdownDistance ? Mathf.Sqrt(distanceToEnd / SlowdownDistance) : 1;
    //            // Interpolate our position along the path
    //            var nextPosition = pathfinder.Agent.SampleSmoothPath(_cooperativeContext.Time - PathSmoothing, _cooperativeContext.Time);
    //            var earlierPosition = pathfinder.Agent.SampleSmoothPath(_cooperativeContext.Time - PathSmoothing - EarlierOffset, _cooperativeContext.Time - EarlierOffset);
    //            var dir = nextPosition - earlierPosition;
    //            var nextRotation = dir != Vector3.zero ? Quaternion.LookRotation(dir, Vector3.up) : node.Entity.Tr.rotation;
                
    //            var currentRotationSpeed = (RotationSpeed) * Mathf.Max(0, (slowdown - 0.3f) / 0.7f);
    //            node.Entity.Tr.rotation = Quaternion.RotateTowards(node.Entity.Tr.rotation, nextRotation, currentRotationSpeed * dt);
    //            node.Entity.Tr.position = Vector3.MoveTowards(node.Entity.Tr.position, nextPosition, 0.25f * dt) ;
    //            bool reachedEnd = (_cooperativeContext.Time - PathSmoothing * 0.5f) >= pathfinder.Agent.endOfPathTime || node.Entity.Tr.position.WorldToGenericGrid(GoalAccuracy) == pathfinder.CurrentTarget;
    //            if (reachedEnd) {
    //                node.Entity.Tags.Remove(EntityTags.Moving);
    //                if (node.Entity.Tr.position.WorldToGenericGridYZero(GoalAccuracy) == pathfinder.CurrentTarget) {
    //                    node.Target.c.Complete();
    //                    //pathfinder.Agent.enabled = false;
    //                    pathfinder.Seeker.CancelCurrentPathRequest();
    //                    //Debug.LogFormat(
    //                    //    "{0} has reached end at {1} Going to {2} Path Time {3} EndOfPathTime {4}", node.Entity.Tr.Tr.name, node.Entity.Tr.position.WorldToGenericGrid(GoalAccuracy), pathfinder.CurrentTarget,
    //                    //    (_cooperativeContext.Time - PathSmoothing * 0.5f), pathfinder.Agent.endOfPathTime);
    //                }
    //            }
    //            else if (!node.Entity.Tags.Contain(EntityTags.Moving)) {
    //                node.Entity.Tags.Add(EntityTags.Moving);
    //            }
    //        }
    //    }

    //    private void SearchPath(PathfindMoverNode node) {
    //        //node.Pathfinder.c.Agent.enabled = true;
    //        node.Pathfinder.c.LastRepath = TimeManager.Time;
    //        node.Pathfinder.c.Seeker.StartPath(node.Pathfinder.c.Agent.GetNewPath(node.Target.c.GetTargetPosition));
    //    }

    //    private float RemainingDistance(CooperativeContext.Agent agent) {
    //        return agent.MeasureDistance(_cooperativeContext.Time - PathSmoothing * 0.5f, agent.endOfPathTime);
    //    }

    //    private Vector3 GetVelocity(CooperativeContext.Agent agent) {
    //        float dt = 0.1f;
    //        var p1 = agent.SampleSmoothPath(_cooperativeContext.Time - PathSmoothing - dt, _cooperativeContext.Time - dt);
    //        var p2 = agent.SampleSmoothPath(_cooperativeContext.Time - PathSmoothing, _cooperativeContext.Time);
    //        return (p2 - p1) / dt;
    //    }

    //    //public GraphNode node { get { return agent.GetNode(cooperativeContext.Tick); } }
    }
}
#endif