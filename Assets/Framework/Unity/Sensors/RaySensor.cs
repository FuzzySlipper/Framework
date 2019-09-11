using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SensorToolkit {
    /*
     * Detects GameObjects along a ray, it's defined by it's length, which physics layers it detects Objects on and which physics layers obstructs
     * its path. The ray sensor can be queried for the RayCastHit objects associated with each object it detects, so that it's possible to get the
     * point of contact, surface normal etc. As well as this the ray sensor can be queried for the collider that blocked it's path.
     *
     * If the DetectsOnLayers layermask is a subset of the ObstructedByLayers layermask then the ray sensor will use the RayCast method as an
     * optmization. Otherwise it will use the RayCastAll method.
     */
    [ExecuteInEditMode]
    public class RaySensor : Sensor {
        
        protected static readonly Color GizmoColor = new Color(51 / 255f, 255 / 255f, 255 / 255f);
        protected static readonly Color GizmoBlockedColor = Color.red;

        // Event fired each time the sensor is pulsed. This is used by the editor extension and you shouldn't have to subscribe to it yourself.
        public delegate void SensorUpdateHandler();

        private Dictionary<GameObject, RaycastHit> detectedObjectHits;
        private List<GameObject> detectedObjects;
        private RayDistanceComparer distanceComparer;
        private bool isObstructed;
        private HashSet<GameObject> previousDetectedObjects;

        // In Collider mode this sensor will show each GameObject with colliders detected by the sensor. In RigidBody
        // mode it will only show the attached RigidBodies to the colliders that are detected.
        public SensorMode DetectionMode;

        // A layermask for colliders that are detected by the ray sensor.
        public LayerMask DetectsOnLayers;

        // What direction does the ray sensor detect in.
        public Vector3 Direction = Vector3.forward;

        // The length of the ray sensor detection range in world units.
        public float Length = 5f;

        // A layermask for colliders that will block the ray sensors path.
        public LayerMask ObstructedByLayers;

        // Event fired at the time the sensor is unobstructed when before it was obstructed
        [SerializeField] public UnityEvent OnClear;

        // Event fired at the time the sensor is obstructed when before it was unobstructed
        [SerializeField] public UnityEvent OnObstruction;

        // Is the Direction parameter in world space or local space.
        public bool WorldSpace;

        // Returns a list of all detected GameObjects in no particular order.
        public override IEnumerable<GameObject> DetectedObjects {
            get {
                var detectedEnumerator = detectedObjects.GetEnumerator();
                while (detectedEnumerator.MoveNext()) {
                    var go = detectedEnumerator.Current;
                    if (go != null && go.activeInHierarchy) {
                        yield return go;
                    }
                }
            }
        }

        // Returns a list of all detected GameObjects in order of distance from the sensor. This distance is given by the RaycastHit.dist for each GameObject.
        public override IEnumerable<GameObject> DetectedObjectsOrderedByDistance { get { return DetectedObjects; } }

        // Returns a list of all RaycastHit objects, each one is associated with a GameObject in the detected objects list.
        public IList<RaycastHit> DetectedObjectRayHits { get { return new List<RaycastHit>(detectedObjectHits.Values); } }

        // Returns the Collider that obstructed the ray sensors path, or null if it wasn't obstructed.
        public Collider ObstructedBy { get { return ObstructionRayHit.collider; } }

        // Returns the RaycastHit data for the collider that obstructed the rays path.
        public RaycastHit ObstructionRayHit { get; private set; }

        // Returns true if the ray sensor is being obstructed and false otherwise
        public bool IsObstructed { get { return isObstructed && ObstructedBy != null; } }
        private Vector3 direction { get { return WorldSpace ? Direction.normalized : transform.rotation * Direction.normalized; } }

        private class RayDistanceComparer : IComparer<RaycastHit> {
            public int Compare(RaycastHit x, RaycastHit y) {
                if (x.distance < y.distance) {
                    return -1;
                }
                if (x.distance > y.distance) {
                    return 1;
                }
                return 0;
            }
        }

        private void addRayHit(RaycastHit hit) {
            GameObject go;
            if (DetectionMode == SensorMode.RigidBodies) {
                if (hit.rigidbody == null) {
                    return;
                }
                go = hit.rigidbody.gameObject;
            }
            else {
                go = hit.collider.gameObject;
            }
            if (!detectedObjectHits.ContainsKey(go) && !ShouldIgnore(go)) {
                detectedObjectHits.Add(go, hit);
                detectedObjects.Add(go);
                if (!previousDetectedObjects.Contains(go)) {
                    //OnDetected.Invoke(go);
                }
                else {
                    previousDetectedObjects.Remove(go);
                }
            }
        }

        private void clearDetectedObjects() {
            ObstructionRayHit = new RaycastHit();
            detectedObjectHits.Clear();
            detectedObjects.Clear();
        }

        private void detectionEvents() {
            // Any GameObjects still in previousDetectedObjects are no longer detected
            var lostDetectionEnumerator = previousDetectedObjects.GetEnumerator();
            while (lostDetectionEnumerator.MoveNext()) {
                //OnLostDetection.Invoke(lostDetectionEnumerator.Current);
            }
            previousDetectedObjects.Clear();
            for (int i = 0; i < detectedObjects.Count; i++) {
                previousDetectedObjects.Add(detectedObjects[i]);
            }
        }

        private bool layerMaskIsSubsetOf(LayerMask lm, LayerMask subsetOf) {
            return ((lm | subsetOf) & ~subsetOf) == 0;
        }

        private void obstructionEvents() {
            if (isObstructed && ObstructionRayHit.collider == null) {
                isObstructed = false;
                OnClear.Invoke();
            }
            else if (!isObstructed && ObstructionRayHit.collider != null) {
                isObstructed = true;
                OnObstruction.Invoke();
            }
        }

        private void OnEnable() {
            detectedObjects = new List<GameObject>();
            distanceComparer = new RayDistanceComparer();
            detectedObjectHits = new Dictionary<GameObject, RaycastHit>();
            previousDetectedObjects = new HashSet<GameObject>();
            clearDetectedObjects();
        }

        private void reset() {
            clearDetectedObjects();
            isObstructed = false;
        }

        private void testRay() {
            clearDetectedObjects();
            if (layerMaskIsSubsetOf(DetectsOnLayers, ObstructedByLayers) && (IgnoreList == null || IgnoreList.Length == 0)) {
                testRaySingle();
            }
            else {
                testRayMulti();
            }
            obstructionEvents();
            detectionEvents();
            if (OnSensorUpdate != null) {
                OnSensorUpdate();
            }
        }

        private void testRayMulti() {
            Ray ray = new Ray(transform.position, direction);
            LayerMask combinedLayers = DetectsOnLayers | ObstructedByLayers;
            RaycastHit[] hits = Physics.RaycastAll(ray, Length, combinedLayers);
            Array.Sort(hits, distanceComparer);
            for (int i = 0; i < hits.Length; i++) {
                var hit = hits[i];
                if (((1 << hit.collider.gameObject.layer) & DetectsOnLayers) != 0) {
                    addRayHit(hit);
                }
                if (((1 << hit.collider.gameObject.layer) & ObstructedByLayers) != 0) {
                    // Potentially blocks the ray, just make sure it isn't in the ignore list
                    if (ShouldIgnore(hit.collider.gameObject)
                        || hit.rigidbody != null
                        && ShouldIgnore(hit.rigidbody.gameObject)) {
                        // Obstructing collider or its rigid body is in the ignore list
                    }
                    else {
                        ObstructionRayHit = hit;
                        break;
                    }
                }
            }
        }

        private void testRaySingle() {
            Ray ray = new Ray(transform.position, direction);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Length, ObstructedByLayers)) {
                if (((1 << hit.collider.gameObject.layer) & DetectsOnLayers) != 0) {
                    addRayHit(hit);
                }
                ObstructionRayHit = hit;
            }
        }

        // detectedGameObject should be a GameObject that is detected by the sensor. In this case it will return
        // the Raycasthit data associated with this object.
        public RaycastHit GetRayHit(GameObject detectedGameObject) {
            RaycastHit val;
            if (!detectedObjectHits.TryGetValue(detectedGameObject, out val)) {
                Debug.LogWarning("Tried to get the RaycastHit for a GameObject that isn't detected by RaySensor.");
            }
            return val;
        }

        // Returns true if the passed GameObject appears in the sensors list of detected gameobjects
        public override bool IsDetected(GameObject go) {
            return detectedObjectHits.ContainsKey(go);
        }

        public void OnDrawGizmosSelected() {
            if (!isActiveAndEnabled) {
                return;
            }
            if (IsObstructed) {
                Gizmos.color = GizmoBlockedColor;
                Gizmos.DrawLine(transform.position, transform.position + direction * ObstructionRayHit.distance);
            }
            else {
                Gizmos.color = GizmoColor;
                Gizmos.DrawLine(transform.position, transform.position + direction * Length);
            }
            Gizmos.color = GizmoColor;
            foreach (RaycastHit hit in DetectedObjectRayHits) {
                Gizmos.DrawIcon(hit.point, "SensorToolkit/eye.png", true);
            }
        }

        public event SensorUpdateHandler OnSensorUpdate;

        // Pulse the ray sensor
        public override void Pulse() {
            if (isActiveAndEnabled) {
                testRay();
            }
        }
    }
}