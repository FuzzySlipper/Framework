using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class FirstPersonCamera : MonoSingleton<FirstPersonCamera>, ISystemUpdate, IOnCreate {

        public static Transform Tr { get; private set; }
        public static Vector2 LookInput { get; set; }
        public static Transform HoldTr { get { return main._holdTransform; } }
        public static Vector3 HoldStartPosition { get; private set; }

        private static Vector3 _center = new Vector3(0.5f, 0.5f, 0);

        [SerializeField] private float _maxLookAngleY = 65f;
        [SerializeField] private float _posForce = .65f;
        [SerializeField] private float _tiltForce = .85f;
        [SerializeField] private Transform _holdTransform = null;
        [SerializeField, Range(10f, 50f)] private float _zTargetSpeed = 20f;
        //[SerializeField] private FirstPersonBodySway _sway = null;

        private Vector3 _cameraOffset = Vector3.up;
        private Quaternion _nativeRotation;
        private float _rotationX, _rotationY;
        private float _lookSmooth;

        public bool Unscaled { get { return true; } }

        public void OnCreate(PrefabEntity entity) {
            Tr = transform;
            _cameraOffset = Tr.localPosition;
            _nativeRotation = Tr.localRotation;
            _nativeRotation.eulerAngles = Vector3.up * Tr.localEulerAngles.y;
            HoldStartPosition = _holdTransform.localPosition;
            MessageKit.addObserver(Messages.OptionsChanged, SetOptions);
        }

        private void SetOptions() {
            _lookSmooth = GameOptions.Get("LookSmooth", 0.75f);
        }

        public void OnSystemUpdate(float delta) {
            CameraLook();
            //_sway.Sway();
        }

        public void CameraLook() {
            //_rotationX += LookInput.x * Time.timeScale;
            //_rotationY += LookInput.y * Time.timeScale;
            _rotationX += LookInput.x;
            _rotationY += LookInput.y;
            if (UICenterTarget.LockedActor != null) {
                var targetPos = UICenterTarget.LockedActor.Entity.Tr.position;// + UICenterTarget.LockedActor.LocalCenter;
                var targetScreen = Player.Cam.WorldToViewportPoint(targetPos);
                var targetDir = (targetScreen - _center);
                _rotationX += (targetDir.x * _zTargetSpeed);
                _rotationY += (targetDir.y * _zTargetSpeed);
            }
            _rotationY = Mathf.Clamp(_rotationY, -_maxLookAngleY, _maxLookAngleY);

            Quaternion camTargetRotation = _nativeRotation * Quaternion.Euler(-1f * _rotationY + (GameOptions.UseHeadBob ? CameraHeadBob.XTilt * _tiltForce : 0f), 0f, 0f);
            Quaternion bodyTargetRotation = _nativeRotation * Quaternion.Euler(0f, _rotationX + (GameOptions.UseHeadBob ? CameraHeadBob.YTilt * _tiltForce : 0f), 0f);

            float smoothRotation = _lookSmooth * (TimeManager.DeltaUnscaled * 50f);
            Tr.localRotation = Quaternion.Slerp(Tr.localRotation, camTargetRotation, smoothRotation);
            FirstPersonController.Tr.localRotation = Quaternion.Slerp(
                FirstPersonController.Tr.localRotation,
                bodyTargetRotation, smoothRotation);

            Vector3 newCameraPosition = Vector3.zero;
            newCameraPosition.x = _cameraOffset.x + (GameOptions.UseHeadBob ? CameraHeadBob.XPos * _posForce : 0f);
            newCameraPosition.y = _cameraOffset.y + (GameOptions.UseHeadBob ? CameraHeadBob.YPos * _posForce : 0f);
            newCameraPosition.z = _cameraOffset.z;
            Tr.localPosition = newCameraPosition;
        }


    }
}