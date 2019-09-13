using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class PathfindingDebugging : IComponent, IDisposable {
        public CachedUnityComponent<LineRenderer> LineR;
        public CachedUnityComponent<TextMesh> Tm;
        public CachedTransform DebugObject;

        public PathfindingDebugging(LineRenderer lineR, TextMesh tm, GameObject debugObject = null) {
            LineR = new CachedUnityComponent<LineRenderer>(lineR);
            Tm = new CachedUnityComponent<TextMesh>(tm);
            if (debugObject != null) {
                DebugObject = new CachedTransform(debugObject.transform);                
            }
        }

        public PathfindingDebugging(SerializationInfo info, StreamingContext context) {
            LineR = info.GetValue(nameof(LineR), LineR);
            Tm = info.GetValue(nameof(Tm), Tm);
            DebugObject = info.GetValue(nameof(DebugObject), DebugObject);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(LineR), LineR);
            info.AddValue(nameof(Tm), Tm);
            info.AddValue(nameof(DebugObject), DebugObject);
        }

        private PathfindingStatus _updateStatus = PathfindingStatus.WaitingOnNode;

        public void UpdateStatus(SimplePathfindingAgent agent) {
            if (agent.CurrentStatus == _updateStatus || Tm == null) {
                return;
            }
            _updateStatus = agent.CurrentStatus;
            Tm.Value.text = string.Format("{0}{2}{1}", agent.GetEntity().Id, agent.CurrentStatus, System.Environment.NewLine);
            if (_updateStatus == PathfindingStatus.NoPath || _updateStatus == PathfindingStatus.InvalidPath) {
                LineR.Value.positionCount = 2;
                LineR.Value.SetPosition(0, this.GetEntity().Tr.position);
                LineR.Value.SetPosition(1, agent.End.toVector3());
            }
        }

        public void SetPath(List<Point3> path) {
            LineR.Value.positionCount = path.Count;
            for (int n = 0; n < path.Count; n++) {
                LineR.Value.SetPosition(n, path[n].toVector3());
            }
        }

        public void ClearPath() {
            LineR.Value.positionCount = 0;
        }

        public void Dispose() {
            if (DebugObject != null) {
                ItemPool.Despawn(DebugObject.Tr.gameObject);
                DebugObject = null;
            }
        }
    }
}
