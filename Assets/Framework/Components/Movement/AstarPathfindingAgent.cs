#define AStarPathfinding
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if AStarPathfinding
using Pathfinding;

namespace PixelComrades {
    public class AstarPathfindingAgent : ComponentBase, IDisposable, IReceive<ChangePositionEvent> {
        
        public PathfindingStatus CurrentStatus = PathfindingStatus.Created;

        public AstarRvoController Controller;
        public Point3 LastPosition;
        public float LastPositionTime;
        public int StuckPathCount = 0;

        public Vector3 Destination { get { return Controller.Destination; } }
        public Point3 DestinationP3 { get { return Controller.Destination.toPoint3(); } }
        public Vector3 SteeringTarget { get { return Controller.SteeringTarget; } }
        public Vector3 DesiredVelocity { get { return Controller.DesiredVelocity; } }
        public float RemainingDistance { get { return Controller.RemainingDistance; } }
        public bool IsPathFinished { get { return Controller.ReachedEndOfPath; } }
        public bool ApproachingDestination { get { return Controller.ApproachingPathEndpoint; } }


        public AstarPathfindingAgent(AstarRvoController controller) {
            Controller = controller;
            Controller.OnSearchPathComplete += PathCompleted;
        }

        public void Dispose() {
            Controller = null;
            Owner = -1;
        }

        private void PathCompleted(Path p) {
            if (p.error) {
                CurrentStatus = PathfindingStatus.InvalidPath;
            }
            else {
                CurrentStatus = PathfindingStatus.PathReceived;
            }

        }

        public void SetPosition(Vector3 pos) {
            Controller.Teleport(pos);
        }

        public void Handle(ChangePositionEvent arg) {
            SetPosition(arg.Position);
        }
    }
}
#endif

