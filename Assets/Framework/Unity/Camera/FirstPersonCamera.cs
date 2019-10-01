using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class FirstPersonCamera : MonoSingleton<FirstPersonCamera> {

        [System.Serializable]
        public class SpringEventConfig {
            [ValueDropdown("SignalsList")] public string AnimationEvent;
            public Vector3 Force;
            public int Frames;
            public SpringType Type;

            public SpringEventConfig() { }

            public SpringEventConfig(string animationEvent, Vector3 force, int frames, SpringType type) {
                AnimationEvent = animationEvent;
                Force = force;
                Frames = frames;
                Type = type;
            }

            public enum SpringType {
                Position,
                Rotation,
                Fov
            }

            private ValueDropdownList<string> SignalsList() {
                return AnimationEvents.GetDropdownList();
            }

            public void Trigger() {
                switch (Type) {
                    case SpringType.Fov:
                        ZoomForce(Force.AbsMax(), Frames);
                        break;
                    case SpringType.Rotation:
                        AddRotationForce(Force, Frames);
                        break;
                    case SpringType.Position:
                        AddForce(Force, Frames);
                        break;
                }
            }
        }

        private static Vector3 _topDirection = new Vector3(-1,0,0);
        private static Vector3 _bottomDirection = new Vector3(1, 0, 0);
        private static Vector3 _rightDirection = new Vector3(0, 1, 0);
        private static Vector3 _leftDirection = new Vector3(0, -1, 0);

        [SerializeField] private float _positionSpeed = 50;
        [Range(0,5), SerializeField] private float _sensitivity = 0.7f;
        [SerializeField] private float _maxLookAngleY = 65f;
        [SerializeField, Range(10f, 50f)] private float _zTargetSpeed = 20f;
        [SerializeField] private Transform _followTr = null;
        [SerializeField] private Camera _cam = null;
        [SerializeField] private Spring _moveSpring = new Spring();
        [SerializeField] private Spring _rotationSpring = new Spring();
        [SerializeField] private Spring _fovSpring = new Spring();
        [SerializeField] private int _shakeFrames = 4;
        [SerializeField] private int _shakeStrength = 25;
        [SerializeField] private int _pullFrames = 32;
        [SerializeField] private int _pullStrength = 50;

        
        private float _rotationX, _rotationY;
        private Transform _camTr;
        private float _originalFov = 55;
        private Dictionary<string, SpringEventConfig> _eventDict = new Dictionary<string, SpringEventConfig>();

        private static Vector3 _center = new Vector3(0.5f, 0.5f, 0);
        private static GameOptions.CachedFloat _lookSmooth = new GameOptions.CachedFloat("LookSmooth");

        public bool Unscaled { get { return true; } }

        void Awake() {
            _camTr = transform;
            _rotationX = _rotationY = 0;
            //_nativeRotation = _camTr.localRotation;
            //_nativeRotation.eulerAngles = Vector3.up * _camTr.localEulerAngles.y;
            MessageKit<float>.addObserver(Messages.PlayerViewRotated, ChangeRotation);
            MessageKit.addObserver(Messages.PlayerTeleported, ForceMoveUpdate);
            InitSprings();
            BuildEventDictionary();
        }

        private void BuildEventDictionary() {
            _eventDict.Clear();
            _eventDict.Add(AnimationEvents.ShakeRightTop, new SpringEventConfig(AnimationEvents.ShakeRightTop, 
                (_topDirection + _rightDirection) * _shakeStrength,_shakeFrames, SpringEventConfig.SpringType.Rotation));
            _eventDict.Add(AnimationEvents.PullRightTop, new SpringEventConfig(AnimationEvents.PullRightTop,
                    (_topDirection + _rightDirection) * _pullStrength, _pullFrames, SpringEventConfig.SpringType.Rotation));

            _eventDict.Add(AnimationEvents.ShakeRightMiddle, new SpringEventConfig(AnimationEvents.ShakeRightMiddle, 
                (_rightDirection) * _shakeStrength,_shakeFrames, SpringEventConfig.SpringType.Rotation));
            _eventDict.Add(AnimationEvents.PullRightMiddle, new SpringEventConfig(AnimationEvents.PullRightMiddle,
                    (_rightDirection) * _pullStrength, _pullFrames, SpringEventConfig.SpringType.Rotation));

            _eventDict.Add(AnimationEvents.ShakeRightBottom, new SpringEventConfig(AnimationEvents.ShakeRightBottom, 
                (_bottomDirection + _rightDirection) * _shakeStrength,_shakeFrames, SpringEventConfig.SpringType.Rotation));
            _eventDict.Add(AnimationEvents.PullRightBottom, new SpringEventConfig(AnimationEvents.PullRightBottom,
                    (_bottomDirection + _rightDirection) * _pullStrength, _pullFrames, SpringEventConfig.SpringType.Rotation));

            _eventDict.Add(AnimationEvents.ShakeTop, new SpringEventConfig(AnimationEvents.ShakeTop, 
                (_topDirection) * _shakeStrength,_shakeFrames, SpringEventConfig.SpringType.Rotation));
            _eventDict.Add(AnimationEvents.PullTop, new SpringEventConfig(AnimationEvents.PullTop,
                    (_topDirection) * _pullStrength, _pullFrames, SpringEventConfig.SpringType.Rotation));

            _eventDict.Add(AnimationEvents.ShakeBottom, new SpringEventConfig(AnimationEvents.ShakeBottom, 
                (_bottomDirection) * _shakeStrength,_shakeFrames, SpringEventConfig.SpringType.Rotation));
            _eventDict.Add(AnimationEvents.PullBottom, new SpringEventConfig(AnimationEvents.PullBottom,
                    (_bottomDirection) * _pullStrength, _pullFrames, SpringEventConfig.SpringType.Rotation));

            _eventDict.Add(AnimationEvents.ShakeLeftTop, new SpringEventConfig(AnimationEvents.ShakeLeftTop, 
                (_topDirection + _leftDirection) * _shakeStrength,_shakeFrames, SpringEventConfig.SpringType.Rotation));
            _eventDict.Add(AnimationEvents.PullLeftTop, new SpringEventConfig(AnimationEvents.PullLeftTop,
                    (_topDirection + _leftDirection) * _pullStrength, _pullFrames, SpringEventConfig.SpringType.Rotation));

            _eventDict.Add(AnimationEvents.ShakeLeftMiddle, new SpringEventConfig(AnimationEvents.ShakeLeftMiddle, 
                (_leftDirection) * _shakeStrength,_shakeFrames, SpringEventConfig.SpringType.Rotation));
            _eventDict.Add(AnimationEvents.PullLeftMiddle, new SpringEventConfig(AnimationEvents.PullLeftMiddle,
                    (_leftDirection) * _pullStrength, _pullFrames, SpringEventConfig.SpringType.Rotation));

            _eventDict.Add(AnimationEvents.ShakeLeftBottom, new SpringEventConfig(AnimationEvents.ShakeLeftBottom, 
                (_bottomDirection + _leftDirection) * _shakeStrength,_shakeFrames, SpringEventConfig.SpringType.Rotation));
            _eventDict.Add(AnimationEvents.PullLeftBottom, new SpringEventConfig(AnimationEvents.PullLeftBottom,
                    (_bottomDirection + _leftDirection) * _pullStrength, _pullFrames, SpringEventConfig.SpringType.Rotation));
        }

        private SpringEventConfig GetConfig(string animationEvent) {
            return _eventDict.TryGetValue(animationEvent, out var config) ? config : null;
        }

        public static void PlaySpringAnimation(string animationEvent) {
            var config = main.GetConfig(animationEvent);
            if (config != null) {
                config.Trigger();
            }
        }

        public static void Jumped() {
            main.GetConfig(AnimationEvents.PullTop).Trigger();
        }

        public static void Landed() {
            main.GetConfig(AnimationEvents.ShakeBottom).Trigger();
        }

        [ValueDropdown("SignalsList")] [SerializeField] private string _testAnimation = "";
        [SerializeField, TextArea] private string _testScripting = "";

        private ValueDropdownList<string> SignalsList() {
            return AnimationEvents.GetDropdownList();
        }
        
        [Button]
        public void TestScripting() {
            TimeManager.StartUnscaled(ScriptingTest());
        }

        [Button]
        public void TestEvent() {
            InitSprings();
            BuildEventDictionary();
            var config = GetConfig(_testAnimation);
            if (config != null) {
                config.Trigger();
                TimeManager.StartUnscaled(UpdateSprings(2));
            }
        }

        private IEnumerator ScriptingTest() {
            InitSprings();
            TimeManager.StartUnscaled(UpdateSprings(1));
            var lines = _testScripting.SplitIntoLines();
            for (int i = 0; i < lines.Length; i++) {
                var words = lines[i].SplitIntoWords();
                if (words.Length < 1) {
                    continue;
                }
                var param0 = words.Length < 2 ? "" : words[1];
                var param1 = words.Length < 3 ? "" : words[2];
                switch (words[0].ToLower()) {
                    case "wait":
                        if (words.Length == 1) {
                            yield return null;
                            continue;
                        }
                        if (float.TryParse(param0, out var time)) {
                            yield return time;
                        }
                        if (param0.ToLower() == "springs") {
                            bool allResting = true;
                            while (allResting) {
                                allResting = _moveSpring.Resting && _rotationSpring.Resting && _fovSpring.Resting;
                                yield return null;
                            }
                        }
                        break;
                    case "fov":
                    case "zoom":
                        ZoomForce(ParseUtilities.TryParse(param0, 5f), ParseUtilities.TryParse(param1, 4));
                        break;
                    case "pitch":
                    case "kick":
                        AddForce(ParseUtilities.TryParse(param0, Vector3.up), ParseUtilities.TryParse(param1, 4));
                        break;
                    case "shake":
                    case "rotate":
                        AddRotationForce(ParseUtilities.TryParse(param0, Vector3.up), ParseUtilities.TryParse(param1, 4));
                        break;
                }
            }
            yield return null;
        }

        private void InitSprings() {
            _fovSpring.Initialize(false);
            _fovSpring.Reset();
            _moveSpring.Initialize(false);
            _moveSpring.Reset();
            _rotationSpring.Initialize(true);
            _rotationSpring.Reset();
        }


        private IEnumerator UpdateSprings(float minTime) {
            var endTime = TimeManager.TimeUnscaled + minTime;
            bool allResting = true;
            while (allResting) {
                UpdateSprings();
                if (!_fovSpring.Resting) {
                    _cam.fieldOfView = _originalFov + _fovSpring.Value.z;
                }
                if (!_moveSpring.Resting) {
                    transform.position = TransformPoint(_followTr.position, transform.rotation, _moveSpring.Value);
                }
                if (!_rotationSpring.Resting) {
                    transform.rotation = TransformQuaternion(_followTr.rotation, Quaternion.Euler(_rotationSpring.Value));
                }
                if (TimeManager.Time > endTime) {
                    allResting = _moveSpring.Resting && _rotationSpring.Resting && _fovSpring.Resting;
                }
                yield return null;
            }
        }

        public static void ZoomForce(float force, int frames = 4) {
            main._fovSpring.AddForce(new Vector3(0,0, force), frames);
        }

        public static void AddForce(Vector3 force, int frames = 4) {
            main._moveSpring.AddForce(force, frames);
        }

        public static void AddRotationForce(Vector3 force, int frames = 4) {
            main._rotationSpring.AddForce(force, frames);
        }

        public static void AddForce(Vector3 force, bool isRotation, int frames = 4) {
            if (isRotation) {
                main._rotationSpring.AddForce(force, frames);
            }
            else {
                main._moveSpring.AddForce(force, frames);
            }
        }

        private void UpdateSprings() {
            _fovSpring.UpdateSpring();
            _moveSpring.UpdateSpring();
            _rotationSpring.UpdateSpring();
        }

        public static Vector3 TransformPoint(Vector3 worldPosition, Quaternion rotation, Vector3 localPosition) {
            return worldPosition + (rotation * localPosition);
        }

        public static Quaternion TransformQuaternion(Quaternion worldRotation, Quaternion rotation) {
            return worldRotation * rotation;
        }

        public void ChangeRotation(float yRot) {
            _camTr.rotation = Quaternion.Euler(0, yRot, 0);
            _rotationX = yRot;
            //_camTr.localRotation = _cameraTargetRotation = Quaternion.identity;
        }

        private void ForceMoveUpdate() {
            _camTr.position = _followTr.position;
        }

        void LateUpdate() {
            CameraLook();
        }

        private void FixedUpdate() {
            UpdateSprings();
            var targetPos = Vector3.MoveTowards(_camTr.position, _followTr.position , _positionSpeed * TimeManager.FixedDeltaUnscaled);
            _camTr.position = TransformPoint(targetPos, _camTr.rotation, _moveSpring.Value);
            _cam.fieldOfView = _originalFov + _fovSpring.Value.z;
        }

        public void CameraLook() {
            _rotationX += PlayerInputSystem.LookInput.x * _sensitivity;
            _rotationY += PlayerInputSystem.LookInput.y * _sensitivity;
            if (UICenterTarget.LockedActor != null) {
                var targetPos = UICenterTarget.LockedActor.Tr.position + Vector3.up;// + UICenterTarget.LockedActor.LocalCenter;
                var targetScreen = Player.Cam.WorldToViewportPoint(targetPos);
                var targetDir = (targetScreen - _center);
                _rotationX += (targetDir.x * _zTargetSpeed);
                _rotationY += (targetDir.y * _zTargetSpeed);
            }
            _rotationY = Mathf.Clamp(_rotationY, -_maxLookAngleY, _maxLookAngleY);
            //if (float.IsNaN(_rotationX)) {
            //    _rotationX = 0;
            //}
            //if (float.IsNaN(_rotationY)) {
            //    _rotationY = 0;
            //}
            //float xTilt = (_useHeadBob ? CameraHeadBob.XTilt * _tiltForce : 0f);
            //float yTilt = (_useHeadBob ? CameraHeadBob.YTilt * _tiltForce : 0f);
            //Quaternion camTargetRotation = Quaternion.Euler(-1f * _rotationY + (_useHeadBob ? CameraHeadBob.XTilt * _tiltForce : 0f), 0f, 0f);
            //Quaternion bodyTargetRotation = _nativeRotation * Quaternion.Euler(0f, _rotationX + (_useHeadBob ? CameraHeadBob.YTilt * _tiltForce : 0f), 0f);
            //Quaternion camTargetRotation = Quaternion.Euler(-1f * _rotationY + xTilt, 0f, 0f);
            //Quaternion bodyTargetRotation = Quaternion.Euler(0f, _rotationX + yTilt, 0f);
            var targetRotation = Quaternion.Euler(-1f * _rotationY , _rotationX, 0);
            //float smoothRotation = _lookSmooth * (TimeManager.DeltaUnscaled * 50f);
            //_camTr.localRotation = Quaternion.Slerp(_camTr.localRotation, targetRotation, smoothRotation);
            //_bodyTr.localRotation = Quaternion.Slerp(_bodyTr.localRotation,bodyTargetRotation, smoothRotation);
            _camTr.localRotation = targetRotation;
            _camTr.rotation = TransformQuaternion(_camTr.rotation, Quaternion.Euler(_rotationSpring.Value));
        }
    }
}