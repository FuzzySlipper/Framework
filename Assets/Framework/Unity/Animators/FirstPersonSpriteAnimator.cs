using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class FirstPersonSpriteAnimator : MonoBehaviour, ISystemUpdate, IOnCreate {

        [SerializeField] private SpriteRenderer _renderer = null;
        [SerializeField] private Transform _pivot = null;
        [Range(0, 1f)] [SerializeField] private float _verticalSwayAmount = 0.2f;
        [Range(0, 1f)] [SerializeField] private float _horizontalSwayAmount = 0.4f;
        [Range(0, 15f)] [SerializeField] private float _swaySpeed = 4f;
        [SerializeField] private float _lowerPosition = -12;
        [SerializeField] private float _moveDuration = 0.3f;
        [SerializeField] private EasingTypes _lowerEasing = EasingTypes.SinusoidalOut;
        [SerializeField] private EasingTypes _raiseEasing = EasingTypes.ElasticOut;

        private TweenV3 _positionAnimator;
        private ActionConfig _current;
        private SpriteAnimation _currentAnim = null;
        private float _timer;
        private Vector3 _resetPoint;
        private MaterialPropertyBlock _materialBlock;
        private Queue<SpriteAnimation> _animationQueue = new Queue<SpriteAnimation>();
        private GameOptions.CachedBool _useWeaponBob = new GameOptions.CachedBool("UseWeaponBob");
        private int _currentFrameIndex = 0;
        private State _state = State.None;
        private Timer _frameTimer;
        private float _bobTime = 0;
        private AnimationFrame _eventFrame;

        public bool Unscaled { get { return false; } }
        public string CurrentAnimation { get; private set; }
        public float CurrentAnimationLength { get { return _currentAnim != null ? _currentAnim.LengthTime : 0f; } }
        public bool DefaultEventTriggered { get; private set; }
        public bool Finished { get; private set; }
        private AnimationFrame CurrentFrame { get { return _currentAnim != null ? _currentAnim.GetFrame(_currentFrameIndex) : null; } }
        public float CurrentAnimationRemaining { get { return _currentAnim != null ? (_currentAnim.LengthFrames - _currentFrameIndex) * _currentAnim.FrameTime : 0; } }
        public Vector3 GetEventPosition { get { return _currentAnim != null ? _currentAnim.GetEventPosition(_renderer, _eventFrame) : Vector3.zero; } }
        public Quaternion GetEventRotation {
            get {
                var target = GetMouseRaycastPosition();
                return Quaternion.LookRotation(target - GetEventPosition);
            }
        }

        public void OnCreate(PrefabEntity entity) {
            _materialBlock = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(_materialBlock);
            _resetPoint = _pivot.localPosition;
            _frameTimer = new Timer(1, Unscaled);
            _positionAnimator = new TweenV3();
        }

        public void OnSystemUpdate(float dt) {
            if (_useWeaponBob && _state == State.None) {
                _bobTime += dt;
                //var velocity = Mathf.Clamp01(Player.FirstPersonController.Velocity.magnitude);
                var velocity = Player.FirstPersonController.VelocityPercent;
                var y = _verticalSwayAmount * Mathf.Sin((_swaySpeed * 2) * _bobTime) * velocity;
                var x = _horizontalSwayAmount * Mathf.Sin(_swaySpeed * _bobTime) * velocity;
                _pivot.localPosition = _resetPoint + new Vector3(x, y, 0);
            }
            if (CheckFrameUpdate()) {
                UpdateSpriteFrame();
            }
        }

        public void ReloadWeapon() {
            if (_current == null || _state != State.None){
                return;
            }
            TimeManager.StartTask(LoadWeaponProcess(), false);
        }

        public void StopReload() {
            if (_state != State.Reloading) {
                return;
            }
            _state = State.None;
        }

        private void UpdateSpriteFrame() {
            var sprite = _currentAnim != null ? _currentAnim.GetSprite(_currentFrameIndex) : null;
            if (sprite == null) {
                return;
            }
            _renderer.sprite = sprite;
        }

        public void PlayAnimation(SpriteAnimation anim) {
            Play(anim);
        }

        public void ChangeAction(ActionConfig newActionConfig) {
            if (_current != null && newActionConfig != null) {
                TimeManager.StartTask(SwapWeapons(newActionConfig));
            }
            else if (_current != null && newActionConfig == null) {
                TimeManager.StartTask(LowerWeapon(true));
            }
            else {
                SetupUsable(newActionConfig);
                TimeManager.StartTask(RaiseWeapon());
            }
        }

        private void SetupUsable(ActionConfig usable) {
            //SetSprite(usable.FpSprite);
            _current = usable;
        }

        private void SetSprite(Sprite sprite) {
            _renderer.sprite = sprite;
            _renderer.enabled = true;
        }

        private void CheckQueue() {
            if (_animationQueue.Count > 0) {
                Play(_animationQueue.Dequeue());
            }
        }

        public bool IsAnimationComplete(string clip) {
            return Finished;
        }

        public bool IsAnimationEventComplete(string clip) {
            return DefaultEventTriggered;
        }

        public void PlayAnimation(string clip, bool overrideClip, ActionConfig actionConfig) {
            switch (clip) {
                case GraphNodeTags.Action:
                    if (_current != null) {
                        CurrentAnimation = clip;
                        //Play(_current);
                    }
                    break;
            }
        }

        public void Play(SpriteAnimation anim) {
            if (_state != State.None) {
                if (!_animationQueue.Contains(anim)) {
                    _animationQueue.Enqueue(anim);
                }
                return;
            }
            if (_currentAnim == anim && !Finished) {
                return;
            }
            _currentAnim = anim;
            _renderer.sprite = _currentAnim.GetSprite(0);
            if (_currentAnim.NormalMap != null) {
                _materialBlock.SetTexture("_BumpMap", _currentAnim.NormalMap);
            }
            _materialBlock.SetTexture("_MainTex", _renderer.sprite.texture);
            _renderer.SetPropertyBlock(_materialBlock);
            _currentFrameIndex = 0;
            _eventFrame = null;
            DefaultEventTriggered = Finished = false;
            _state = State.Playing;
            UpdateSpriteFrame();
        }

        private RaycastHit[] _hits = new RaycastHit[10];

        private Vector3 GetMouseRaycastPosition() {
            var ray = PlayerInputSystem.GetLookTargetRay;
            var cnt = Physics.RaycastNonAlloc(ray, _hits, 500, LayerMasks.DefaultCollision);
            _hits.SortByDistanceAsc(cnt);
            for (int i = 0; i < cnt; i++) {
                if (_hits[i].transform.CompareTag(StringConst.TagPlayer)) {
                    continue;
                }
                return _hits[i].point;
            }
            return ray.origin + (ray.direction * 500);
        }

        private bool CheckFrameUpdate() {
            if (_state != State.Playing || _frameTimer.IsActive || _currentAnim == null || Finished) {
                return false;
            }
            _currentFrameIndex++;
            if (CurrentFrame != null) {
                if (CurrentFrame.HasEvent) {
                    // if (CurrentFrame.Type == AnimationEvent.Type.Default) {
                    //     DefaultEventTriggered = true;
                    // }
                    _eventFrame = CurrentFrame;
                }
                _frameTimer.StartNewTime(_currentAnim.FrameTime * CurrentFrame.Length);
                return true;
            }
            if (!_currentAnim.IsComplete(_currentFrameIndex)) {
                return true;
            }
            Finished = true;
            _state = State.None;
            if (_animationQueue.Count > 0) {
                Play(_animationQueue.Dequeue());
                return true;
            }
            if (_currentAnim.Looping) {
                _currentFrameIndex = 0;
                _frameTimer.StartNewTime(_currentAnim.FrameTime * CurrentFrame?.Length ?? 1);
                return true;
            }
            return false;
        }

        private void SetupLowerTween() {
            _positionAnimator.UnScaled = false;
            _positionAnimator.EasingConfig = _lowerEasing;
            _positionAnimator.Length = _moveDuration;
            _positionAnimator.Restart(_pivot.localPosition, new Vector3(_resetPoint.x, _lowerPosition, _resetPoint.z));
        }

        private void SetupRaiseTween() {
            _positionAnimator.UnScaled = false;
            _positionAnimator.EasingConfig = _raiseEasing;
            _positionAnimator.Length = _moveDuration;
            _positionAnimator.Restart(_pivot.localPosition, _resetPoint);
        }

        private IEnumerator LowerWeapon(bool removing) {
            _state = removing ? State.Removing : State.Playing;
            SetupLowerTween();
            yield return null;
            while (_positionAnimator.Active) {
                _pivot.transform.localPosition = _positionAnimator.Get();
                yield return null;
            }
            _renderer.enabled = false;
            _state = State.None;
        }

        private IEnumerator RaiseWeapon() {
            _state = State.Playing;
            _renderer.enabled = true;
            SetupRaiseTween();
            yield return null;
            while (_positionAnimator.Active) {
                _pivot.transform.localPosition = _positionAnimator.Get();
                yield return null;
            }
            _bobTime = 0;
            _state = State.None;
            CheckQueue();
        }

        private IEnumerator SwapWeapons(ActionConfig newActionConfig) {
            yield return LowerWeapon(true);
            SetupUsable(newActionConfig);
            yield return RaiseWeapon();
        }

        private IEnumerator LoadWeaponProcess() {
            yield return LowerWeapon(false);
            _state = State.Reloading;
            UIChargeCircle.StopCharge();
            if (_state == State.None || _state == State.Reloading) {
                yield return RaiseWeapon();
            }
        }

#if UNITY_EDITOR
        [Button]
        private void TestReload() {
            TimeManager.StartUnscaled(RunTestReload());
        }

        private IEnumerator RunTestReload() {
            _resetPoint = _pivot.localPosition;
            //_simpleSpring.Transform = _pivot;
            _positionAnimator = new TweenV3();
            yield return LowerWeapon(false);
            yield return 0.5f;
            yield return RaiseWeapon();
        }

        [Button]
        private void WeaponBobTest() {
            TimeManager.StartUnscaled(RunWeaponBobTest());
        }

        private IEnumerator RunWeaponBobTest() {
            _resetPoint = _pivot.localPosition;
            _bobTime = 0;
            var timeStop = TimeManager.TimeUnscaled + 5;
            while (TimeManager.TimeUnscaled < timeStop) {
                _bobTime += TimeManager.DeltaUnscaled;
                var velocity = 1;
                var y = _verticalSwayAmount * Mathf.Sin((_swaySpeed * 2) * _bobTime) * velocity;
                var x = _horizontalSwayAmount * Mathf.Sin(_swaySpeed * _bobTime) * velocity;
                _pivot.localPosition = _resetPoint + new Vector3(x, y, 0);
                yield return null;
            }
            _pivot.localPosition = _resetPoint;
        }

        private static bool _looping;

        [SerializeField] private SpriteAnimation _testAnimation = null;

        [Button]
        public void TestAnimation() {
            _looping = true;
            TimeManager.StartUnscaled(TestAnimationRunner());
        }

        [Button]
        public void StopLoop() {
            _looping = false;
        }

        private IEnumerator TestAnimationRunner() {
            OnCreate(null);
            Play(_testAnimation);
            UpdateSpriteFrame();
            var maxTime = TimeManager.TimeUnscaled + 50;
            while (true) {
                if (!_looping || TimeManager.TimeUnscaled > maxTime) {
                    break;
                }
                if (CheckFrameUpdate()) {
                    UpdateSpriteFrame();
                }
                if (_state == State.None) {
                    Play(_testAnimation);
                }
                yield return null;
            }
            _state = State.None;
            _currentAnim = null;
        }

        private void OnDrawGizmosSelected() {
            UnityEditor.Handles.Label(transform.position, _state.ToString());
        }

#endif
        private enum State {
            None,
            Playing,
            Reloading,
            Removing,
        }
    }
}
