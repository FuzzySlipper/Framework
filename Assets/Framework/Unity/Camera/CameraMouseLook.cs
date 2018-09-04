using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace PixelComrades {
    public class CameraMouseLook : MonoSingleton<CameraMouseLook> {

        [Tooltip("Should the rotation around x-axis be clamped?")] [SerializeField] private bool _shouldClampPitch = true;
        [Tooltip("How fast the cursor moves in response to mouse lateral (x-axis) movement.")] [SerializeField] private float _lateralSensitivity = 2.0f;
        [Tooltip("The maximum pitch angle (in degrees).")] [SerializeField] private float _maxPitchAngle = 90.0f;
        [Tooltip("The minimum pitch angle (in degrees).")] [SerializeField] private float _minPitchAngle = -90.0f;
        [Tooltip("Should the rotation be smoothed (eg: interpolated)?")] [SerializeField] private bool _smooth;
        [Tooltip("How fast the cursor moves in response to mouse vertical (y-axis) movement.")] [SerializeField] private float _verticalSensitivity = 2.0f;
        [Tooltip("Approximately the time (in secs) it will take to reach target.\n" + "A smaller value will reach the target faster.")] [SerializeField] private float _smoothTime = 5.0f;
        [SerializeField] private Transform _camTr = null;
        [SerializeField] private Transform _pivotTr = null;
        [SerializeField] private GridCamera _gridCamera = null;
        

        private Quaternion _cameraTargetRotation;
        private Quaternion _characterTargetRotation;

        public Transform Pivot { get { return _pivotTr; } }
        public float LateralSensitivity { get { return _lateralSensitivity; } set { _lateralSensitivity = MathEx.Max(0.0f, value); } }
        public float VerticalSensitivity { get { return _verticalSensitivity; } set { _verticalSensitivity = MathEx.Max(0.0f, value); } }
        public bool Smooth { get { return _smooth; } set { _smooth = value; } }
        public float SmoothTime { get { return _smoothTime; } set { _smoothTime = MathEx.Max(0.0f, value); } }
        public bool ShouldClampPitch { get { return _shouldClampPitch; } set { _shouldClampPitch = value; } }
        public float MinPitchAngle { get { return _minPitchAngle; } set { _minPitchAngle = Mathf.Clamp(value, -180.0f, 180.0f); } }
        public float MaxPitchAngle { get { return _maxPitchAngle; } set { _maxPitchAngle = Mathf.Clamp(value, -180.0f, 180.0f); } }

        void OnValidate() {
            LateralSensitivity = _lateralSensitivity;
            VerticalSensitivity = _verticalSensitivity;

            SmoothTime = _smoothTime;

            MinPitchAngle = _minPitchAngle;
            MaxPitchAngle = _maxPitchAngle;
        }

        void Awake() {
            _characterTargetRotation = _pivotTr.localRotation;
            _cameraTargetRotation = _camTr.localRotation;
            // don't forget about keyboard turning
        }

        void LateUpdate() {
            if (!Game.GameActive) {
                return;
            }
            if (GameOptions.MouseLook) {
                LookRotation();
            }
            else {
                _gridCamera.LookRotation();
            }
        }

        public void ChangeRotation(float yRot) {
            _pivotTr.localRotation = _characterTargetRotation = Quaternion.Euler(0, yRot, 0);
            _camTr.localRotation = _cameraTargetRotation = Quaternion.identity;
        }

        public void LookRotation() {
            if (Cursor.lockState != CursorLockMode.Locked) {
                return;
            }
            Quaternion currentRotation = _pivotTr.localRotation;
            var yaw = Input.GetAxis("Mouse X") * LateralSensitivity;
            var pitch = Input.GetAxis("Mouse Y") * VerticalSensitivity;

            _characterTargetRotation *= Quaternion.Euler(0.0f, yaw, 0.0f);
            _cameraTargetRotation *= Quaternion.Euler(-pitch, 0.0f, 0.0f);

            if (ShouldClampPitch) {
                _cameraTargetRotation = ClampPitch(_cameraTargetRotation);
            }
            if (Smooth) {
                currentRotation = Quaternion.Slerp(currentRotation, _characterTargetRotation,
                    SmoothTime * Time.deltaTime);
                _camTr.localRotation = Quaternion.Slerp(_camTr.localRotation, _cameraTargetRotation,
                    SmoothTime * Time.deltaTime);
            }
            else {
                currentRotation = _characterTargetRotation;
                _camTr.localRotation = _cameraTargetRotation;
            }
            _pivotTr.localRotation = currentRotation;
        }

        private Quaternion ClampPitch(Quaternion q) {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            var pitch = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            pitch = Mathf.Clamp(pitch, MinPitchAngle, MaxPitchAngle);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * pitch);

            return q;
        }
    }
}
