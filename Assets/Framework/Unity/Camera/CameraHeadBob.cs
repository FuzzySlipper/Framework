using System;
using UnityEngine;

namespace PixelComrades {
    public class CameraHeadBob : MonoBehaviour, ISystemUpdate {

        [Range(1f, 3f)] [SerializeField] private float _headBobFrequency = 1.5f;
        [Range(.1f, 2f)] [SerializeField] private float _bobStrideSpeedLengthen = .35f;
        [Range(.1f, 2f)] [SerializeField] private float _headBobSwayAngle = .5f;
        [Range(.1f, 5f)] [SerializeField] private float _jumpLandMove = 2f;
        [Range(10f, 100f)] [SerializeField] private float _jumpLandTilt = 35f;
        [Range(.1f, 4f)] [SerializeField] private float _springElastic = 1.25f;
        [Range(.1f, 2f)] [SerializeField] private float _springDampen = .77f;
        [Header("Position")]
        [Range(-1, 1f)] [SerializeField] private float _headBobHeight = .35f;
        [Range(0, 0.5f)] [SerializeField] private float _headBobSideMovement = .075f;
        [Range(.1f, 2f)] [SerializeField] private float _bobHeightSpeedMultiplier = .35f;

        private Transform _tr;
        private float _springPos, _springVelocity, _headBobFade;
        private Vector3 _velocity, _velocityChange, _prevPosition, _prevVelocity;

        public static float HeadBobCycle { get; private set; }
        public static float XPos { get; private set; }
        public static float YPos { get; private set; }
        public static float XTilt { get; private set; }
        public static float YTilt { get; private set; }
        public bool Unscaled { get { return false; } }

        private void Awake() {
            _tr = transform;
            ResetValues();
            MessageKit.addObserver(Messages.PlayerTeleported, ResetValues);
        }

        private void ResetValues() {
            _velocity = _velocityChange = _prevVelocity = Vector3.zero;
            XPos = YPos = HeadBobCycle = XTilt = YTilt = _springPos = _springVelocity = _headBobFade = 0f;
            _prevPosition = _tr.position;
        }

        public void OnSystemUpdate(float delta) {
            if (Math.Abs(delta) < 0.00001f) {
                return;
            }
            _velocity = (_tr.position - _prevPosition) / delta;
            _velocityChange = _velocity - _prevVelocity;
            _prevPosition = _tr.position;
            _prevVelocity = _velocity;
            if (Player.FirstPersonController.CurrentMovement != FPMovementAction.Climbing) {
                _velocity.y = 0f;
            }
            if (float.IsNaN(_springVelocity)) {
                ResetValues();
            }
            _springVelocity -= _velocityChange.y;
            _springVelocity -= _springPos * _springElastic;
            _springVelocity *= _springDampen;
            _springPos += _springVelocity * delta;
            _springPos = Mathf.Clamp(_springPos, -.32f, .32f);

            if (Mathf.Abs(_springVelocity) < .05f && Mathf.Abs(_springPos) < .05f) {
                _springVelocity = _springPos = 0f;
            }

            var flatVelocity = _velocity.magnitude;

            if (Player.FirstPersonController.CurrentMovement == FPMovementAction.Climbing) {
                flatVelocity *= 4f;
            }
            else if (Player.FirstPersonController.CurrentMovement != FPMovementAction.Climbing && !Player.FirstPersonController.Grounded) {
                flatVelocity /= 4f;
            }

            var strideLengthen = 1f + flatVelocity * _bobStrideSpeedLengthen;
            HeadBobCycle += flatVelocity / strideLengthen * (delta / _headBobFrequency);

            var bobSwayFactor = Mathf.Sin(HeadBobCycle * Mathf.PI * 2f + Mathf.PI * .5f);
            var bobFactor = Mathf.Sin(HeadBobCycle * Mathf.PI * 2f);
            bobFactor = 1f - (bobFactor * .5f + 1f);
            bobFactor *= bobFactor;
            _headBobFade = Mathf.Lerp(_headBobFade, _velocity.magnitude < .1f ? 0f : 1f, delta);
            var speedHeightFactor = 1f + flatVelocity * _bobHeightSpeedMultiplier;

            XPos = -_headBobSideMovement * bobSwayFactor * _headBobFade;
            YPos = _springPos * _jumpLandMove + bobFactor * _headBobHeight * _headBobFade * speedHeightFactor;
            XTilt = _springPos * _jumpLandTilt;
            YTilt = bobSwayFactor * _headBobSwayAngle * _headBobFade;
        }
    }
}