using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SensorToolkit {
    /*
     *  SensorTargets can run in two detection modes
     *  - Colliders: The sensor detects the GameObject attached to any collider it intersects.
     *  - RigidBodies: The sensor detects the GameObject owning the attached RigidBody of any collider it intersects.
     */
    public enum SensorMode {
        Colliders,
        RigidBodies
    }

    public class TagSelectorAttribute : PropertyAttribute {}

    /*
     *  Base class implemented by all sensor types with common functions for querying and filtering
     *  the sensors list of detected objects.
     */
    public abstract class Sensor : MonoBehaviour {
        [TagSelector] [SerializeField] private string[] _allowedTags = null;
        [SerializeField] private bool _enableTagFilter = false;
        [SerializeField] protected GameObject[] IgnoreList;
        
        private List<Collider> _detectedColliders = new List<Collider>();

        public abstract IEnumerable<GameObject> DetectedObjects { get; }
        public abstract IEnumerable<GameObject> DetectedObjectsOrderedByDistance { get; }
        public List<Collider> DetectedColliders { get => _detectedColliders; }

        private GameObject NearestToPoint(IEnumerable<GameObject> gos, Vector3 point) {
            GameObject nearest = null;
            var nearestDistance = 0f;
            var gosEnumerator = gos.GetEnumerator();
            try {
                while (gosEnumerator.MoveNext()) {
                    var go = gosEnumerator.Current;
                    var d = Vector3.SqrMagnitude(go.transform.position - point);
                    if (nearest == null || d < nearestDistance) {
                        nearest = go;
                        nearestDistance = d;
                    }
                }
            }
            finally {
                gosEnumerator.Dispose();
            }
            return nearest;
        }

        private T NearestToPointWithComponent<T>(IEnumerable<GameObject> gos, Vector3 point) where T : Component {
            T nearest = null;
            var nearestDistance = 0f;
            var gosEnumerator = gos.GetEnumerator();
            try {
                while (gosEnumerator.MoveNext()) {
                    var go = gosEnumerator.Current;
                    var c = go.GetComponent<T>();
                    if (c == null) {
                        continue;
                    }
                    var d = Vector3.SqrMagnitude(go.transform.position - point);
                    if (nearest == null || d < nearestDistance) {
                        nearest = c;
                        nearestDistance = d;
                    }
                }
            }
            finally {
                gosEnumerator.Dispose();
            }
            return nearest;
        }

        private Component NearestToPointWithComponent(IEnumerable<GameObject> gos, Vector3 point, Type t) {
            Component nearest = null;
            var nearestDistance = 0f;
            var gosEnumerator = gos.GetEnumerator();
            try {
                while (gosEnumerator.MoveNext()) {
                    var go = gosEnumerator.Current;
                    var c = go.GetComponent(t);
                    if (c == null) {
                        continue;
                    }
                    var d = Vector3.SqrMagnitude(go.transform.position - point);
                    if (nearest == null || d < nearestDistance) {
                        nearest = c;
                        nearestDistance = d;
                    }
                }
            }
            finally {
                gosEnumerator.Dispose();
            }
            return nearest;
        }

        private GameObject NearestToPointWithName(IEnumerable<GameObject> gos, Vector3 point, string name) {
            GameObject nearest = null;
            var nearestDistance = 0f;
            var gosEnumerator = gos.GetEnumerator();
            try {
                while (gosEnumerator.MoveNext()) {
                    var go = gosEnumerator.Current;
                    if (go.name != name) {
                        continue;
                    }
                    var d = Vector3.SqrMagnitude(go.transform.position - point);
                    if (nearest == null || d < nearestDistance) {
                        nearest = go;
                        nearestDistance = d;
                    }
                }
            }
            finally {
                gosEnumerator.Dispose();
            }
            return nearest;
        }

        private T NearestToPointWithNameAndComponent<T>(IEnumerable<GameObject> gos, Vector3 point, string name) where T : Component {
            T nearest = null;
            var nearestDistance = 0f;
            var gosEnumerator = gos.GetEnumerator();
            try {
                while (gosEnumerator.MoveNext()) {
                    var go = gosEnumerator.Current;
                    if (go.name != name) {
                        continue;
                    }
                    var c = go.GetComponent<T>();
                    if (c == null) {
                        continue;
                    }
                    var d = Vector3.SqrMagnitude(go.transform.position - point);
                    if (nearest == null || d < nearestDistance) {
                        nearest = c;
                        nearestDistance = d;
                    }
                }
            }
            finally {
                gosEnumerator.Dispose();
            }
            return nearest;
        }

        private Component NearestToPointWithNameAndComponent(IEnumerable<GameObject> gos, Vector3 point, string name, Type t) {
            Component nearest = null;
            var nearestDistance = 0f;
            var gosEnumerator = gos.GetEnumerator();
            try {
                while (gosEnumerator.MoveNext()) {
                    var go = gosEnumerator.Current;
                    if (go.name != name) {
                        continue;
                    }
                    var c = go.GetComponent(t);
                    if (c == null) {
                        continue;
                    }
                    var d = Vector3.SqrMagnitude(go.transform.position - point);
                    if (nearest == null || d < nearestDistance) {
                        nearest = c;
                        nearestDistance = d;
                    }
                }
            }
            finally {
                gosEnumerator.Dispose();
            }
            return nearest;
        }

        protected bool ShouldIgnore(GameObject go) {
            if (_enableTagFilter) {
                var tagFound = false;
                for (int i = 0; i < _allowedTags.Length; i++) {
                    if (_allowedTags[i] != "" && go != null && go.CompareTag(_allowedTags[i])) {
                        tagFound = true;
                        break;
                    }
                }
                if (!tagFound) {
                    return true;
                }
            }
            for (int i = 0; i < IgnoreList.Length; i++) {
                if (IgnoreList[i] == go) {
                    return true;
                }
            }
            return false;
        }

        // Returns the visibility between 0-1 of the specified object. A 0 means its not visible at all while
        // a 1 means it is entirely visible. Generally only used in the context of line of sight testing.
        public virtual float GetVisibility(GameObject go) {
            return IsDetected(go) ? 1f : 0f;
        }

        // Returns true when the passed GameObject is currently detected by the sensor, false otherwise.
        public virtual bool IsDetected(GameObject go) {
            var detectedEnumerator = DetectedObjects.GetEnumerator();
            try {
                while (detectedEnumerator.MoveNext()) {
                    if (detectedEnumerator.Current == go) {
                        return true;
                    }
                }
            }
            finally {
                detectedEnumerator.Dispose();
            }
            return false;
        }

        // Should cause the sensor to perform it's 'sensing' routine, so that its list of detected objects
        // is up to date at the time of calling. Each sensor can be configured to pulse automatically at
        // fixed intervals or each timestep, however, if you need more control over when this occurs then
        // you can call this method manually.
        public abstract void Pulse();

//#if UNITY_EDITOR
//        protected static readonly Color GizmoColor = new Color(51 / 255f, 255 / 255f, 255 / 255f);
//        public virtual void OnDrawGizmosSelected() {
//            if (TargetLosTargets == null)
//                return;

//            Gizmos.color = GizmoColor;
//            foreach (Transform t in TargetLosTargets) {
//                if (t == null)
//                    return;
//                Gizmos.DrawCube(t.position, Vector3.one * 0.1f);
//            }
//        }
//#endif
    }

    public class DistanceFromPointComparer : IComparer<GameObject> {
        public Vector3 Point;

        public int Compare(GameObject x, GameObject y) {
            var d1 = Vector3.SqrMagnitude(x.transform.position - Point);
            var d2 = Vector3.SqrMagnitude(y.transform.position - Point);
            if (d1 < d2) {
                return -1;
            }
            if (d1 > d2) {
                return 1;
            }
            return 0;
        }
    }
}