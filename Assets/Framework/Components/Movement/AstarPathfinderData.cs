//#define AStarPathfinding
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if AStarPathfinding
using Pathfinding.Cooperative;
using Pathfinding;

namespace PixelComrades {
    public class AstarPathfinderData : IComponent, IDisposable, IReceive<SetMoveTarget> {
        public int Owner { get;set; }
        public Seeker Seeker { get; }
        public CooperativeContext.Agent Agent { get; private set; }
        public Point3 CurrentTarget = Point3.max;
        public float LastRepath = float.NegativeInfinity;
        public Point3 LastPosition;
        public int UpdatesImmobile;

        public AstarPathfinderData(Seeker seeker) {
            Seeker = seeker;
            Seeker.startEndModifier.exactStartPoint = StartEndModifier.Exactness.Original;
        }

        public void SetAgent(CooperativeContext.Agent agent) {
            Agent = agent;
        }

        public void Dispose() {
            LastRepath = float.NegativeInfinity;
            Seeker.CancelCurrentPathRequest();
            Agent.enabled = false;
            Agent = null;
        }

        public void Handle(SetMoveTarget arg) {
            //CurrentTarget = arg.Target.WorldToGenericGridYZero(AstarMoverSystem.GoalAccuracy);
            
        }
    }
}
#endif

