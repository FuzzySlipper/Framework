using UnityEngine;

namespace PixelComrades {
    public class CameraHeadBob : MonoBehaviour, ISystemFixedUpdate {

        [Range(1f, 3f)] [SerializeField] private float _headBobFrequency = 1.5f;
        [Range(.1f, 2f)] [SerializeField] private float _headBobHeight = .35f;
        [Range(.01f, .1f)] [SerializeField] private float _headBobSideMovement = .075f;
        [Range(.1f, 2f)] [SerializeField] private float _bobHeightSpeedMultiplier = .35f;
        [Range(.1f, 2f)] [SerializeField] private float _bobStrideSpeedLengthen = .35f;
        [Range(.1f, 2f)] [SerializeField] private float _headBobSwayAngle = .5f;
        [Range(.1f, 5f)] [SerializeField] private float _jumpLandMove = 2f;
        [Range(10f, 100f)] [SerializeField] private float _jumpLandTilt = 35f;
        [Range(.1f, 4f)] [SerializeField] private float _springElastic = 1.25f;
        [Range(.1f, 2f)] [SerializeField] private float _springDampen = .77f;


        private Transform _tr;
        private float _springPos, _springVelocity, _headBobFade;
        private Vector3 _velocity, _velocityChange, _prevPosition, _prevVelocity;

        public static float HeadBobCycle { get; private set; }
        public static float XPos { get; private set; }
        public static float YPos { get; private set; }
        public static float XTilt { get; private set; }
        public static float YTilt { get; private set; }

        private void Awake() {
            _tr = transform;
            HeadBobCycle = 0f;
            XPos = YPos = 0f;
            XTilt = YTilt = 0f;
        }

        public void OnFixedSystemUpdate(float delta) {
            _velocity = (_tr.position - _prevPosition) / Time.fixedDeltaTime;
            _velocityChange = _velocity - _prevVelocity;
            _prevPosition = _tr.position;
            _prevVelocity = _velocity;

            if (!FirstPersonController.Climbing) {
                _velocity.y = 0f;
            }

            _springVelocity -= _velocityChange.y;
            _springVelocity -= _springPos * _springElastic;
            _springVelocity *= _springDampen;
            _springPos += _springVelocity * Time.fixedDeltaTime;
            _springPos = Mathf.Clamp(_springPos, -.32f, .32f);

            if (Mathf.Abs(_springVelocity) < .05f && Mathf.Abs(_springPos) < .05f) {
                _springVelocity = _springPos = 0f;
            }

            var flatVelocity = _velocity.magnitude;

            if (FirstPersonController.Climbing) {
                flatVelocity *= 4f;
            }
            else if (!FirstPersonController.Climbing && !FirstPersonController.Grounded) {
                flatVelocity /= 4f;
            }

            var strideLengthen = 1f + flatVelocity * _bobStrideSpeedLengthen;
            HeadBobCycle += flatVelocity / strideLengthen * (Time.fixedDeltaTime / _headBobFrequency);

            var bobFactor = Mathf.Sin(HeadBobCycle * Mathf.PI * 2f);
            var bobSwayFactor = Mathf.Sin(HeadBobCycle * Mathf.PI * 2f + Mathf.PI * .5f);
            bobFactor = 1f - (bobFactor * .5f + 1f);
            bobFactor *= bobFactor;

            if (_velocity.magnitude < .1f) {
                _headBobFade = Mathf.Lerp(_headBobFade, 0f, Time.fixedDeltaTime);
            }
            else {
                _headBobFade = Mathf.Lerp(_headBobFade, 1f, Time.fixedDeltaTime);
            }

            var speedHeightFactor = 1f + flatVelocity * _bobHeightSpeedMultiplier;

            XPos = -_headBobSideMovement * bobSwayFactor * _headBobFade;
            YPos = _springPos * _jumpLandMove + bobFactor * _headBobHeight * _headBobFade * speedHeightFactor;
            XTilt = _springPos * _jumpLandTilt;
            YTilt = bobSwayFactor * _headBobSwayAngle * _headBobFade;
        }
    }
}