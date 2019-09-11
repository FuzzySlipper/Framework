using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SensorToolkit {
    /*
     * A sensor that detects colliders that cause the sensors OnTriggerEnter function to be called. This means
     * that the list of detected objects is updated outside of the sensor being pulsed. Pulsing the sensor may
     * still be required though for refreshing the line of sight tests if they are enabled.
     *
     * This sensor guards against cases where a collider causes OnTriggerEnter to be called, but no corresponding
     * OnTriggerExit event occurs. This can happen if a collider is disabled and then re-enabled outside of the
     * sensors range. The sensor expects all detected colliders to regularly create OnTriggerStay events, and if they
     * don't then the collider is timed out and the detection is lost.
     */
    public class TriggerSensor : BaseVolumeSensor {
        
        private List<Collider> _collidersToIncrement;
        private List<Collider> _collidersToRemove;
        private SensorMode _oldDetectionMode;
        private bool _oldRequiresLineOfSight;
        private HashSet<GameObject> _previousDetectedObjects;
        private Dictionary<Collider, int> _triggerStayLag;

        private void OnTriggerEnter(Collider other) {
            AddCollider(other);
        }

        private void OnTriggerExit(Collider other) {
            RemoveCollider(other);
        }

        private void OnTriggerStay(Collider other) {
            if (!_triggerStayLag.ContainsKey(other)) {
                AddCollider(other);
            }
            _triggerStayLag[other] = 0;
        }

        protected override void OnEnable() {
            base.OnEnable();
            _previousDetectedObjects = new HashSet<GameObject>();
            _triggerStayLag = new Dictionary<Collider, int>();
            _oldDetectionMode = DetectionMode;
            _oldRequiresLineOfSight = RequiresLineOfSight;
            _collidersToRemove = new List<Collider>();
            _collidersToIncrement = new List<Collider>();
            if (!CheckForTriggers()) {
                Debug.LogWarning("Trigger Sensor cannot detect anything if there are no triggers on the same GameObject.", gameObject);
            }
            if (DetectionMode == SensorMode.Colliders && GetComponent<Rigidbody>() == null) {
                Debug.LogWarning("In order to detect GameObjects without RigidBodies the TriggerSensor must itself have a RigidBody. Recommend adding a kinematic RigidBody.");
            }
            //StartCoroutine(LineOfSightRoutine());
        }

        public override void Pulse() {
            if (!isActiveAndEnabled) {
                return;
            }
            UpdateSensor();
            RefreshLineOfSight();
            SensorDetectionEvents();
        }

        private void UpdateSensor() {
            // If one of these properties is changed at runtime then the list of DetectedObjects will be changed immediately. This code ensures
            // that the relevant sensor events are fired.
            if (_oldDetectionMode != DetectionMode || _oldRequiresLineOfSight != RequiresLineOfSight) {
                SensorDetectionEvents();
                _oldDetectionMode = DetectionMode;
                _oldRequiresLineOfSight = RequiresLineOfSight;
            }

            // Increment triggerStayLag for each detected collider, if it is increased above a threshold this means its
            // no longer calling OnTriggerStay and should be removed from the list of detected colliders.
            _collidersToRemove.Clear();
            _collidersToIncrement.Clear();
            var colliderStayLagEnumerator = _triggerStayLag.Keys.GetEnumerator();
            try {
                while (colliderStayLagEnumerator.MoveNext()) {
                    var c = colliderStayLagEnumerator.Current;
                    if (c == null) {
                        continue;
                    }
                    _triggerStayLag.TryGetValue(c, out var currentCount);
                    if (currentCount >= 10) {
                        _collidersToRemove.Add(c);
                    }
                    else {
                        _collidersToIncrement.Add(c);
                    }
                }
            }
            finally {
                colliderStayLagEnumerator.Dispose();
            }
            
            for (int i = 0; i < _collidersToRemove.Count; i++) {
                RemoveCollider(_collidersToRemove[i]);
            }
            for (int i = 0; i < _collidersToIncrement.Count; i++) {
                var c = _collidersToIncrement[i];
                _triggerStayLag.TryGetValue(c, out var currentCount);
                _triggerStayLag[c] = currentCount + 1;
            }
        }
        

        private bool CheckForTriggers() {
            var hasRb = GetComponent<Rigidbody>() != null;
            if (hasRb) {
                foreach (Collider c in GetComponentsInChildren<Collider>()) {
                    if (c.enabled && c.isTrigger) {
                        return true;
                    }
                }
            }
            else {
                foreach (Collider c in GetComponents<Collider>()) {
                    if (c.enabled && c.isTrigger) {
                        return true;
                    }
                }
            }
            return false;
        }

        //private IEnumerator LineOfSightRoutine() {
        //    while (true) {
        //        if (!RequiresLineOfSight || _lineOfSightUpdateMode == UpdateMode.Manual) {
        //            yield return null;
        //        }
        //        TestSensor();
        //        if (_checkLineOfSightInterval > 0f) {
        //            yield return new WaitForSeconds(_checkLineOfSightInterval);
        //        }
        //        else {
        //            yield return null;
        //        }
        //    }
        //}

        protected override GameObject AddCollider(Collider other) {
            var newDetected = base.AddCollider(other);
            _triggerStayLag[other] = 0;
            if (newDetected != null) {
                //OnDetected.Invoke(newDetected);
                _previousDetectedObjects.Add(newDetected);
            }
            //if (OnSensorUpdate != null) {
            //    OnSensorUpdate();
            //}
            return newDetected;
        }

        protected override GameObject RemoveCollider(Collider other) {
            _triggerStayLag.Remove(other);
            var detectionLost = base.RemoveCollider(other);
            if (detectionLost != null) {
                //OnLostDetection.Invoke(detectionLost);
                _previousDetectedObjects.Remove(detectionLost);
            }
            //if (OnSensorUpdate != null) {
            //    OnSensorUpdate();
            //}
            return detectionLost;
        }

        private void SensorDetectionEvents() {
            var detectedEnumerator = DetectedObjects.GetEnumerator();
            try {
                while (detectedEnumerator.MoveNext()) {
                    var go = detectedEnumerator.Current;
                    if (_previousDetectedObjects.Contains(go)) {
                        _previousDetectedObjects.Remove(go);
                    }
                    //else {
                    //    // This is a newly detected object
                    //    //OnDetected.Invoke(go);
                    //}
                }
            }
            finally {
                detectedEnumerator.Dispose();
            }
            // Any object still in previousDetectedObjects is no longer detected
            //var previousDetectedEnumerator = _previousDetectedObjects.GetEnumerator();
            //while (previousDetectedEnumerator.MoveNext()) {
            //    //var go = previousDetectedEnumerator.Current;
            //    //OnLostDetection.Invoke(go);
            //}
            _previousDetectedObjects.Clear();
            detectedEnumerator = DetectedObjects.GetEnumerator();
            try {
                while (detectedEnumerator.MoveNext()) {
                    _previousDetectedObjects.Add(detectedEnumerator.Current);
                }
            }
            finally {
                detectedEnumerator.Dispose();
            }
        }

    }
}