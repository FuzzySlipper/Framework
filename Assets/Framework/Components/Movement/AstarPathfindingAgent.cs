#define AStarPathfinding
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if AStarPathfinding
using System.Runtime.Serialization;
using Pathfinding;

namespace PixelComrades {
    [System.Serializable]
	public sealed class AstarPathfindingAgent : IComponent, IDisposable {
        
        public PathfindingStatus CurrentStatus = PathfindingStatus.Created;
        public Point3 LastPosition;
        public float LastPositionTime;
        public int StuckPathCount = 0;
        private CachedUnityComponent<AstarRvoController> _controller = new CachedUnityComponent<AstarRvoController>();
        
        public AstarRvoController Controller { get { return _controller; } }
        public Vector3 Destination { get { return Controller.Destination; } }
        public Point3 DestinationP3 { get { return Controller.Destination.toPoint3(); } }
        public Vector3 SteeringTarget { get { return Controller.SteeringTarget; } }
        public Vector3 DesiredVelocity { get { return Controller.DesiredVelocity; } }
        public float RemainingDistance { get { return Controller.RemainingDistance; } }
        public bool IsPathFinished { get { return Controller.ReachedEndOfPath; } }
        public bool ApproachingDestination { get { return Controller.ApproachingPathEndpoint; } }


        public AstarPathfindingAgent(AstarRvoController controller) {
            _controller  = new CachedUnityComponent<AstarRvoController>(controller);
            controller.OnSearchPathComplete += PathCompleted;
        }

        public AstarPathfindingAgent(SerializationInfo info, StreamingContext context) {
            CurrentStatus = info.GetValue(nameof(CurrentStatus), CurrentStatus);
            _controller = info.GetValue(nameof(_controller), _controller);
            LastPosition = info.GetValue(nameof(LastPosition), LastPosition);
            LastPositionTime = info.GetValue(nameof(LastPositionTime), LastPositionTime);
            StuckPathCount = info.GetValue(nameof(StuckPathCount), StuckPathCount);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(CurrentStatus), CurrentStatus);
            info.AddValue(nameof(_controller), _controller);
            info.AddValue(nameof(LastPosition), LastPosition);
            info.AddValue(nameof(LastPositionTime), LastPositionTime);
            info.AddValue(nameof(StuckPathCount), StuckPathCount);
        }

        public void Dispose() {
            _controller = null;
        }

        private void PathCompleted(Path p) {
            if (p.error) {
                CurrentStatus = PathfindingStatus.InvalidPath;
            }
            else {
                CurrentStatus = PathfindingStatus.PathReceived;
            }

        }
    }
}
#endif

