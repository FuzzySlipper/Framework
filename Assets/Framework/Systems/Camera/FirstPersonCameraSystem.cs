using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    [Priority(Priority.Highest)]
    public sealed class FirstPersonCameraSystem : SystemBase<FirstPersonCameraSystem>, IMainLateUpdate, IMainFixedUpdate {

        private GameOptions.CachedFloat _positionSpeed = new GameOptions.CachedFloat("CameraPositionSpeed");
        private GameOptions.CachedFloat _sensitivity = new GameOptions.CachedFloat("CameraSensitivity");
        private GameOptions.CachedFloat _maxLookAngleY = new GameOptions.CachedFloat("CameraMaxLookAngleY");
        private GameOptions.CachedFloat _zTargetSpeed = new GameOptions.CachedFloat("CameraZTargetSpeed");
        private PlayerCameraComponent _singleton;

        private static Vector3 _center = new Vector3(0.5f, 0.5f, 0);
        private static GameOptions.CachedFloat _lookSmooth = new GameOptions.CachedFloat("LookSmooth");

        public FirstPersonCameraSystem() {
            MessageKit<float>.addObserver(Messages.PlayerViewRotated, ChangeRotation);
            MessageKit.addObserver(Messages.PlayerTeleported, ForceMoveUpdate);
        }
        
        public void OnSystemLateUpdate(float dt, float unscaledDt) {
            if (_singleton == null || !_singleton.Active) {
                return;
            }
            _singleton.RotationX += PlayerInputSystem.LookInput.x * _sensitivity;
            _singleton.RotationY += PlayerInputSystem.LookInput.y * _sensitivity;
            if (UICenterTarget.LockedActor != null) {
                var targetPos = UICenterTarget.LockedActor.Tr.position + Vector3.up; // + UICenterTarget.LockedActor.LocalCenter;
                var targetScreen = Player.Cam.WorldToViewportPoint(targetPos);
                var targetDir = (targetScreen - _center);
                _singleton.RotationX += (targetDir.x * _zTargetSpeed);
                _singleton.RotationY += (targetDir.y * _zTargetSpeed);
            }
            _singleton.RotationY = Mathf.Clamp(_singleton.RotationY, -_maxLookAngleY, _maxLookAngleY);
            var targetRotation = Quaternion.Euler(-1f * _singleton.RotationY, _singleton.RotationX, 0);
            _singleton.CamTr.localRotation = targetRotation;
            _singleton.CamTr.rotation = TransformQuaternion(_singleton.CamTr.rotation, Quaternion.Euler(_singleton.RotationSpring
            .Value));
        }

        public void OnFixedSystemUpdate(float dt) {
            if (_singleton == null || !_singleton.Active) {
                return;
            }
            UpdateSprings();
            var targetPos = Vector3.MoveTowards(_singleton.CamTr.position, _singleton.FollowTr.position, _positionSpeed * TimeManager.FixedDeltaUnscaled);
            _singleton.CamTr.position = TransformPoint(targetPos, _singleton.CamTr.rotation, _singleton.MoveSpring.Value);
            _singleton.Cam.fieldOfView = _singleton.OriginalFov + _singleton.FovSpring.Value.z;
        }

        private void UpdateSprings() {
            _singleton.FovSpring.UpdateSpring();
            _singleton.MoveSpring.UpdateSpring();
            _singleton.RotationSpring.UpdateSpring();
        }

        public void ChangeRotation(float yRot) {
            _singleton.CamTr.rotation = Quaternion.Euler(0, yRot, 0);
            _singleton.RotationX = yRot;
            //_camTr.localRotation = _cameraTargetRotation = Quaternion.identity;
        }

        private void ForceMoveUpdate() {
            _singleton.CamTr.position = _singleton.FollowTr.position;
        }

        public static void Set(PlayerCameraComponent component) {
            Get._singleton = component;
        }

        public static Vector3 TransformPoint(Vector3 worldPosition, Quaternion rotation, Vector3 localPosition) {
            return worldPosition + (rotation * localPosition);
        }

        public static Quaternion TransformQuaternion(Quaternion worldRotation, Quaternion rotation) {
            return worldRotation * rotation;
        }
    }
}
