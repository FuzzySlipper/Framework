using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine.Playables;

namespace PixelComrades {
    [ExecuteInEditMode]
    public class PoseAnimator : PlayerWeaponAnimator, INotificationReceiver {

        [SerializeField] private Avatar _avatar = null;
        
        [SerializeField] private bool _isMain = true;
        [SerializeField] private PlayableDirector _director = null;
        [SerializeField] private TweenFloat _proceduralMovement = new TweenFloat();
        [SerializeField] private MusclePose _loweredPose = null;
        [SerializeField] private MusclePose _raisedPose = null;
        [SerializeField] private MusclePose _defaultPose = null;

        private HumanPose _pose;
        private HumanPoseHandler _hph = null;
        private Dictionary<string, PlayableClipState> _animDictionary = new Dictionary<string, PlayableClipState>();
        private PlayableClipState _currentAnimation;
        private Queue<PlayableClipState> _animationClipQueue = new Queue<PlayableClipState>();
        private GenericPool<SavedMuscleInstance> _musclePool = new GenericPool<SavedMuscleInstance>(20);
        private List<SavedMuscleInstance> _currentMuscles = new List<SavedMuscleInstance>();

        protected bool IsDirectorPlaying { get { return _currentAnimation != null && !_currentAnimation.Complete; } }
        public HumanPose HumanPose { get => _pose; }
        public MusclePose DefaultPose { get => WeaponModel != null ? WeaponModel.IdlePose : _defaultPose; }
        public override string CurrentAnimation { get => _currentAnimation?.Id ?? ""; }
        public override float CurrentAnimationLength { get => _currentAnimation != null ? (float) _currentAnimation.Clip.duration : 0f; }
        public override float CurrentAnimationRemaining { get { return _currentAnimation?.TimeRemaining ?? 0; } }
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

        public override void OnCreate(PrefabEntity entity) {
            base.OnCreate(entity);
            var clips = Resources.LoadAll<PlayableAsset>(UnityDirs.PlayerAnimations);
            for (int i = 0; i < clips.Length; i++) {
                var state = new PlayableClipState(clips[i]);
                _animDictionary.Add(state.Id, state);
            }
            ResetPose();
            SetPose(_defaultPose);
            MessageKit.addObserver(Messages.PauseChanged, CheckPause);
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
                if (_currentAnimation.TimeRemaining <= 0) {
                    ClipFinished();
                }
            }
        }

        private void CheckPause() {
            if (_currentAnimation != null) {
                if (Game.Paused) {
                    _director.Pause();
                }
                else {
                    _director.Resume();
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

        public void OnNotify(Playable origin, INotification notification, object context) {
            if (notification is AnimationEventMarker eventMarker) {
                ProcessEvent(eventMarker.Event);
            }
            else if (notification is HandPoseMarker handPose && handPose.Pose != null) {
                handPose.Pose.SetPose(this);
            }
        }

        public override bool IsAnimationComplete(string clip) {
            return !_animDictionary.TryGetValue(clip, out var clipHolder) || clipHolder.Complete;
        }

        public override bool IsAnimationEventComplete(string clip) {
            return !_animDictionary.TryGetValue(clip, out var clipHolder) || clipHolder.EventTriggered;
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
                _currentAnimation.EventTriggered = true;
            }
            base.ClipEventTriggered();
        }

        protected PlayableClipState CanPlayClip(string clip, bool overrideClip) {
            if (!_animDictionary.TryGetValue(clip, out var state) || state.Clip == null) {
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
                    state.Reset();
                    _animationClipQueue.Enqueue(state);
                }
                return null;
            }
            return state;
        }

        protected virtual void PlayAnimation(PlayableClipState clip) {
            CurrentAnimationEvent = "";
            _currentAnimation = clip;
            _currentAnimation.Start();
            _director.time = 0;
            _director.Play(clip.Clip);
        }

        public override void StopCurrentAnimation() {
            _director.Stop();
            _director.playableAsset = null;
            _currentAnimation = null;
        }

        public void ClipFinished() {
            _currentAnimation.Complete = true;
            if (!_currentAnimation.EventTriggered) {
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
                str.Append(_currentAnimation.TimeRemaining);
                str.Append(" Event");
                str.Append(_currentAnimation.EventTriggered);
            }
            UnityEditor.Handles.Label(transform.position, str.ToString());
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.ConeHandleCap(999, PrimaryPivot.position, PrimaryPivot.rotation, 0.025f, EventType.Repaint);
            UnityEditor.Handles.ConeHandleCap(999, SecondaryPivot.position, SecondaryPivot.rotation, 0.025f, EventType.Repaint);
        }
#endif
    }
}
