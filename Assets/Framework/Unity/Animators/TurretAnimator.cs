using UnityEngine;
using System.Collections;


namespace PixelComrades {
    public class TurretAnimator : ActorAnimator {

        [SerializeField] private Transform _hub = null;
        [SerializeField] private Transform _barrel = null;
        [SerializeField] private Transform _spawnTr = null;
        [SerializeField] private float _percentBarrelSpeed = 0.1f;
        [SerializeField] private float _trackingSpeed = 5f;
        [SerializeField] private float _slowRotateSpeed = 30;

        public Transform Base { get { return _hub; } }
        public Transform Spawn { get { return _spawnTr; } }
        public Transform Barrel { get { return _barrel; } }
        public override Transform AnimTr { get { return Spawn; } }
        

        public void SlowRotate() {
            _hub.Rotate(Vector3.up * _slowRotateSpeed * TimeManager.DeltaTime);
        }

        private void TestSlowRotate() {
            TimeManager.StartUnscaled(SlowRotateTest());
        }

        private IEnumerator SlowRotateTest() {
            var rot = _hub.transform.localRotation;
            var endTime = TimeManager.TimeUnscaled + 2;
            while (TimeManager.TimeUnscaled < endTime) {
                SlowRotate();
                yield return null;
            }
            _hub.transform.localRotation = rot;
        }

        public void RotateToTarget(Vector3 target) {
            Vector3 headingVector = ProjectVectorOnPlane(_hub.up, target - _hub.position);
            Quaternion newHubRotation = Quaternion.LookRotation(headingVector);
            var hubAngle = SignedVectorAngle(_hub.forward, headingVector, Vector3.up);
            // Limit heading angle if required
            if (hubAngle <= -60) {
                newHubRotation = Quaternion.LookRotation(Quaternion.Euler(0, -60, 0)*_hub.forward);
            }
            else if (hubAngle >= 60) {
                newHubRotation = Quaternion.LookRotation(Quaternion.Euler(0, 60, 0)*_hub.forward);
            }
            _hub.rotation = Quaternion.RotateTowards(_hub.rotation, newHubRotation, _trackingSpeed * TimeManager.DeltaTime);
            Vector3 elevationVector = ProjectVectorOnPlane(_hub.right, target - _barrel.position);
            Quaternion newBarrelRotation = Quaternion.LookRotation(elevationVector);
            _barrel.rotation = Quaternion.RotateTowards(_barrel.rotation, newBarrelRotation, 
                _trackingSpeed * TimeManager.DeltaTime * _percentBarrelSpeed);
        }

        Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 vector) {
            return vector - (Vector3.Dot(vector, planeNormal) * planeNormal);
        }

        float SignedVectorAngle(Vector3 referenceVector, Vector3 otherVector, Vector3 normal) {
            var perpVector = Vector3.Cross(normal, referenceVector);
            var angle = Vector3.Angle(referenceVector, otherVector);
            angle *= Mathf.Sign(Vector3.Dot(perpVector, otherVector));
            return angle;
        }

    }
}