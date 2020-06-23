using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace PixelComrades {
    [AutoRegister]
    public sealed class RtsCameraSystem : SystemWithSingleton<RtsCameraSystem, RtsCameraComponent>, IMainSystemUpdate, IMainLateUpdate {
        
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
        private MeshRenderer _targetRenderer;
        private float _defaultRotateTilt;
        private float _distance;
        private float _rotateSpeed;
        private float _tiltSpeed;
        private float _lookAtHeightOffset;
        private bool _followBehind;
        private float _currentHeight;
        
        private RtsCameraConfig Config { get { return Current.Config; } }
        private Camera Cam { get { return Current.Cam; } }
        public Transform FollowTarget { get { return Current.FollowTr; } }
        public Transform CamTargetMarker { get { return Current.CamTargetMarker; } }
        public bool IsFollowing { get { return FollowTarget != null; } }
        public bool RunTestInput { get; set; }

        public RtsCameraSystem() {
            MessageKit<Transform>.addObserver(Messages.CameraFocusChanged, Follow);
            RunTestInput = false;
        }

        protected override void SetCurrent(RtsCameraComponent current) {
            base.SetCurrent(current);
            if (current == null) {
                return;
            }
            _defaultRotateTilt = Mathf.Max(Config.RotateSpeed, Config.TiltSpeed);
            _currTilt = current.Tr.rotation.eulerAngles.x;
            _currRotation = current.Tr.rotation.eulerAngles.y;
            _distance = _initialDistance = Config.StartingDistance;
            _currDistance = Config.DistanceRange.Lerp(_distance);
            _lookAt = current.Tr.position;
            _currentHeight = current.Tr.position.y;
            _initialRotation = _rotation = _currRotation;
            _initialTilt = _tilt = _currTilt;
            
            UpdateConfig();
            CreateTarget();
        }

        public void UpdateConfig() {
            _rotateSpeed = Config.RotateSpeed;
            _lookAtHeightOffset = Config.LookAtHeightOffset;
            _followBehind = Config.FollowBehind;
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            if (Current == null || !Current.Active) {
                return;
            }
            if (Config.AutoInput) {
                if (RunTestInput) {
                    TestInputUpdate();   
                }
                else {
                    UpdateInput();
                }
            }
            if (_lastDebugCamera != Config.ShowDebugCameraTarget) {
                if (_targetRenderer != null) {
                    _targetRenderer.enabled = Config.ShowDebugCameraTarget;
                    _lastDebugCamera = Config.ShowDebugCameraTarget;
                }
            }
        }
        
        public void ResetPosition() {
            ResetToInitialValues(true, true);
        }

        public void SetCameraMode(string mode) {
            switch (mode) {
                case "Overhead":
                    Cam.orthographic = true;
                    _rotateSpeed = _tiltSpeed = 0;
                    break;
                default:
                    Cam.orthographic = false;
                    _rotateSpeed = _tiltSpeed = _defaultRotateTilt;
                    break;
            }
        }

        public void OnSystemLateUpdate(float dt, float unscaledDt) {
            if (Current == null || !Current.Active) {
                return;
            }
            if (Current.FollowTr != null) {
                _lookAt = Current.FollowTr.position;
            }
            else {
                _moveVector.y = 0;
                _lookAt += Quaternion.Euler(0, _rotation, 0) * _moveVector;
                _lookAt.y = GetHeightAt(_lookAt.x, _lookAt.z);
            }
            _lookAt.y += _lookAtHeightOffset;
            if (Config.LimitPos && !Current.CameraLimitSpace.bounds.Contains(_lookAt)) {
                _lookAt = Current.CameraLimitSpace.ClosestPointOnBounds(_lookAt);
            }
            _tilt = Config.TiltRange.Clamp(_tilt);
            //LookAt = new Vector3(Mathf.Clamp(LookAt.x, MinBounds.x, MaxBounds.x),
            //    Mathf.Clamp(LookAt.y, MinBounds.y, MaxBounds.y), Mathf.Clamp(LookAt.z, MinBounds.z, MaxBounds.z));
            
            if (Config.Smoothing) {
                _currRotation = Mathf.LerpAngle(_currRotation, _rotation, TimeManager.DeltaUnscaled * Config.RotationDampening);
                _currDistance = Mathf.Lerp(_currDistance, Config.DistanceRange.Lerp(_distance), TimeManager.DeltaUnscaled * Config.ZoomDampening);
                _currTilt = Mathf.LerpAngle(_currTilt, _tilt, TimeManager.DeltaUnscaled * Config.TiltDampening);
                CamTargetMarker.position = Vector3.Lerp(CamTargetMarker.position, _lookAt, TimeManager.DeltaUnscaled * Config.MoveDampening);
            }
            else {
                _currRotation = _rotation;
                _currDistance = Config.DistanceRange.Lerp(_distance);
                _currTilt = _tilt;
                CamTargetMarker.position = _lookAt;
            }

            _moveVector = Vector3.zero;
            // if we're following AND forcing behind, override the rotation to point to target (with offset)
            if (Current.FollowTr != null && _followBehind) {
                ForceFollowBehind();
            }

            // optionally, we'll check to make sure the target is visible
            // Note: we only do this when following so that we don't "jar" when moving manually
            if (Current.FollowTr != null && Config.TargetVisbilityViaPhysics && DistanceToTargetIsLessThan(1f)) {
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
                _currDistance = Config.DistanceRange.Lerp(_distance);
                _currRotation = _rotation;
                _currTilt = _tilt;
                CamTargetMarker.position = _lookAt;
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
            _currentHeight = _lookAt.y;
            if (snap) {
                CamTargetMarker.position = toPosition;
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
            Current.FollowTr = followTarget;

            if (Current.FollowTr != null) {
                if (snap) {
                    _lookAt = Current.FollowTr.position;
                    _currentHeight = _lookAt.y;
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
            if (Config.TerrainHeightViaPhysics) {
                RaycastHit hitInfo;
                if (Physics.Raycast(new Vector3(x, Current.Tr.position.y, z), new Vector3(0, -1, 0), out hitInfo, 350f, Config.TerrainPhysicsLayerMask)) {
                    return hitInfo.point.y;
                }
                return _currentHeight; // no hit!
            }
            return _currentHeight;
        }

        private void UpdateCamera() {
            Quaternion rotation = Quaternion.Euler(_currTilt, _currRotation, 0);
            var v = new Vector3(0.0f, 0.0f, -_currDistance);
            Vector3 position = rotation * v + CamTargetMarker.position;

            if (Cam.orthographic) {
                Cam.orthographicSize = Config.OrthoDistance.Lerp(_distance);
            }
            if (Config.UseVisibilityCheck) {
                float y = GetHeightAt(position.x, position.z) + 1;
                if (y > position.y) {
                    position.y = y;
                }                
            }
            // update position and rotation of camera
            Current.Tr.rotation = rotation;
            Current.Tr.position = position;
        }

        private void CreateTarget() {
            if (Current.CamTargetMarker != null) {
                return;
            }
            Current.CamTargetMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            Current.CamTargetMarker.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            Current.CamTargetMarker.GetComponent<Renderer>().material.color = Color.green;

            var targetCollider = Current.CamTargetMarker.GetComponent<Collider>();
            if (targetCollider != null) {
                targetCollider.enabled = false;
            }

            _targetRenderer = Current.CamTargetMarker.GetComponent<MeshRenderer>();
            _targetRenderer.enabled = false;

            Current.CamTargetMarker.name = "CameraTarget " + Current.Cam.name;
            Current.CamTargetMarker.transform.position = _lookAt;
        }

        private bool DistanceToTargetIsLessThan(float sqrDistance) {
            if (Current.FollowTr == null) {
                return true; // our distance is technically zero
            }
            Vector3 p1 = CamTargetMarker.position;
            Vector3 p2 = FollowTarget.position;
            p1.y = p2.y = 0; // ignore height offset
            Vector3 v = p1 - p2;
            float vd = v.sqrMagnitude; // use sqr for performance

            return vd < sqrDistance;
        }

        private void EnsureTargetIsVisible() {
            Vector3 direction = (Current.Tr.position - CamTargetMarker.position);
            direction.Normalize();
            float distance = Config.DistanceRange.Lerp(_distance);
            RaycastHit hitInfo;
            //if (Physics.Raycast(_target.transform.position, direction, out hitInfo, distance, ~TargetVisibilityIgnoreLayerMask))
            if (Physics.SphereCast(
                CamTargetMarker.position, Config.CameraRadius, direction, out hitInfo, distance,
                ~ Config.TargetVisibilityIgnoreLayerMask)) {
                if (hitInfo.transform != CamTargetMarker) // don't collide with outself!
                {
                    _currDistance = hitInfo.distance - 0.1f;
                }
            }
        }

        private void ForceFollowBehind() {
            Vector3 v = FollowTarget.transform.forward * -1;
            float angle = Vector3.Angle(Vector3.forward, v);
            float sign = (Vector3.Dot(v, Vector3.right) > 0.0f) ? 1.0f : -1.0f;
            _currRotation = _rotation = 180f + (sign * angle) + Config.FollowRotationOffset;
        }

        public void UpdateInput(Vector2 move, float scroll, bool rotate) {
            _distance -= Mathf.Clamp01(scroll * Config.ZoomSpeed * TimeManager.DeltaUnscaled);
            if (rotate) {
                float tilt = move.y;
                _tilt -= tilt * _tiltSpeed * TimeManager.DeltaUnscaled;

                float rot = move.x;
                _rotation += rot * _rotateSpeed * TimeManager.DeltaUnscaled;
            }
            else if (Config.MoveCamera) {
                float speed = Config.MoveSpeed;
                if (PlayerInputSystem.GetKeyDown(Config.FastMoveKeyCode)) {
                    speed = Config.FastMoveSpeed;
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
            if (PlayerInputSystem.GetKeyDown(Config.BreakFollowKey)) {
                EndFollow();
            }
            float scroll = Mouse.current.scroll.ReadValue().y;
            _distance = Mathf.Clamp01(_distance - (scroll * Config.ZoomSpeed * TimeManager.DeltaUnscaled));
            if (PlayerInputSystem.GetMouseButtonDown(Config.MouseOrbitButton)) {
                float tilt = PlayerInputSystem.LookInput.y;
                _tilt -= tilt * _tiltSpeed * TimeManager.DeltaUnscaled;

                float rot = PlayerInputSystem.LookInput.x;
                _rotation += rot * _rotateSpeed * TimeManager.DeltaUnscaled;
            }
            if (!Config.MoveCamera) {
                return;
            }
            float speed = Config.MoveSpeed;
            if (PlayerInputSystem.GetKeyDown(Config.FastMoveKeyCode)) {
                speed = Config.FastMoveSpeed;
            }

            float h = PlayerInputSystem.MoveInput.x;
            if (Mathf.Abs(h) > 0.001f) {
                AddToPosition(h * speed * TimeManager.DeltaUnscaled, 0, 0);
            }

            float v = PlayerInputSystem.MoveInput.y;
            if (Mathf.Abs(v) > 0.001f) {
                AddToPosition(0, 0, v * speed * TimeManager.DeltaUnscaled);
            }
        }

        private void TestInputUpdate() {
            if (GetKey(Config.BreakFollowKey)) {
                EndFollow();
            }
            var mousePos = Mouse.current.position.ReadValue();
            if (GetMouseButton(0)) {
                var ray = Cam.ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray, out var hitInfo, 5000f, LayerMasks.Actor)) {
                    Follow(hitInfo.transform);
                }
            }
            float scroll = Mouse.current.scroll.ReadValue().y;
            _distance -= scroll * Config.ZoomSpeed * TimeManager.DeltaUnscaled;
            _distance = Mathf.Clamp01(_distance);
            var mouseMovement = Mouse.current.delta.ReadValue();
            if (GetMouseButton(Config.MouseOrbitButton)) {
                float tilt = mouseMovement.y;
                _tilt -= tilt * _tiltSpeed * TimeManager.DeltaUnscaled;

                float rot = mouseMovement.x;
                _rotation += rot * _rotateSpeed * TimeManager.DeltaUnscaled;
            }
            if (!Config.MoveCamera) {
                return;
            }
            float speed = Config.MoveSpeed;
            if (GetKey(Config.FastMoveKeyCode)) {
                speed = Config.FastMoveSpeed;
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
            
            if (Config.AllowScreenEdgeMove && (!IsFollowing || Config.ScreenEdgeMoveBreaksFollow)) {
                var hasMovement = false;

                if (mousePos.y > (Screen.height - Config.ScreenEdgeBorderWidth)) {
                    hasMovement = true;
                    AddToPosition(0, 0, speed * TimeManager.DeltaUnscaled);
                }
                else if (mousePos.y < Config.ScreenEdgeBorderWidth) {
                    hasMovement = true;
                    AddToPosition(0, 0, -1 * speed * TimeManager.DeltaUnscaled);
                }

                if (mousePos.x > (Screen.width - Config.ScreenEdgeBorderWidth)) {
                    hasMovement = true;
                    AddToPosition(speed * TimeManager.DeltaUnscaled, 0, 0);
                }
                else if (mousePos.x < Config.ScreenEdgeBorderWidth) {
                    hasMovement = true;
                    AddToPosition(-1 * speed * TimeManager.DeltaUnscaled, 0, 0);
                }

                if (hasMovement && IsFollowing && Config.ScreenEdgeMoveBreaksFollow) {
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
