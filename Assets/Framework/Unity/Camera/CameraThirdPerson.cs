using UnityEngine;
using System.Collections;
namespace PixelComrades {
    public class CameraThirdPerson : MonoBehaviour {
        public enum UpdateType {
            FixedUpdate, // Update in FixedUpdate (for tracking rigidbodies).
            LateUpdate, // Update in LateUpdate. (for tracking objects that are moved in Update)
            ManualUpdate, // user must call to update camera
        }

        public Vector3 TargetOffset = new Vector3(0, 1, 0);
        public UpdateType WhenUpdate; // stores the selected update type
        public float MoveSpeed = 1f;                      // How fast the rig will move to keep up with the unit's position.
        [Range(0f, 10f)]
        public float TurnSpeed = 1.5f;   // How fast the rig will rotate from user input.
        public float TurnSmoothing = 0.1f;                // How much smoothing to apply to the turn input, to reduce mouse-turn jerkiness
        public float TiltMax = 75f;                       // The maximum value of the _camAngle.x axis rotation of the pivot.
        public float TiltMin = 45f;                       // The minimum value of the _camAngle.x axis rotation of the pivot.
        public float CamDistance = 2.35f;
        public float CamMaxDistance = 5;
        public float CamMinDistance = 2;
        public float TargetLook = 0.5f;
        [Range(0f, 1f)]
        public float MaxLookPercent = 0.5f;
        public float ZoomSmoothing = 0.1f;
        public bool ControlRotation = false;
        public Transform Cursor;

        private float _lookAngle;                    // The rig's y axis rotation.
        private float _tiltAngle;                    // The pivot's _camAngle.x axis rotation.
        private Vector3 _pivotEulers;
        private Quaternion _pivotTargetRot;
        private Quaternion _transformTargetRot;
        private Transform _cam;
        private Transform _pivot;


        void Awake() {
            _cam = GetComponentInChildren<Camera>().transform;
            _pivot = _cam.parent;
            _pivotEulers = _pivot.rotation.eulerAngles;
            _pivotTargetRot = _pivot.transform.localRotation;
            _transformTargetRot = transform.localRotation;
        }

        void Update() {
            if (ControlRotation) {
                HandleRotationMovement();
            }
            var scroll = PlayerInput.main.GetAxis("Scroll");
            if (Mathf.Abs(scroll) < 0.05f) {
                return;
            }
            CamDistance += (scroll * ZoomSmoothing);
            CamDistance = Mathf.Clamp(CamDistance, CamMinDistance, CamMaxDistance);
            _cam.transform.localPosition = new Vector3(0, 0, -CamDistance);
        }

        void FixedUpdate() {
            if (WhenUpdate == UpdateType.FixedUpdate) {
                FollowTarget(TimeManager.DeltaTime);
            }
        }

        void LateUpdate() {
            if (WhenUpdate == UpdateType.LateUpdate) {
                FollowTarget(TimeManager.DeltaTime);
            }
        }

        public void ManualUpdate() {
            if (WhenUpdate == UpdateType.ManualUpdate) {
                FollowTarget(TimeManager.DeltaTime);
            }
        }

        void FollowTarget(float deltaTime) {
            if (Player.Tr == null) {
                return;
            }

            Vector3 targetPosition = Player.Tr.position + TargetOffset;
            //if (Player.unit != null) {
            //    var targetLook = TargetLook * Vector3.Normalize(Player.unit.Tr.position - targetPosition) + targetPosition;
            //    var maxTargetLook = (targetPosition + MaxLookPercent * (Player.unit.Tr.position - targetPosition));
            //    Debug.DrawLine(targetPosition, targetLook, Color.green,4f);
            //    Debug.DrawLine(targetPosition, maxTargetLook, Color.red,4f);
            //    if (Distance.XZSqrMagnitude(targetPosition, targetLook) >
            //        Distance.XZSqrMagnitude(targetPosition, maxTargetLook)) {
            //        targetPosition = maxTargetLook;
            //    }
            //    else {
            //        targetPosition = targetLook;
            //    }
            //}
            transform.position = Vector3.Lerp(transform.position, targetPosition, deltaTime * MoveSpeed);
        }

        private void HandleRotationMovement() {
            if (Time.timeScale < float.Epsilon) {
                return;
            }
            SetRootRotation(PlayerInput.main.GetAxis(PlayerInput.Axis.MoveX));
            float y = PlayerInput.main.GetAxis(PlayerInput.Axis.LookY);
            _tiltAngle -= y * TurnSpeed;
            _tiltAngle = Mathf.Clamp(_tiltAngle, -TiltMin, TiltMax);
            _pivotTargetRot = Quaternion.Euler(_tiltAngle, _pivotEulers.y, _pivotEulers.z);
            if (TurnSmoothing > 0) {
                _pivot.localRotation = Quaternion.Slerp(_pivot.localRotation, _pivotTargetRot,
                    TurnSmoothing * TimeManager.DeltaTime);
            }
            else {
                _pivot.localRotation = _pivotTargetRot;
            }
        }

        private void SetRootRotation(float x) {
            _lookAngle += x * TurnSpeed;
            _transformTargetRot = Quaternion.Euler(0f, _lookAngle, 0f);
            if (TurnSmoothing > 0) {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, _transformTargetRot,
                    TurnSmoothing * TimeManager.DeltaTime);
            }
            else {
                transform.localRotation = _transformTargetRot;
            }
        }
    }
}