using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface ISimpleCam {
        void ResetPosition();
        void MoverUpdate();
        float PanUpDownAxis { get; set; }
    }
    public sealed class FreeCamera : MonoBehaviour, ISimpleCam {
        
        [SerializeField] private Camera _cam = null;

        [Header("Camera")] [SerializeField] private bool _levelCamera = true;
        [Range(0f,15f)] [SerializeField] private float _levelCameraAngleThreshold = 7.5f;

        [Header("Movement Speed")]
        [SerializeField] private float _movementSpeedMagnification = 100f;
        [SerializeField] private float _wheelMouseMagnification = 100f;
        [SerializeField] private float _shiftKeyMagnification = 2f;

        [Header("Pan Speed Modifications")]
            [SerializeField] private float _panLeftRightSensitivity = 100f;
        [SerializeField] private float _panUpDownSensitivity = 1001f;

        [Header("Mouse Rotation Sensitivity")]
        [SerializeField] private float _mouseRotationSensitivity = 2f;

        [Header("Look At")] 
        [SerializeField] private GameObject _lookAtTarget = null;
        [SerializeField] private float _minimumZoom = 20f;
        [SerializeField] private float _maximumZoom = 80f;
        [SerializeField] private int _mouseButtonRotate = 1;
        
        private Vector3 _initPosition;
        private Vector3 _initRotation;
        private float _res = 1f;
        
        public float PanUpDownAxis { get; set; }
        
        void Awake() {
            _initPosition = transform.position;
            _initRotation = transform.eulerAngles;
        }

        public void MoverUpdate() {
            UpdateMovement();
            if (_levelCamera) {
                LevelCamera();
            }
            if (_lookAtTarget != null ) {
                transform.LookAt(_lookAtTarget.transform);
                if (_cam.fieldOfView < _minimumZoom) {
                    _cam.fieldOfView = _minimumZoom;
                } else if (_cam.fieldOfView > _maximumZoom) {
                    _cam.fieldOfView = _maximumZoom;
                }
            }
        }

        public void ResetPosition() {
            transform.position = _initPosition;
            transform.eulerAngles = _initRotation;
        }

        private void LevelCamera() {
            Vector3 rotation = transform.rotation.eulerAngles;
            if (rotation.x > 180) {
                rotation.x -= 360;
            }
            if (rotation.x > (90 - _levelCameraAngleThreshold)) {
                rotation.x = (90 - _levelCameraAngleThreshold);
                transform.rotation = Quaternion.Euler(rotation);
            }
            else if (rotation.x < (-90 + _levelCameraAngleThreshold)) {
                rotation.x = -90 + _levelCameraAngleThreshold;
                transform.rotation = Quaternion.Euler(rotation);
            }
            transform.rotation = Quaternion.LookRotation(transform.forward.normalized, Vector3.up);
        }

        private void UpdateMovement() {
            var isFastModifier = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            var isRotateAction = Input.GetMouseButton(_mouseButtonRotate);

            var isPanLeft = Input.GetKey(KeyCode.A);
            var isPanRight = Input.GetKey(KeyCode.D);

            var isMoveForward = Input.GetKey(KeyCode.W);
            var isMoveBackward = Input.GetKey(KeyCode.S);

            var isMoveForwardAlt = Input.GetAxis("Mouse ScrollWheel") > 0;
            var isMoveBackwardAlt = Input.GetAxis("Mouse ScrollWheel") < 0;
            
            float mag = isFastModifier ? _shiftKeyMagnification : 1f;
            float xVel = 0f;
            if (isPanLeft) {
                xVel = -0.01f * mag * _res * _panLeftRightSensitivity;
            }
            else if (isPanRight) {
                xVel = 0.01f * mag * _res * _panLeftRightSensitivity;
            }
            float zVel = 0f;
            if (isMoveForward) {
                zVel = 0.005f * mag * _res * _movementSpeedMagnification;
            }
            else if (isMoveBackward) {
                zVel = -0.005f * mag * _res * _movementSpeedMagnification;
            }
            if (isMoveForwardAlt) {
                zVel = 0.005f * mag * _res * _movementSpeedMagnification * _wheelMouseMagnification;
            }
            else if (isMoveBackwardAlt) {
                zVel = -0.005f * mag * _res * _movementSpeedMagnification * _wheelMouseMagnification;
            }
            float yVel = PanUpDownAxis * (0.005f * mag * _res * _panUpDownSensitivity);
//            else if (isPanDown) {
//                yVel = -0.005f * mag * _res * _panUpDownSensitivity;
//            }
            if (_lookAtTarget != null) {
                _cam.fieldOfView += zVel;
                zVel = 0;
            }
            transform.Translate(xVel, yVel, zVel);

            if (isRotateAction) {
                transform.rotation *= Quaternion.AngleAxis(-Input.GetAxis("Mouse Y") * _mouseRotationSensitivity, Vector3.right);
                transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + Input.GetAxis("Mouse X") * _mouseRotationSensitivity, transform.eulerAngles.z);
            }
        }
    }
}
