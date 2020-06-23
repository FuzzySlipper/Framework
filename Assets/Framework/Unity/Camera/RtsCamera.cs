using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace PixelComrades {
    public class RtsCamera : MonoBehaviour, IPoolEvents, ISimpleCam {

        [SerializeField] private float _lookAtHeightOffset = 1;// Y coordinate of camera target.   Only used if TerrainHeightViaPhysics and GetTerrainHeight are not set.
        [SerializeField] private FloatRange _tiltRange = new FloatRange(-360, 360);
        [SerializeField] private FloatRange _distanceRange = new FloatRange(50, 500);
        [SerializeField] private bool _smoothing = false; // Should the camera "slide" between positions and targets?
        [SerializeField] private float _moveDampening = 0.75f; // How "smooth" should the camera moves be?  Note: Smaller numbers are smoother
        [SerializeField] private float _rotationDampening = 0.75f; // How "smooth" should the camera rotations be?  Note: Smaller numbers are smoother
        [SerializeField] private float _tiltDampening = 0.75f; // How "smooth" should the camera tilts be?  Note: Smaller numbers are smoother
        [SerializeField] private float _zoomDampening = 0.75f; // How "smooth" should the camera zooms be?  Note: Smaller numbers are smoother
        [Header("Visibility")]
        [SerializeField] private bool _useVisibilityCheck = false;
        [SerializeField] private bool _showDebugCameraTarget = false; // If set, "small green sphere" will be shown indicating camera target position (even when Following)
        [SerializeField] private float _cameraRadius = 1f;
        [SerializeField] private bool _targetVisbilityViaPhysics = false; // If set, camera will raycast from target out in order to avoid objects being between target and camera
        [SerializeField] private LayerMask _targetVisibilityIgnoreLayerMask = 0; // Layer mask to ignore when raycasting to determine camera visbility
        [SerializeField] private bool _terrainHeightViaPhysics = false; // If set, camera will automatically raycast against terrain (using TerrainPhysicsLayerMask) to determine height 
        [SerializeField] private LayerMask _terrainPhysicsLayerMask = 0; // Layer mask indicating which layers the camera should ray cast against for height detection
        [Header("Follow")]
        [SerializeField] private bool _followBehind = false; // If set, keyboard and mouse rotation will be disabled when Following a target
        [SerializeField] private float _followRotationOffset = 0; // Offset (degrees from zero) when forcing follow behind target
        [Header("Input")]
        [SerializeField] private bool _moveCamera = true;
        [SerializeField] private int _mouseOrbitButton = 1;
        [SerializeField] private float _orthoMulti = 0.75f;
        [SerializeField] private float _moveSpeed = 50;
        [SerializeField] private float _rotateSpeed = 50f;
        [SerializeField] private float _tiltSpeed = 50f;
        [SerializeField] private float _zoomSpeed = 700f;
        [SerializeField] private float _fastMoveSpeed = 300f;
        [SerializeField] private Key _fastMoveKeyCode1 = Key.LeftShift;
        [SerializeField] private Key _breakFollowKey = Key.Escape;
        [SerializeField] private Transform _followTarget;
        [SerializeField] private bool _limitPos = true;
        [SerializeField] private Collider _cameraLimitSpace = null;
        [SerializeField] private bool _autoInput = false;
        [SerializeField] private float _distance = 0;
        [SerializeField] private bool _allowScreenEdgeMove = true;
        [SerializeField] private bool _screenEdgeMoveBreaksFollow = true;
        [SerializeField] private int _screenEdgeBorderWidth = 4;

        private Vector3 _lookAt;
        private float _rotation;
        private float _currDistance; // actual distance
        private float _currRotation; // actual rotation
        private float _currTilt; // actual tilt
        private float _initialDistance;
        private float _tilt; // Desired tilt (degrees)
        private float _initialRotation;
        private float _initialTilt;
        private bool _lastDebugCamera;
        private Vector3 _moveVector;
        private GameObject _target;
        private MeshRenderer _targetRenderer;
        private Camera _camera;
        private float _defaultRotateTilt;

        public Transform CameraTarget { get { return _target.transform; } }
        public bool IsFollowing { get { return FollowTarget != null; } }
        public Transform FollowTarget { get { return _followTarget; } }
        public float LookAtHeightOffset { get => _lookAtHeightOffset; set => _lookAtHeightOffset = value; }
        public float PanUpDownAxis { get => _lookAtHeightOffset; set => _lookAtHeightOffset += value; }
        
        void Awake() {
            _camera = GetComponentInChildren<Camera>();
        }

        void Start() {
            _defaultRotateTilt = Mathf.Max(_rotateSpeed, _tiltSpeed);
            _currTilt = transform.rotation.eulerAngles.x;
            _currRotation = transform.rotation.eulerAngles.y;
            _distance = _distanceRange.Clamp(_distance);
            _lookAt = transform.position;
            _initialDistance = _currDistance = _distance;
            _initialRotation = _rotation = _currRotation;
            _initialTilt = _tilt = _currTilt;
            // set our current values to the desired values so that we don't "slide in"
            CreateTarget();
        }

        public void OnPoolSpawned() {
            MessageKit<Transform>.addObserver(Messages.CameraFocusChanged, Follow);
        }

        public void OnPoolDespawned() {
            MessageKit<Transform>.removeObserver(Messages.CameraFocusChanged, Follow);
        }

        protected void Update() {
            //if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) {
            //    return;
            //}
            if (_autoInput) {
                UpdateInput();
            }
            if (_lastDebugCamera != _showDebugCameraTarget) {
                if (_targetRenderer != null) {
                    _targetRenderer.enabled = _showDebugCameraTarget;
                    _lastDebugCamera = _showDebugCameraTarget;
                }
            }
        }

        public void ResetPosition() {
            ResetToInitialValues(true, true);
        }

        public void SetCameraMode(string mode) {
            switch (mode) {
                case "Overhead":
                    _camera.orthographic = true;
                    _rotateSpeed = _tiltSpeed = 0;
                    break;
                default:
                    _camera.orthographic = false;
                    _rotateSpeed = _tiltSpeed = _defaultRotateTilt;
                    break;
            }
        }

        public void MoverUpdate() {
            UpdateInput();
        }

        protected void LateUpdate() {
            if (IsFollowing) {
                _lookAt = _followTarget.position;
            }
            else {
                _moveVector.y = 0;
                _lookAt += Quaternion.Euler(0, _rotation, 0) * _moveVector;
                _lookAt.y = GetHeightAt(_lookAt.x, _lookAt.z);
            }
            _lookAt.y += _lookAtHeightOffset;
            if (_limitPos && !_cameraLimitSpace.bounds.Contains(_lookAt)) {
                _lookAt = _cameraLimitSpace.ClosestPointOnBounds(_lookAt);
            }
            _tilt = _tiltRange.Clamp(_tilt);
            _distance = _distanceRange.Clamp(_distance);
            //LookAt = new Vector3(Mathf.Clamp(LookAt.x, MinBounds.x, MaxBounds.x),
            //    Mathf.Clamp(LookAt.y, MinBounds.y, MaxBounds.y), Mathf.Clamp(LookAt.z, MinBounds.z, MaxBounds.z));
            
            if (_smoothing) {
                _currRotation = Mathf.LerpAngle(_currRotation, _rotation, TimeManager.DeltaUnscaled * _rotationDampening);
                _currDistance = Mathf.Lerp(_currDistance, _distance, TimeManager.DeltaUnscaled * _zoomDampening);
                _currTilt = Mathf.LerpAngle(_currTilt, _tilt, TimeManager.DeltaUnscaled * _tiltDampening);
                _target.transform.position = Vector3.Lerp(_target.transform.position, _lookAt, TimeManager.DeltaUnscaled * _moveDampening);
            }
            else {
                _currRotation = _rotation;
                _currDistance = _distance;
                _currTilt = _tilt;
                _target.transform.position = _lookAt;
            }

            _moveVector = Vector3.zero;
            // if we're following AND forcing behind, override the rotation to point to target (with offset)
            if (IsFollowing && _followBehind) {
                ForceFollowBehind();
            }

            // optionally, we'll check to make sure the target is visible
            // Note: we only do this when following so that we don't "jar" when moving manually
            if (IsFollowing && _targetVisbilityViaPhysics && DistanceToTargetIsLessThan(1f)) {
                EnsureTargetIsVisible();
            }
            // recalculate the actual position of the camera based on the above
            UpdateCamera();
        }

        /// <summary>
        ///     Reset camera to initial (startup) position, distance, rotation, tilt, etc.
        /// </summary>
        /// <param name="includePosition">If true, position will be reset as well.  If false, only distance/rotation/tilt.</param>
        /// <param name="snap">
        ///     If true, camera will snap instantly to the position.  If false, camera will slide smoothly back to
        ///     initial values.
        /// </param>
        public void ResetToInitialValues(bool includePosition, bool snap = false) {
            _distance = _initialDistance;
            _rotation = _initialRotation;
            _tilt = _initialTilt;

            if (snap) {
                _currDistance = _distance;
                _currRotation = _rotation;
                _currTilt = _tilt;
                _target.transform.position = _lookAt;
            }
        }

        /// <summary>
        ///     Manually set target position (snap or slide).
        /// </summary>
        /// <param name="toPosition">Vector3 position</param>
        /// <param name="snap">If true, camera will "snap" to the position, else will "slide"</param>
        public void JumpTo(Vector3 toPosition, bool snap = false) {
            EndFollow();

            _lookAt = toPosition;

            if (snap) {
                _target.transform.position = toPosition;
            }
        }

        /// <summary>
        ///     Manually set target position (snap or slide).
        /// </summary>
        /// <param name="toTransform">Transform to which the camera target will be moved</param>
        /// <param name="snap">If true, camera will "snap" to the position, else will "slide"</param>
        public void JumpTo(Transform toTransform, bool snap = false) {
            JumpTo(toTransform.position, snap);
        }

        public void JumpTo(GameObject toGameObject, bool snap = false) {
            JumpTo(toGameObject.transform.position, snap);
        }

        /// <summary>
        ///     Set current auto-follow target (snap or slide).
        /// </summary>
        /// <param name="followTarget">Transform which the camera should follow</param>
        /// <param name="snap">If true, camera will "snap" to the position, else will "slide"</param>
        public void Follow(Transform followTarget, bool snap) {
            _followTarget = followTarget;

            if (_followTarget != null) {
                if (snap) {
                    _lookAt = _followTarget.position;
                }
            }
        }

        public void Follow(Transform followTarget) {
            Follow(followTarget, false);
        }

        public void EndFollow() {
            Follow((Transform)null, false);
        }

        /// <summary>
        ///     Adds movement to the camera (world coordinates).
        /// </summary>
        /// <param name="dx">World coordinate X distance to move</param>
        /// <param name="dy">World coordinate Y distance to move</param>
        /// <param name="dz">World coordinate Z distance to move</param>
        public void AddToPosition(float dx, float dy, float dz) {
            _moveVector += new Vector3(dx, dy, dz);
        }

        /// <summary>
        ///     If "GetTerrainHeight" function set, will call to obtain desired camera height (y position).
        ///     Else, if TerrainHeightViaPhysics is true, will use Physics.RayCast to determine camera height.
        ///     Else, will assume flat terrain and will return "0" (which will later be offset by LookAtHeightOffset)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private float GetHeightAt(float x, float z) {
            //
            // priority 2:  use physics ray casting to get height at point
            //
            if (_terrainHeightViaPhysics) {
                RaycastHit hitInfo;
                if (Physics.Raycast(new Vector3(x, transform.position.y, z), new Vector3(0, -1, 0), out hitInfo, 350f, _terrainPhysicsLayerMask)) {
                    return hitInfo.point.y;
                }
                return 0; // no hit!
            }
            return 0;
        }

        private void UpdateCamera() {
            Quaternion rotation = Quaternion.Euler(_currTilt, _currRotation, 0);
            var v = new Vector3(0.0f, 0.0f, -_currDistance * (_camera.orthographic ? _orthoMulti : 1 ));
            Vector3 position = rotation * v + _target.transform.position;

            if (_camera.orthographic) {
                _camera.orthographicSize = _currDistance * _orthoMulti;
            }
            if (_useVisibilityCheck) {
                float y = GetHeightAt(position.x, position.z) + 1;
                if (y > position.y) {
                    position.y = y;
                }                
            }
            // update position and rotation of camera
            transform.rotation = rotation;
            transform.position = position;
        }

        private void CreateTarget() {
            _target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _target.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            _target.GetComponent<Renderer>().material.color = Color.green;

            var targetCollider = _target.GetComponent<Collider>();
            if (targetCollider != null) {
                targetCollider.enabled = false;
            }

            _targetRenderer = _target.GetComponent<MeshRenderer>();
            _targetRenderer.enabled = false;

            _target.name = "CameraTarget";
            _target.transform.position = _lookAt;
        }

        private bool DistanceToTargetIsLessThan(float sqrDistance) {
            if (!IsFollowing) {
                return true; // our distance is technically zero
            }
            Vector3 p1 = _target.transform.position;
            Vector3 p2 = _followTarget.position;
            p1.y = p2.y = 0; // ignore height offset
            Vector3 v = p1 - p2;
            float vd = v.sqrMagnitude; // use sqr for performance

            return vd < sqrDistance;
        }

        private void EnsureTargetIsVisible() {
            Vector3 direction = (transform.position - _target.transform.position);
            direction.Normalize();
            float distance = _distance;
            RaycastHit hitInfo;
            //if (Physics.Raycast(_target.transform.position, direction, out hitInfo, distance, ~TargetVisibilityIgnoreLayerMask))
            if (Physics.SphereCast(_target.transform.position, _cameraRadius, direction, out hitInfo, distance,
                ~_targetVisibilityIgnoreLayerMask)) {
                if (hitInfo.transform != _target) // don't collide with outself!
                {
                    _currDistance = hitInfo.distance - 0.1f;
                }
            }
        }

        private void ForceFollowBehind() {
            Vector3 v = _followTarget.transform.forward * -1;
            float angle = Vector3.Angle(Vector3.forward, v);
            float sign = (Vector3.Dot(v, Vector3.right) > 0.0f) ? 1.0f : -1.0f;
            _currRotation = _rotation = 180f + (sign * angle) + _followRotationOffset;
        }

        public void UpdateInput(Vector2 move, float scroll, bool rotate) {
            _distance -= scroll * _zoomSpeed * TimeManager.DeltaUnscaled;
            if (rotate) {
                float tilt = move.y;
                _tilt -= tilt * _tiltSpeed * TimeManager.DeltaUnscaled;

                float rot = move.x;
                _rotation += rot * _rotateSpeed * TimeManager.DeltaUnscaled;
            }
            else if (_moveCamera) {
                float speed = _moveSpeed;
                if (GetKey(_fastMoveKeyCode1)) {
                    speed = _fastMoveSpeed;
                }
                float h = move.x;
                if (Mathf.Abs(h) > 0.001f) {
                    AddToPosition(h * speed * TimeManager.DeltaUnscaled, 0, 0);
                }

                float v = move.y;
                if (Mathf.Abs(v) > 0.001f) {
                    AddToPosition(0, 0, v * speed * TimeManager.DeltaUnscaled);
                }
            }
        }

        public void UpdateInput() {
            if (GetKey(_breakFollowKey)) {
                EndFollow();
            }
            var mousePos = Mouse.current.position.ReadValue();
            if (GetMouseButton(0)) {
                var ray = _camera.ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray, out var hitInfo, 5000f, LayerMasks.Actor)) {
                    Follow(hitInfo.transform);
                }
            }
            float scroll = Mouse.current.scroll.ReadValue().y;
            _distance -= scroll * _zoomSpeed * TimeManager.DeltaUnscaled;
            var mouseMovement = Mouse.current.delta.ReadValue();
            if (GetMouseButton(_mouseOrbitButton)) {
                float tilt = mouseMovement.y;
                _tilt -= tilt * _tiltSpeed * TimeManager.DeltaUnscaled;

                float rot = mouseMovement.x;
                _rotation += rot * _rotateSpeed * TimeManager.DeltaUnscaled;
            }
            if (!_moveCamera) {
                return;
            }
            float speed = _moveSpeed;
            if (GetKey(_fastMoveKeyCode1)) {
                speed = _fastMoveSpeed;
            }

            float h = 0;
            if (GetKey(Key.D)) {
                h = 1;
            }
            else if (GetKey(Key.A)) {
                h = -1;
            }
            float v = 0;
            if (GetKey(Key.W)) {
                v = 1;
            }
            else if (GetKey(Key.S)) {
                v = -1;
            }
            
            if (_allowScreenEdgeMove && (!IsFollowing || _screenEdgeMoveBreaksFollow)) {
                var hasMovement = false;

                if (mousePos.y > (Screen.height - _screenEdgeBorderWidth)) {
                    hasMovement = true;
                    AddToPosition(0, 0, speed * TimeManager.DeltaUnscaled);
                }
                else if (mousePos.y < _screenEdgeBorderWidth) {
                    hasMovement = true;
                    AddToPosition(0, 0, -1 * speed * TimeManager.DeltaUnscaled);
                }

                if (mousePos.x > (Screen.width - _screenEdgeBorderWidth)) {
                    hasMovement = true;
                    AddToPosition(speed * TimeManager.DeltaUnscaled, 0, 0);
                }
                else if (mousePos.x < _screenEdgeBorderWidth) {
                    hasMovement = true;
                    AddToPosition(-1 * speed * TimeManager.DeltaUnscaled, 0, 0);
                }

                if (hasMovement && IsFollowing && _screenEdgeMoveBreaksFollow) {
                    EndFollow();
                }
            }
            
            if (Mathf.Abs(h) > 0.001f) {
                AddToPosition(h * speed * TimeManager.DeltaUnscaled, 0, 0);
            }
            if (Mathf.Abs(v) > 0.001f) {
                AddToPosition(0, 0, v * speed * TimeManager.DeltaUnscaled);
            }
        }
        public static bool GetMouseButton(int button) {
            if (button == 0) {
                return Mouse.current.leftButton.isPressed;
            }
            if (button == 1) {
                return Mouse.current.rightButton.isPressed;
            }
            if (button == 2) {
                return Mouse.current.middleButton.isPressed;
            }
            return false;
        }

        private bool GetKey(Key key) {
            return Keyboard.current[key].wasPressedThisFrame;
        }
    }
}