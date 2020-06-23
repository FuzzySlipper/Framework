using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class CameraIsometric : MonoBehaviour {

        [SerializeField] private Transform _target = null;
        [SerializeField] private float _startingDistance = 10f;
        [SerializeField] private float _maxDistance = 20f;
        [SerializeField] private float _minDistance = 3f;
        [SerializeField] private float _zoomSpeed = 20f;
        [SerializeField] private float _targetHeight = 1.0f;
        [SerializeField] private float _camRotationSpeed = 70;
        [SerializeField] private float _camXAngle = 45.0f;
        [SerializeField] private bool _fadeObjects = false;
        [SerializeField] private LayerMask _transparencyMask = 0;
        [SerializeField] private float _fadeAlpha = 0.3f;
        [SerializeField] private float _minCameraAngle = 0.0f;
        [SerializeField] private float _maxCameraAngle = 90.0f;
        [SerializeField] private int _mouseRotateButton = 1;
        [SerializeField] private bool _autoUpdate = true;

        private float _camY = 0.0f;
        private Transform _tr;
        private Transform _prevHit;

        void Awake() {
            MessageKit<Transform>.addObserver(Messages.CameraFocusChanged, SetTarget);
        }

        public void SetTarget(Transform target) {
            _target = target;
            if (_target == null) {
                return;
            }
            _tr.position = _target.position;
        }

        void Start() {
            _tr = transform;
            Vector3 angles = _tr.eulerAngles;
            _camY = angles.y;
            if (_target != null) {
                SetTarget(_target);
                //UpdatePosition(0, 0, 0, false);
            }
        }

        void LateUpdate() {
            if (_target == null || !_autoUpdate) {
                return;
            }
            float mw = Input.GetAxis("Mouse ScrollWheel");
            float h = Input.GetAxis("Mouse X"); // The horizontal movement of the mouse.						
            float v = Input.GetAxis("Mouse Y"); // The vertical movement of the mouse.
            UpdatePosition(h, v, mw, Input.GetMouseButton(_mouseRotateButton));
        }

        public void UpdatePosition(float horizontal, float forward, float mw, bool doRotate) {
            if (mw > 0) {
                _startingDistance -= TimeManager.DeltaUnscaled * _zoomSpeed;
                if (_startingDistance < _minDistance) {
                    _startingDistance = _minDistance;
                }
            }
            else if (mw < 0) {
                _startingDistance += TimeManager.DeltaUnscaled * _zoomSpeed;
                if (_startingDistance > _maxDistance) {
                    _startingDistance = _maxDistance;
                }
            }
            if (doRotate) {
                if (horizontal > 0 && horizontal > Math.Abs(forward)) {
                    _tr.RotateAround(_target.transform.position, new Vector3(0, 1, 0), _camRotationSpeed * TimeManager.DeltaTime);
                    _camY = _tr.eulerAngles.y;
                }
                else if (horizontal < 0 && horizontal < -Math.Abs(forward)) {
                    _tr.RotateAround(_target.transform.position, new Vector3(0, 1, 0), -_camRotationSpeed * TimeManager.DeltaTime);
                    _camY = _tr.eulerAngles.y;
                }
                else if (forward > 0 && forward > Math.Abs(horizontal)) {
                    _camXAngle += _camRotationSpeed * TimeManager.DeltaTime;
                    if (_camXAngle > _maxCameraAngle) {
                        _camXAngle = _maxCameraAngle;
                    }
                }
                else if (forward < 0 && forward < -Math.Abs(horizontal)) {
                    _camXAngle += -_camRotationSpeed * TimeManager.DeltaTime;
                    if (_camXAngle < _minCameraAngle) {
                        _camXAngle = _minCameraAngle;
                    }
                }
            }
            Quaternion rotation = Quaternion.Euler(_camXAngle, _camY, 0);
            _tr.rotation = rotation;

            Vector3 trm = rotation * Vector3.forward * _startingDistance + new Vector3(0, -1 * _targetHeight, 0);
            Vector3 position = _target.position - trm;
            _tr.position = position;

            if (!_fadeObjects) {
                return;
            }
            Ray ray = new Ray(_tr.position, (_target.position - _tr.position).normalized);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, _maxDistance, _transparencyMask)) {
                Transform objectHit = hit.transform;
                if (_prevHit != null) {
                    _prevHit.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
                }
                if (objectHit.GetComponent<Renderer>() != null) {
                    _prevHit = objectHit;
                    // Can only apply alpha if this material shader is transparent.
                    _prevHit.GetComponent<Renderer>().material.color = new Color(1, 1, 1, _fadeAlpha);
                }
            }
            else if (_prevHit != null) {
                _prevHit.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
                _prevHit = null;
            }
        }
    }
}