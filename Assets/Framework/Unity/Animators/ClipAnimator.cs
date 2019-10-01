using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if Animancer
using Animancer;
#endif
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PixelComrades {
    public class ClipAnimator : PlayerWeaponAnimator, IPoolEvents, ISystemUpdate {

        [SerializeField] private int _layer = 0;
#if Animancer
        [SerializeField] private SimpleEventAnimancerController _controller = null;
#endif
        [SerializeField] private AnimationClipState[] _clips = new AnimationClipState[0];
        [SerializeField] private Animator _animator = null;
        [SerializeField] private bool _checkMovement = false;
        [SerializeField] protected float FadeDuration = 0.5f;

        private bool _playing;
        private AnimationClipState _currentAnimation;
#if Animancer
        private Dictionary<AnimationClipState, AnimancerState> _animancerDictionary = new Dictionary<AnimationClipState, AnimancerState>();
        private AnimancerState _currentState;
        public System.Action<AnimancerState, AnimationClipState> OnAnimationStart;
        public SimpleEventAnimancerController Controller { get => _controller; }
        protected AnimancerState CurrentState { get { return _currentState; } }
#endif
        protected Dictionary<string, AnimationClipState> _animDictionary = new Dictionary<string, AnimationClipState>();
        protected Queue<AnimationClipState> _animationClipQueue = new Queue<AnimationClipState>();
        private float _currentClipTime = 0;
        
        private int _lastClipFrame = -1;
        
        public override string CurrentAnimation { get { return _currentAnimation.Id; } }
        public Animator Animator { get => _animator; set => _animator = value; }
        public override float CurrentAnimationLength { get { return _currentAnimation?.ClipLength ?? 1f; } }
        public override float CurrentAnimationRemaining { get { return _currentAnimation?.AdjustedLength - _currentClipTime ?? 0f; } }
        public AnimationClipState[] Clips { get => _clips; set => _clips = value; }
        
        public override bool PlayingAnimation { get { return _playing; } }
        
        protected int Layer { get { return _layer; } }
        public AnimationClipState CurrentAnimationClipState { get { return _currentAnimation; } }
        private string CurrentClipID { get { return _currentAnimation?.Id ?? ""; } }
        
        public override void OnCreate(PrefabEntity entity) {
            base.OnCreate(entity);
            BuildDictionary();
        }

        private void BuildDictionary() {
            _animDictionary.Clear();
            for (int i = 0; i < _clips.Length; i++) {
                var clip = _clips[i];
                if (clip.Clip == null) {
                    continue;
                }
                _animDictionary.TryAdd(clip.Id, clip);
            }
        }

        public void OnPoolSpawned() {
            MessageKit.addObserver(Messages.PauseChanged, CheckPause);
            if (_checkMovement) {
                PlayAnimation(AnimationIds.Idle, false);
            }
        }

        public void OnPoolDespawned() {
            MessageKit.removeObserver(Messages.PauseChanged, CheckPause);
            ClearWeaponModel();
        }

        public override void OnSystemUpdate(float dt) {
            base.OnSystemUpdate(dt);
#if Animancer
            if (_currentAnimation != null && _currentState != null) {
                _currentClipTime += dt * _currentAnimation.PlaySpeedMultiplier;
                if (_currentClipTime > _currentAnimation.ClipLength) {
                    ClipFinished();
                    return;
                }
                var frame = _currentAnimation.CalculateCurrentFrame(_currentClipTime);
                if (frame < 0) {
                    ClipFinished();
                    return;
                }
                if (frame != _lastClipFrame && frame > _lastClipFrame) {
                    AdvanceClip(frame);
                }
            }
            if (!_checkMovement || Entity == null || Entity.IsDead()) {
                return;
            }
            if (CurrentClipID == AnimationIds.Move && !Entity.Tags.Contain(EntityTags.Moving)) {
                PlayAnimation(AnimationIds.Idle, false);
            }
            else if (CurrentClipID == AnimationIds.Idle && Entity.Tags.Contain(EntityTags.Moving)) {
                PlayAnimation(AnimationIds.Move, false);
            }
#endif
        }

        private void AdvanceClip(int frame) {
            _lastClipFrame = frame;
            if (!string.IsNullOrEmpty(_currentAnimation.Events[frame])) {
                ProcessEvent(_currentAnimation.Events[frame]);
            }
#if Animancer
            _currentState.IsPlaying = true;
            _currentState.NormalizedTime = _currentAnimation.ConvertFrameToAnimationTime(frame)/_currentAnimation.ClipLength;
            _currentState.IsPlaying = false;
#endif
        }


        public AnimationClipState GetState(AnimationClip clip) {
            for (int i = 0; i < Clips.Length; i++) {
                if (Clips[i].Clip == clip) {
                    return Clips[i];
                }
            }
            return null;
        }

        private void CheckPause() {
#if Animancer
            if (Game.Paused) {
                _controller.PauseAll();
            }
            else {
                _controller.ResumeAll();
            }
#endif
        }

        public bool IsAnimationComplete() {
            return !PlayingAnimation;
        }

        public override bool IsAnimationComplete(string clip) {
            return !_animDictionary.TryGetValue(clip, out var clipHolder) || clipHolder.Complete;
        }

        public override bool IsAnimationEventComplete(string clip) {
            return !_animDictionary.TryGetValue(clip, out var clipHolder) || clipHolder.EventTriggered;
        }

        public bool IsPlayingAnimation(string clip) {
            return CurrentClipID == clip;
        }

        public override void PlayAnimation(string clip, bool overrideClip, Action action) {
            var state = CanPlayClip(clip, overrideClip);
            if (state != null) {
                PlayAnimation(state);
            }
        }

        public override void ClipEventTriggered() {
            if (_currentAnimation != null) {
                _currentAnimation.EventTriggered = true;
            }
            base.ClipEventTriggered();
        }

        protected AnimationClipState CanPlayClip(string clip, bool overrideClip) {
            if (!_animDictionary.TryGetValue(clip, out var state) || state.Clip == null) {
                return null;
            }
            if (_playing && _currentAnimation != null && !_currentAnimation.Complete) {
                if (_currentAnimation == state) {
                    return null;
                }
                if (!overrideClip) {
                    if (!_animationClipQueue.Contains(state)) {
                        state.ResetBools();
                        _animationClipQueue.Enqueue(state);
                    }
                    return null;
                }
            }
            return state;
        }

        public override void StopCurrentAnimation() {
#if Animancer
            if (_currentState != null) {
                _currentState.Stop();
            }
            _playing = false;
            _currentAnimation = null;
            _currentState = null;
#endif
        }

        protected virtual void PlayAnimation(AnimationClipState clip) {
            _currentAnimation = clip;
            _currentAnimation.ResetBools();
#if Animancer
            if (!_animancerDictionary.TryGetValue(clip, out _currentState)) {
                _currentState = _controller.CreateState(clip.ClipName + Layer.ToString(), clip.Clip, Layer);
                _animancerDictionary.Add(clip, _currentState);
            }
            _controller.CrossFade(_currentState, FadeDuration);
            _currentState.IsPlaying = false;
            //_controller.OnEvent += ClipEventTriggered;
            //state.OnEnd += ClipFinished;
            OnAnimationStart?.Invoke(_currentState, _currentAnimation);
            _lastClipFrame = -1;
            _currentClipTime = 0;
            if (!_animationsIgnorePlaying.Contains(_currentAnimation.Id)) {
                _playing = true;
            }
#endif
        }

        public void ClipFinished() {
            _currentAnimation.Complete = true;
            if (!_currentAnimation.EventTriggered) {
                ClipEventTriggered();
            }
            CheckFinish();
        }

        protected override void CheckFinish() {
            _playing = false;
            if (_animationClipQueue.Count > 0) {
                PlayAnimation(_animationClipQueue.Dequeue());
                return;
            }
            if (_animationClipQueue.Count == 0 && _currentAnimation!= null && _currentAnimation.Clip.isLooping) {
                _currentClipTime = 0;
                _lastClipFrame = -1;
            }
        }

        private static HashSet<string> _animationsIgnorePlaying = new HashSet<string>() {
            AnimationIds.Idle, AnimationIds.Move, PlayerAnimationIds.Idle1H, PlayerAnimationIds.Idle2H, 
            PlayerAnimationIds.IdleMelee1H, PlayerAnimationIds.IdleMelee2H
        };
        protected override IEnumerator LowerArms() {
            yield break;
        }

        protected override IEnumerator RaiseArms() {
            yield break;
        }
        
        protected override IEnumerator TransitionToPose(MusclePose pose) {
            yield break;
        }
    }
}
