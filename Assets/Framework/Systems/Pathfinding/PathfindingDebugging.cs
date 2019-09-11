using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PathfindingDebugging : ComponentBase, IDisposable {
        public LineRenderer LineR;
        public TextMesh Tm;
        public GameObject DebugObject;

        public PathfindingDebugging(LineRenderer lineR, TextMesh tm, GameObject debugObject = null) {
            LineR = lineR;
            Tm = tm;
            DebugObject = debugObject;
        }

        private PathfindingStatus _updateStatus = PathfindingStatus.WaitingOnNode;

        public void UpdateStatus(SimplePathfindingAgent agent) {
            if (agent.CurrentStatus == _updateStatus || Tm == null) {
                return;
            }
            _updateStatus = agent.CurrentStatus;
            Tm.text = string.Format("{0}{2}{1}", agent.Entity.Id, agent.CurrentStatus, System.Environment.NewLine);
            if (_updateStatus == PathfindingStatus.NoPath || _updateStatus == PathfindingStatus.InvalidPath) {
                LineR.positionCount = 2;
                LineR.SetPosition(0, Entity.Tr.position);
                LineR.SetPosition(1, agent.End.toVector3());
            }
        }

        public void SetPath(List<Point3> path) {
            LineR.positionCount = path.Count;
            for (int n = 0; n < path.Count; n++) {
                LineR.SetPosition(n, path[n].toVector3());
            }
        }

        public void ClearPath() {
            LineR.positionCount = 0;
        }

        public void Dispose() {
            if (DebugObject != null) {
                ItemPool.Despawn(DebugObject);
                DebugObject = null;
            }
        }
    }
}
