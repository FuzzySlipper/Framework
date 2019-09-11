using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SensorToolkit {
    /*
     * Common functionality for sensors that detect colliders within a volume. SensorTargets that implement this class should
     * detect colliders and pass those colliders to this base implementation through the AddCollider protected method. When
     * colliders are no longer detected then a corresponding call to removeCollider should be made. This class manages
     * the list of detected objects by processing the list of detected colliders.
     *
     * Includes optional support for line of sight testing, when enabled a GameObject will only appear as detected if
     * it passes a line of sight test. To calculate line of sight the sensor casts a number of rays towards the target
     * object. The ratio of these test rays that reach the object unobstructed is it's computed visibility. If the visibility
     * ratio is greater then a user specified amount then the object appears in the detected list. These target points can
     * be specified by adding a LOSTargets component to the object, or else they are randomly generated. See the documentation 
     * at www.micosmo.com/sensortoolkit/#rangesensor for more info.
     * 
     * If line of sight testing is enabled then it will need to be continually refreshed by calls to the refreshLineOfSight()
     * protected method. Ideally this should happen during a pulse.
     */
    [ExecuteInEditMode]
    public abstract class BaseVolumeSensor : Sensor {
        protected static ListCache<Collider> ColliderListCache = new ListCache<Collider>();
        protected static ListCache<Vector3> Vector3ListCache = new ListCache<Vector3>();
        private static RayCastTargetsCache _rayCastTargetsCache = new RayCastTargetsCache();
        
        // In Collider mode this sensor will show each GameObject with colliders detected by the sensor. In RigidBody
        // mode it will only show the attached RigidBodies to the colliders that are detected.
        [SerializeField] protected SensorMode DetectionMode;
        // GameObjects are only detected if they pass a line of sight test. The refreshLineOfSight method must be called
        // regularly to update the set of objects in line of sight.
        [SerializeField] protected bool RequiresLineOfSight;
        // Minimum visibility an object must be for it to be detected.
        [SerializeField, Range(0f, 1f)] private float _minimumVisibility = 0.5f;
        [SerializeField] private LayerMask _blocksLineOfSight = 0;
        // Number of test points the Sensor will generate on objects that don't have a LOSTargets component.
        [SerializeField, Range(1, 20)] private int _numberOfRays = 1;

        private DistanceFromPointComparer _distanceComparer = new DistanceFromPointComparer();
        // Maps a RigidBody GameObject to a list of it's colliders that have been detected. These colliders
        // may be attached to children GameObjects.
        private Dictionary<GameObject, List<Collider>> _gameObjectColliders = new Dictionary<GameObject, List<Collider>>();
        private List<GameObject> _tempGoList = new List<GameObject>();
        private Dictionary<GameObject, float> _objectVisibility = new Dictionary<GameObject, float>();
        private List<RayCastResult> _raycastResults = new List<RayCastResult>();
        // Maps a GameObject to a list of raycast target positions for calculating line of sight
        private Dictionary<GameObject, RayCastTargets> _rayCastTargets = new Dictionary<GameObject, RayCastTargets>();
        // Maps a GameObject to a list of it's colliders that have been detected.
        private Dictionary<GameObject, List<Collider>> _rigidBodyColliders = new Dictionary<GameObject, List<Collider>>();

        public override IEnumerable<GameObject> DetectedObjects {
            get {
                if (RequiresLineOfSight) {
                    var objectVisibilityEnumerator = _objectVisibility.Keys.GetEnumerator();
                    try {
                        while (objectVisibilityEnumerator.MoveNext()) {
                            var go = objectVisibilityEnumerator.Current;
                            if (go != null && go.activeInHierarchy && !ShouldIgnore(go) && _objectVisibility[go] >= _minimumVisibility) {
                                yield return go;
                            }
                        }
                    }
                    finally {
                        objectVisibilityEnumerator.Dispose();
                    }
                }
                else {
                    var colliderEnumerator = DetectionMode == SensorMode.RigidBodies? _rigidBodyColliders.Keys.GetEnumerator() : _gameObjectColliders.Keys.GetEnumerator();
                    try {
                        while (colliderEnumerator.MoveNext()) {
                            var go = colliderEnumerator.Current;
                            if (go != null && go.activeInHierarchy && !ShouldIgnore(go)) {
                                yield return go;
                            }
                        }
                    }
                    finally {
                        colliderEnumerator.Dispose();
                    }
                }
            }
        }

        // Returns a list of all GameObjects detected by sensor and ordered by their distance from the sensor.
        // Bit of a hack here, don't get nested enumerators from this property.
        public override IEnumerable<GameObject> DetectedObjectsOrderedByDistance {
            get {
                _tempGoList.Clear();
                _tempGoList.AddRange(DetectedObjects);
                _distanceComparer.Point = transform.position;
                _tempGoList.Sort(_distanceComparer);
                return _tempGoList;
            }
        }

        // Returns a map of every sensed object and their computed visibility. This includes objects whose
        // visibility is too low to be detected.
        public Dictionary<GameObject, float> ObjectVisibilities { get { return _objectVisibility; } }
        
        protected virtual void OnEnable() {
            _rigidBodyColliders.Clear();
            _gameObjectColliders.Clear();
            _rayCastTargets.Clear();
            _objectVisibility.Clear();
            _raycastResults.Clear();
            _tempGoList.Clear();
            DetectedColliders.Clear();
        }

        private bool AddColliderToMap(Collider c, GameObject go, IDictionary<GameObject, List<Collider>> dict) {
            var newDetection = false;
            if (!dict.TryGetValue(go, out var colliderList)) {
                newDetection = true;
                colliderList = ColliderListCache.Get();
                dict[go] = colliderList;
            }
            colliderList.Add(c);
            return newDetection;
        }

        private void DisposeRayCastTarget(GameObject forGameObject) {
            if (_rayCastTargets.ContainsKey(forGameObject)) {
                _rayCastTargetsCache.Dispose(_rayCastTargets[forGameObject]);
                _rayCastTargets.Remove(forGameObject);
            }
        }

        private List<Vector3> GenerateRayCastTargets(GameObject go) {
            IList<Collider> cs;
            if (DetectionMode == SensorMode.Colliders) {
                cs = _gameObjectColliders[go];
            }
            else {
                cs = _rigidBodyColliders[go];
            }
            List<Vector3> rts = Vector3ListCache.Get();
            if (_numberOfRays == 1) {
                rts.Add(GetCentreOfColliders(go, cs));
            }
            else {
                for (int i = 0; i < _numberOfRays; i++) {
                    rts.Add(GetRandomPointInColliders(go, cs));
                }
            }
            return rts;
        }

        private Vector3 GetCentreOfColliders(GameObject goRoot, IList<Collider> goColliders) {
            Vector3 aggregate = Vector3.zero;
            for (int i = 0; i < goColliders.Count; i++) {
                var c = goColliders[i];
                aggregate += c.bounds.center - goRoot.transform.position;
            }
            return aggregate / goColliders.Count;
        }

        private Vector3 GetRandomPointInColliders(GameObject goRoot, IList<Collider> colliders) {
            // Choose a random collider weighted by its volume
            Collider rc = colliders[0];
            var totalVolume = 0f;
            for (int i = 0; i < colliders.Count; i++) {
                var c = colliders[i];
                totalVolume += c.bounds.size.x * c.bounds.size.y + c.bounds.size.z;
            }
            var r = Random.Range(0f, 1f);
            for (int i = 0; i < colliders.Count; i++) {
                var c = colliders[i];
                rc = c;
                var v = c.bounds.size.x * c.bounds.size.y * c.bounds.size.z;
                r -= v / totalVolume;
                if (r <= 0f) {
                    break;
                }
            }

            // Now choose a random point within that random collider and return it
            var rp = new Vector3(Random.Range(-.5f, .5f), Random.Range(-.5f, .5f), Random.Range(-.5f, .5f));
            rp.Scale(rc.bounds.size);
            rp += rc.bounds.center - goRoot.transform.position;
            return rp;
        }

        private RayCastTargets GetRayCastTargets(GameObject go) {
            if (_rayCastTargets.TryGetValue(go, out var rts)) {
                return rts;
            }
            rts = _rayCastTargetsCache.Get();
            //if (TargetLosTargets != null) {
            //    rts.Set(go, TargetLosTargets);
            //    rayCastTargets.Add(go, rts);
            //    return rts;
            //}
            rts.Set(go, GenerateRayCastTargets(go));
            _rayCastTargets.Add(go, rts);
            return rts;
        }

        private bool IsInLineOfSight(GameObject go, Vector3 testPoint, out Vector3 obstructionPoint) {
            obstructionPoint = Vector3.zero;
            var toGoCentre = testPoint - transform.position;
            var ray = new Ray(transform.position, toGoCentre.normalized);
            RaycastHit hitInfo;
            if (!Physics.Raycast(ray, out hitInfo, toGoCentre.magnitude, _blocksLineOfSight)) {
                return true;
            }
            // Ray hit something, check that it was the target.
            if (DetectionMode == SensorMode.RigidBodies && hitInfo.rigidbody != null && hitInfo.rigidbody.gameObject == go) {
                return true;
            }
            if (DetectionMode == SensorMode.Colliders && hitInfo.collider.gameObject == go) {
                return true;
            }
            obstructionPoint = hitInfo.point;
            return false;
            // Ray didn't hit anything so assume target is in line of sight
        }

        private bool RemoveColliderFromMap(Collider c, GameObject go, IDictionary<GameObject, List<Collider>> dict) {
            var detectionLost = false;
            if (dict.TryGetValue(go, out var colliderList)) {
                colliderList.Remove(c);
                if (colliderList.Count == 0) {
                    detectionLost = true;
                    dict.Remove(go);
                    ColliderListCache.Dispose(colliderList);
                    _objectVisibility.Remove(go);
                }
            }
            return detectionLost;
        }

        private float TestObjectVisibility(GameObject go) {
            int nSuccess = 0;
            var rayCastTargets = GetRayCastTargets(go);
            IList<Vector3> testPoints = rayCastTargets.GetTargetPoints();
            for (int i = 0; i < testPoints.Count; i++) {
                var testPoint = testPoints[i];
                var result = new RayCastResult();
                result.Go = go;
                result.TestPoint = testPoint;
                result.IsObstructed = false;
                if (IsInLineOfSight(go, testPoint, out var obstructionPoint)) {
                    nSuccess++;
                    rayCastTargets.SetIsTargetVisible(i, true);
                }
                else {
                    result.IsObstructed = true;
                    result.ObstructionPoint = obstructionPoint;
                    rayCastTargets.SetIsTargetVisible(i, false);
                }
                _raycastResults.Add(result);
            }
            return nSuccess / (float) testPoints.Count;
        }

        protected virtual GameObject AddCollider(Collider c) {
            GameObject newColliderDetection = null;
            GameObject newRigidBodyDetection = null;
            if (AddColliderToMap(c, c.gameObject, _gameObjectColliders)) {
                DisposeRayCastTarget(c.gameObject);
                newColliderDetection = c.gameObject;
            }
            if (c.attachedRigidbody != null && AddColliderToMap(c, c.attachedRigidbody.gameObject, _rigidBodyColliders)) {
                DisposeRayCastTarget(c.attachedRigidbody.gameObject);
                newRigidBodyDetection = c.attachedRigidbody.gameObject;
            }
            var newDetection = DetectionMode == SensorMode.Colliders ? newColliderDetection : newRigidBodyDetection;
            if (ShouldIgnore(newDetection)) {
                return null;
            }
            if (RequiresLineOfSight && newDetection != null) {
                bool prevDetected = _objectVisibility.ContainsKey(newDetection) && _objectVisibility[newDetection] >= _minimumVisibility;
                _objectVisibility[newDetection] = TestObjectVisibility(newDetection);
                if (!prevDetected && _objectVisibility[newDetection] >= _minimumVisibility) {
                    if (!DetectedColliders.Contains(c)) {
                        DetectedColliders.Add(c);
                    }
                    return newDetection;
                }
                return null;
            }
            if (!DetectedColliders.Contains(c)) {
                DetectedColliders.Add(c);
            }
            return newDetection;
        }

        protected void ClearColliders() {
            var collidersEnumerator = _gameObjectColliders.GetEnumerator();
            try {
                while (collidersEnumerator.MoveNext()) {
                    var colliderList = collidersEnumerator.Current.Value;
                    ColliderListCache.Dispose(colliderList);
                }
            }
            finally {
                collidersEnumerator.Dispose();
            }
            _gameObjectColliders.Clear();
            collidersEnumerator = _rigidBodyColliders.GetEnumerator();
            try {
                while (collidersEnumerator.MoveNext()) {
                    var colliderList = collidersEnumerator.Current.Value;
                    ColliderListCache.Dispose(colliderList);
                }
            }
            finally {
                collidersEnumerator.Dispose();
            }
            _rigidBodyColliders.Clear();
            DetectedColliders.Clear();
            ClearLineOfSight();
        }

        protected void ClearDestroyedGameObjects() {
            _tempGoList.Clear();
            var collidersGameObjectsEnumerator = _gameObjectColliders.Keys.GetEnumerator();
            try {
                while (collidersGameObjectsEnumerator.MoveNext()) {
                    var go = collidersGameObjectsEnumerator.Current;
                    if (go == null) {
                        _tempGoList.Add(go);
                    }
                }
            }
            finally {
                collidersGameObjectsEnumerator.Dispose();
            }
            for (int i = 0; i < _tempGoList.Count; i++) {
                _gameObjectColliders.Remove(_tempGoList[i]);
            }
            _tempGoList.Clear();
            var rigidBodyGameObjectsEnumerator = _rigidBodyColliders.Keys.GetEnumerator();
            try {
                while (rigidBodyGameObjectsEnumerator.MoveNext()) {
                    var go = rigidBodyGameObjectsEnumerator.Current;
                    if (go == null) {
                        _tempGoList.Add(go);
                    }
                }
            }
            finally {
                rigidBodyGameObjectsEnumerator.Dispose();
            }
            for (int i = 0; i < _tempGoList.Count; i++) {
                _rigidBodyColliders.Remove(_tempGoList[i]);
            }
        }

        protected void ClearLineOfSight() {
            _objectVisibility.Clear();
            _raycastResults.Clear();
        }


        protected void RefreshLineOfSight() {
            _objectVisibility.Clear();
            _raycastResults.Clear();
            var gosEnumerator = DetectionMode == SensorMode.RigidBodies? _rigidBodyColliders.Keys.GetEnumerator() : _gameObjectColliders.Keys.GetEnumerator();
            try {
                while (gosEnumerator.MoveNext()) {
                    var go = gosEnumerator.Current;
                    if (go == null) {
                        continue;
                    }
                    _objectVisibility[go] = TestObjectVisibility(go);
                }
            }
            finally {
                gosEnumerator.Dispose();
            }
        }

        protected virtual GameObject RemoveCollider(Collider c) {
            if (c == null) {
                ClearDestroyedGameObjects();
                return null;
            }
            GameObject colliderDetectionLost = null;
            GameObject rigidBodyDetectionLost = null;
            if (RemoveColliderFromMap(c, c.gameObject, _gameObjectColliders)) {
                DisposeRayCastTarget(c.gameObject);
                colliderDetectionLost = c.gameObject;
            }
            if (c.attachedRigidbody != null && RemoveColliderFromMap(c, c.attachedRigidbody.gameObject, _rigidBodyColliders)) {
                DisposeRayCastTarget(c.attachedRigidbody.gameObject);
                rigidBodyDetectionLost = c.attachedRigidbody.gameObject;
            }
            var detectionLost = DetectionMode == SensorMode.Colliders ? colliderDetectionLost : rigidBodyDetectionLost;
            if (ShouldIgnore(detectionLost)) {
                return null;
            }
            DetectedColliders.Remove(c);
            if (RequiresLineOfSight && detectionLost != null) {
                if (_objectVisibility.ContainsKey(detectionLost)) {
                    _objectVisibility.Remove(detectionLost);
                    return detectionLost;
                }
                return null;
            }
            if (detectionLost != null) {
                _objectVisibility.Remove(detectionLost);
            }
            return detectionLost;
        }

        // Returns the visibility between 0-1 of the specified object. A 0 means its not visible at all while
        // a 1 means it is entirely visible. Visibility only makes sense in the context of line of sight tests,
        // it is the ratio of rays towards the target object that are clear of obstructions.
        public override float GetVisibility(GameObject go) {
            if (!RequiresLineOfSight) {
                return base.GetVisibility(go);
            }
            if (_objectVisibility.ContainsKey(go)) {
                return _objectVisibility[go];
            }
            return 0f;
        }

        // Returns a list of positions on a given object that passed line of sight tests.
        public List<Vector3> GetVisiblePositions(GameObject go) {
            if (go != null && _rayCastTargets.TryGetValue(go, out var targets)) {
                return targets.GetVisibleTargetPositions();
            }
            return null;
        }

        // Returns a list of transforms on the given object that passed line of sight tests. Will only return
        // results for objects that have a LOSTargets component.
        public List<Transform> GetVisibleTransforms(GameObject go) {
            if (go != null && _rayCastTargets.TryGetValue(go, out var targets)) {
                return targets.GetVisibleTransforms();
            }
            return null;
        }
        
        private struct RayCastResult {
            public GameObject Go;
            public Vector3 TestPoint;
            public Vector3 ObstructionPoint;
            public bool IsObstructed;
        }

        private class RayCastTargets {
            private GameObject _go;
            private List<bool> _isTargetVisible;
            private List<Vector3> _returnPoints;
            private List<Vector3> _targetPoints;
            private IList<Transform> _targetTransforms;

            public RayCastTargets() {
                _returnPoints = new List<Vector3>();
                _isTargetVisible = new List<bool>();
            }

            public void Dispose() {
                if (_targetPoints != null) {
                    Vector3ListCache.Dispose(_targetPoints);
                }
            }

            public IList<Vector3> GetTargetPoints() {
                _returnPoints.Clear();
                if (_targetTransforms != null) {
                    for (int i = 0; i < _targetTransforms.Count; i++) {
                        _returnPoints.Add(_targetTransforms[i].position);
                    }
                }
                else {
                    var go = this._go;
                    for (int i = 0; i < _targetPoints.Count; i++) {
                        _returnPoints.Add(go.transform.TransformPoint(_targetPoints[i]));
                    }
                }
                return _returnPoints;
            }

            public List<Vector3> GetVisibleTargetPositions() {
                var visibleList = new List<Vector3>();
                if (_targetTransforms != null) {
                    for (int i = 0; i < _isTargetVisible.Count; i++) {
                        if (_isTargetVisible[i]) {
                            visibleList.Add(_targetTransforms[i].position);
                        }
                    }
                }
                else {
                    for (int i = 0; i < _isTargetVisible.Count; i++) {
                        if (_isTargetVisible[i]) {
                            visibleList.Add(_go.transform.TransformPoint(_targetPoints[i]));
                        }
                    }
                }
                return visibleList;
            }

            public List<Transform> GetVisibleTransforms() {
                var visibleList = new List<Transform>();
                for (int i = 0; i < _isTargetVisible.Count; i++) {
                    if (_isTargetVisible[i]) {
                        visibleList.Add(_targetTransforms[i]);
                    }
                }
                return visibleList;
            }

            public bool IsTransforms() {
                return _targetTransforms != null;
            }

            public void Set(GameObject go, IList<Transform> targets) {
                this._go = go;
                _targetTransforms = targets;
                _targetPoints = null;
                _isTargetVisible.Clear();
                for (int i = 0; i < targets.Count; i++) {
                    _isTargetVisible.Add(false);
                }
            }

            public void Set(GameObject go, List<Vector3> targets) {
                this._go = go;
                _targetTransforms = null;
                _targetPoints = targets;
                _isTargetVisible.Clear();
                for (int i = 0; i < targets.Count; i++) {
                    _isTargetVisible.Add(false);
                }
            }

            public void SetIsTargetVisible(int i, bool isVisible) {
                _isTargetVisible[i] = isVisible;
            }
        }

        private class RayCastTargetsCache : ObjectCache<RayCastTargets> {
            public override void Dispose(RayCastTargets obj) {
                obj.Dispose();
                base.Dispose(obj);
            }
        }
#if UNITY_EDITOR
        protected static readonly Color GizmoColor = new Color(51 / 255f, 255 / 255f, 255 / 255f);
        //protected static readonly Color GizmoBlockedColor = Color.red;
        // Displays the results of line of sight tests during OnDrawGizmosSelected for objects in this set.
        //public HashSet<GameObject> ShowRayCastDebug;

        public virtual void OnDrawGizmosSelected() {
            if (!isActiveAndEnabled) {
                return;
            }
            Gizmos.color = GizmoColor;
            foreach (GameObject go in DetectedObjects) {
                Vector3 goCentre = GetCentreOfColliders(go, DetectionMode == SensorMode.RigidBodies && _rigidBodyColliders.ContainsKey(go) ? _rigidBodyColliders[go]: _gameObjectColliders[go]) + go.transform.position;
                Gizmos.DrawIcon(goCentre, "SensorToolkit/eye.png", true);
            }
            //if (RequiresLineOfSight && ShowRayCastDebug != null) {
            //    foreach (RayCastResult result in _raycastResults) {
            //        if (!ShowRayCastDebug.Contains(result.Go)) {
            //            continue;
            //        }
            //        Gizmos.color = GizmoColor;
            //        if (result.IsObstructed) {
            //            Gizmos.DrawLine(transform.position, result.ObstructionPoint);
            //            Gizmos.color = GizmoBlockedColor;
            //            Gizmos.DrawLine(result.ObstructionPoint, result.TestPoint);
            //            Gizmos.DrawCube(result.TestPoint, Vector3.one * 0.1f);
            //        }
            //        else {
            //            Gizmos.DrawLine(transform.position, result.TestPoint);
            //            Gizmos.DrawCube(result.TestPoint, Vector3.one * 0.1f);
            //        }
            //    }
            //}
        }
#endif
    }
}