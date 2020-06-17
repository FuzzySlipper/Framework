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
        private GameObject _target;
        private MeshRenderer _targetRenderer;
        private float _defaultRotateTilt;
        private float _distance = 0;
        private float _rotateSpeed;
        private float _tiltSpeed;
        private float _lookAtHeightOffset;
        private bool _followBehind;
        
        private RtsCameraConfig Config { get { return Current.Config; } }
        private Camera Cam { get { return Current.Cam; } }
        public Transform CameraTarget { get { return _target.transform; } }
        public Transform FollowTarget { get { return Current.FollowTr; } }
        
        public RtsCameraSystem() {
            MessageKit<Transform>.addObserver(Messages.CameraFocusChanged, Follow);
        }

        protected override void SetCurrent(RtsCameraComponent current) {
            base.SetCurrent(current);
            if (current == null) {
                return;
            }
            _defaultRotateTilt = Mathf.Max(Config.RotateSpeed, Config.TiltSpeed);
            _currTilt = current.Tr.rotation.eulerAngles.x;
            _currRotation = current.Tr.rotation.eulerAngles.y;
            _distance = current.Config.DistanceRange.Clamp(Config.StartingDistance);
            _lookAt = current.Tr.position;
            _initialDistance = _currDistance = _distance;
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
                UpdateInput();
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

        public void MoverUpdate() {
            UpdateInput();
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
            _distance = Config.DistanceRange.Clamp(_distance);
            //LookAt = new Vector3(Mathf.Clamp(LookAt.x, MinBounds.x, MaxBounds.x),
            //    Mathf.Clamp(LookAt.y, MinBounds.y, MaxBounds.y), Mathf.Clamp(LookAt.z, MinBounds.z, MaxBounds.z));
            
            if (Config.Smoothing) {
                _currRotation = Mathf.LerpAngle(_currRotation, _rotation, TimeManager.DeltaUnscaled * Config.RotationDampening);
                _currDistance = Mathf.Lerp(_currDistance, _distance, TimeManager.DeltaUnscaled * Config.ZoomDampening);
                _currTilt = Mathf.LerpAngle(_currTilt, _tilt, TimeManager.DeltaUnscaled * Config.TiltDampening);
                _target.transform.position = Vector3.Lerp(_target.transform.position, _lookAt, TimeManager.DeltaUnscaled * Config.MoveDampening);
            }
            else {
                _currRotation = _rotation;
                _currDistance = _distance;
                _currTilt = _tilt;
                _target.transform.position = _lookAt;
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
            Current.FollowTr = followTarget;

            if (Current.FollowTr != null) {
                if (snap) {
                    _lookAt = Current.FollowTr.position;
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
                return 0; // no hit!
            }
            return 0;
        }

        private void UpdateCamera() {
            Quaternion rotation = Quaternion.Euler(_currTilt, _currRotation, 0);
            var v = new Vector3(0.0f, 0.0f, -_currDistance * (Cam.orthographic ? Config.OrthoMulti : 1 ));
            Vector3 position = rotation * v + _target.transform.position;

            if (Cam.orthographic) {
                Cam.orthographicSize = _currDistance * Config.OrthoMulti;
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
            if (Current.FollowTr == null) {
                return true; // our distance is technically zero
            }
            Vector3 p1 = _target.transform.position;
            Vector3 p2 = FollowTarget.position;
            p1.y = p2.y = 0; // ignore height offset
            Vector3 v = p1 - p2;
            float vd = v.sqrMagnitude; // use sqr for performance

            return vd < sqrDistance;
        }

        private void EnsureTargetIsVisible() {
            Vector3 direction = (Current.Tr.position - _target.transform.position);
            direction.Normalize();
            float distance = _distance;
            RaycastHit hitInfo;
            //if (Physics.Raycast(_target.transform.position, direction, out hitInfo, distance, ~TargetVisibilityIgnoreLayerMask))
            if (Physics.SphereCast(_target.transform.position, Config.CameraRadius, direction, out hitInfo, distance,
                ~ Config.TargetVisibilityIgnoreLayerMask)) {
                if (hitInfo.transform != _target) // don't collide with outself!
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
            _distance -= scroll * Config.ZoomSpeed * TimeManager.DeltaUnscaled;
            if (rotate) {
                float tilt = move.y;
                _tilt -= tilt * _tiltSpeed * TimeManager.DeltaUnscaled;

                float rot = move.x;
                _rotation += rot * _rotateSpeed * TimeManager.DeltaUnscaled;
            }
            else if (Config.MoveCamera) {
                float speed = Config.MoveSpeed;
                if (PlayerInputSystem.GetKeyDown(Config.FastMoveKeyCode1)) {
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

            _distance -= scroll * Config.ZoomSpeed * TimeManager.DeltaUnscaled;
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
            if (PlayerInputSystem.GetKeyDown(Config.FastMoveKeyCode1)) {
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
    }
}
