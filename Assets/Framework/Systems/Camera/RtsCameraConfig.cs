using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;

namespace PixelComrades {
    public class RtsCameraConfig : ScriptableObject {
        [SerializeField] private float _lookAtHeightOffset = 1;// Y coordinate of camera target.   Only used if TerrainHeightViaPhysics and GetTerrainHeight are not set.
        [SerializeField] private FloatRange _tiltRange = new FloatRange(-360, 360);
        [SerializeField] private FloatRange _distanceRange = new FloatRange(50, 500);
        [SerializeField] private FloatRange _orthographicDistance = new FloatRange(1, 50);
        [SerializeField, Range(0,1)] private float _startingDistance = 0.25f;
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
        [SerializeField] private float _moveSpeed = 50;
        [SerializeField] private float _rotateSpeed = 50f;
        [SerializeField] private float _tiltSpeed = 50f;
        [SerializeField] private float _zoomSpeed = 0.7f;
        [SerializeField] private float _fastMoveSpeed = 300f;
        [SerializeField] private Key _fastMoveKeyCode = Key.LeftShift;
        [SerializeField] private Key _breakFollowKey = Key.Escape;
        [SerializeField] private bool _limitPos = true;
        [SerializeField] private bool _autoInput = false;
        [SerializeField] private bool _allowScreenEdgeMove = true;
        [SerializeField] private bool _screenEdgeMoveBreaksFollow = true;
        [SerializeField] private int _screenEdgeBorderWidth = 4;
        
        public bool AllowScreenEdgeMove { get => _allowScreenEdgeMove; }
        public bool ScreenEdgeMoveBreaksFollow { get => _screenEdgeMoveBreaksFollow; }
        public int ScreenEdgeBorderWidth { get => _screenEdgeBorderWidth; }
        public float StartingDistance { get => _startingDistance; }
        public float LookAtHeightOffset { get => _lookAtHeightOffset; }
        public FloatRange TiltRange { get => _tiltRange; }
        public FloatRange DistanceRange { get => _distanceRange; }
        public bool Smoothing { get => _smoothing; }
        public float MoveDampening { get => _moveDampening; }
        public float RotationDampening { get => _rotationDampening; }
        public float TiltDampening { get => _tiltDampening; }
        public float ZoomDampening { get => _zoomDampening; }
        public bool UseVisibilityCheck { get => _useVisibilityCheck; }
        public bool ShowDebugCameraTarget { get => _showDebugCameraTarget; }
        public float CameraRadius { get => _cameraRadius; }
        public bool TargetVisbilityViaPhysics { get => _targetVisbilityViaPhysics; }
        public LayerMask TargetVisibilityIgnoreLayerMask { get => _targetVisibilityIgnoreLayerMask; }
        public bool TerrainHeightViaPhysics { get => _terrainHeightViaPhysics; }
        public LayerMask TerrainPhysicsLayerMask { get => _terrainPhysicsLayerMask; }
        public bool FollowBehind { get => _followBehind; }
        public float FollowRotationOffset { get => _followRotationOffset; }
        public bool MoveCamera { get => _moveCamera; }
        public int MouseOrbitButton { get => _mouseOrbitButton; }
        public FloatRange OrthoDistance { get => _orthographicDistance; }
        public float MoveSpeed { get => _moveSpeed; }
        public float RotateSpeed { get => _rotateSpeed; }
        public float TiltSpeed { get => _tiltSpeed; }
        public float ZoomSpeed { get => _zoomSpeed; }
        public float FastMoveSpeed { get => _fastMoveSpeed; }
        public Key FastMoveKeyCode { get => _fastMoveKeyCode; }
        public Key BreakFollowKey { get => _breakFollowKey; }
        public bool LimitPos { get => _limitPos; }
        public bool AutoInput { get => _autoInput; }

        [Button]
        public void UpdateConfig() {
            RtsCameraSystem.Get.UpdateConfig();
        }
    }
}
