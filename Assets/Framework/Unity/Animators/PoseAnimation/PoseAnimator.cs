using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;

namespace PixelComrades {
    [ExecuteInEditMode]
    public class PoseAnimator : PlayerWeaponAnimator {

        [SerializeField] private Avatar _avatar = null;
        
        [SerializeField] private bool _isMain = true;
        [SerializeField] private TweenFloat _proceduralMovement = new TweenFloat();
        [SerializeField] private MusclePose _loweredPose = null;
        [SerializeField] private MusclePose _raisedPose = null;
        [SerializeField] private MusclePose _defaultPose = null;

        private HumanPose _pose;
        private HumanPoseHandler _hph = null;
        private Dictionary<string, RuntimeSequence> _animDictionary = new Dictionary<string, RuntimeSequence>();
        private RuntimeSequence _currentAnimation;
        private Queue<RuntimeSequence> _animationClipQueue = new Queue<RuntimeSequence>();
        private GenericPool<SavedMuscleInstance> _musclePool = new GenericPool<SavedMuscleInstance>(20);
        private List<SavedMuscleInstance> _currentMuscles = new List<SavedMuscleInstance>();
        private bool _eventTriggered = false;

        protected bool IsDirectorPlaying { get { return _currentAnimation != null && !_currentAnimation.IsComplete; } }
        public HumanPose HumanPose { get => _pose; }
        public MusclePose DefaultPose { get => WeaponModel != null ? WeaponModel.IdlePose : _defaultPose; }
        public override string CurrentAnimation { get => _currentAnimation?.Sequence.name ?? ""; }
        public override float CurrentAnimationLength { get => _currentAnimation != null ? (float) _currentAnimation.Length : 0f; }
        public override float CurrentAnimationRemaining { get { return _currentAnimation?.Remaining ?? 0; } }
        public override bool PlayingAnimation { get { return IsDirectorPlaying || (ProceduralAnimation != null && !ProceduralAnimation.Finished); } }

        private static PoseAnimator _main;
        public static PoseAnimator Main {
            get {
                if (_main == null) {
                    var animators = GameObject.FindObjectsOfType<PoseAnimator>();
                    for (int i = 0; i < animators.Length; i++) {
                        if (animators[i]._isMain) {
                            _main = animators[i];
                            break;
                        }
                    }
                }
                return _main;
            }
        }

        void Awake() {
            if (_isMain) {
                _main = this;
            }
            Init();
        }

        public override void SetEntity(Entity entity) {
            base.SetEntity(entity);
            foreach (var sequence in _animDictionary) {
                sequence.Value.SetEntity(entity);
            }
        }

        public override void OnCreate(PrefabEntity entity) {
            base.OnCreate(entity);
            var clips = Resources.LoadAll<GenericSequence>(UnityDirs.PlayerAnimations);
            for (int i = 0; i < clips.Length; i++) {
                _animDictionary.Add(clips[i].name, new RuntimeSequence( null,clips[i]));
            }
            ResetPose();
            SetPose(_defaultPose);
        }
        
        public void Init() {
            _hph = new HumanPoseHandler(_avatar, transform);
            _pose = new HumanPose();
            _hph.GetHumanPose(ref _pose);
            for (int i = 0; i < _pose.muscles.Length; i++) {
                _pose.muscles[i] = 0;
            }
        }

        [Button]
        public void ResetPose() {
            for (int i = 0; i < HumanPose.muscles.Length; i++) {
                HumanPose.muscles[i] = 0;
            }
        }

        //void FixedUpdate() {
        //    RefreshPose();
        //    if (_currentAnimation != null) {
        //        _director.time += TimeManager.DeltaTime;
        //        _director.Evaluate();
        //        if (_currentAnimation.TimeRemaining <= 0) {
        //            ClipFinished();
        //        }
        //    }
        //}

        public override void OnSystemUpdate(float dt) {
            base.OnSystemUpdate(dt);
            RefreshPose();
            if (_currentAnimation != null) {
                _currentAnimation.Update(dt);
                if (_currentAnimation.IsComplete) {
                    ClipFinished();
                }
            }
        }

        public void UpdatePose() {
            if (_hph == null) {
                Init();
            }
            _hph.GetHumanPose(ref _pose);
        }

        public void RefreshPose() {
            _pose.bodyPosition = Vector3.zero;
            _pose.bodyRotation = Quaternion.identity;
            _hph.SetHumanPose(ref _pose);
        }

        public override bool IsAnimationComplete(string clip) {
            return !_animDictionary.TryGetValue(clip, out var clipHolder) || clipHolder.IsComplete;
        }

        public override bool IsAnimationEventComplete(string clip) {
            return !_animDictionary.TryGetValue(clip, out var clipHolder) || _eventTriggered;
        }

        public override void PlayAnimation(string clip, bool overrideClip, Action action) {
            var state = CanPlayClip(clip, overrideClip);
            if (state != null) {
                AnimationAction = action;
                PlayAnimation(state);
            }
        }

        public override void ClipEventTriggered() {
            if (_currentAnimation != null) {
                _eventTriggered = true;
            }
            base.ClipEventTriggered();
        }

        private RuntimeSequence CanPlayClip(string clip, bool overrideClip) {
            if (!_animDictionary.TryGetValue(clip, out var state)) {
                return null;
            }
            if (!PlayingAnimation) {
                return state;
            }
            if (_currentAnimation == state) {
                return null;
            }
            if (!overrideClip) {
                if (!_animationClipQueue.Contains(state)) {
                    _animationClipQueue.Enqueue(state);
                }
                return null;
            }
            return state;
        }

        protected virtual void PlayAnimation(RuntimeSequence clip) {
            _eventTriggered = false;
            CurrentAnimationEvent = "";
            _currentAnimation = clip;
            RuntimeSequence.DebugSequence = clip;
            _currentAnimation.Play();
        }

        public override void StopCurrentAnimation() {
            if (_currentAnimation != null) {
                _currentAnimation.Stop();
            }
            _currentAnimation = null;
        }

        public void ClipFinished() {
            if (!_eventTriggered) {
                ClipEventTriggered();
            }
            _currentAnimation = null;
            CheckFinish();
        }

        protected override void CheckFinish() {
            if (_animationClipQueue.Count > 0) {
                PlayAnimation(_animationClipQueue.Dequeue());
            }
        }

        protected override void ProcessEvent(string eventName) {
            base.ProcessEvent(eventName);
            if (eventName == AnimationEvents.Default) {
                _eventTriggered = true;
            }
        }

        public override void SetWeaponModel(IWeaponModel model) {
            base.SetWeaponModel(model);
            if (model != null) {
                SetPose(model.IdlePose);
            }
        }

        protected override IEnumerator LowerArms() {
            yield return TransitionToPose(_loweredPose);
        }

        protected override IEnumerator RaiseArms() {
            yield return TransitionToPose(WeaponModel != null && WeaponModel.IdlePose != null ? WeaponModel.IdlePose : _raisedPose);
        }

        protected override IEnumerator TransitionToPose(MusclePose pose) {
            _proceduralMovement.Restart(0, 1);
            SetupPoseTransition(pose);
            while (_proceduralMovement.Active) {
                for (int i = 0; i < _currentMuscles.Count; i++) {
                    var muscle = _currentMuscles[i];
                    _pose.muscles[muscle.MuscleIndex] = Mathf.Lerp(muscle.Start, muscle.Target, _proceduralMovement.Get());
                }
                RefreshPose();
                yield return null;
            }
            ClearSavedMuscles();
        }

        private void SetupPoseTransition(MusclePose newPose) {
            ClearSavedMuscles();
            for (int i = 0; i < newPose.Pose.Count; i++) {
                var muscle = _musclePool.New();
                var savedMuscle = newPose.Pose[i];
                muscle.Set(savedMuscle, _pose.muscles[savedMuscle.MuscleIndex]);
                _currentMuscles.Add(muscle);
            }
        }

        private void SetPose(MusclePose pose) {
            UpdatePose();
            for (int i = 0; i < pose.Pose.Count; i++) {
                var muscle = pose.Pose[i];
                _pose.muscles[muscle.MuscleIndex] = muscle.Value;
            }
            RefreshPose();
        }

        private void ClearSavedMuscles() {
            for (int i = 0; i < _currentMuscles.Count; i++) {
                _musclePool.Store(_currentMuscles[i]);
            }
            _currentMuscles.Clear();
        }

#if UNITY_EDITOR
        void OnDrawGizmos() {
            if (PrimaryPivot == null || SecondaryPivot == null) {
                return;
            }
            System.Text.StringBuilder str = new StringBuilder();
            str.Append("DirectorPlaying ");
            str.Append(IsDirectorPlaying);
            str.Append(" ProceduralPlaying ");
            str.Append(ProceduralAnimation != null && !ProceduralAnimation.Finished);
            if (_currentAnimation != null) {
                str.Append(" Remaining ");
                str.Append(_currentAnimation.Remaining);
                str.Append(" Event");
                str.Append(_eventTriggered);
            }
            UnityEditor.Handles.Label(transform.position, str.ToString());
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.ConeHandleCap(999, PrimaryPivot.position, PrimaryPivot.rotation, 0.025f, EventType.Repaint);
            UnityEditor.Handles.ConeHandleCap(999, SecondaryPivot.position, SecondaryPivot.rotation, 0.025f, EventType.Repaint);
        }
#endif
    }
}
