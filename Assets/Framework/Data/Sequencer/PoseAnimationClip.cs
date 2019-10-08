using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PoseAnimationClip : SequenceObject {

        private static string _defaultName = "DefaultPose";
        
        public AnimationCurve Curve;
        public MusclePose TargetPose;
        
        [SerializeField] private float _duration = 0.2f;

        public override float Duration { get => _duration; set => _duration = value; }
        public override bool CanResize { get { return true; } }

        public override IRuntimeSequenceObject GetRuntime(IRuntimeSequence owner) {
            return new RuntimePoseAnimation(this, owner);
        }

        public override void DrawTimelineGui(Rect rect) {
            
#if UNITY_EDITOR
            if (TargetPose != null && name != TargetPose.name) {
                name = TargetPose.name;
                UnityEditor.EditorUtility.SetDirty(this);
            }
            else if (TargetPose == null && name != _defaultName) {
                name = _defaultName;
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
            GUI.Box(rect,string.Format("{0} {1:F3}-{2:F3} Total: {3:F3}", TargetPose != null ? TargetPose.name : "Default Pose", 
                StartTime, EndTime, Duration), GUI.skin.box);
        }

        public override void DrawEditorGui() {
#if UNITY_EDITOR
            
            Curve = UnityEditor.EditorGUILayout.CurveField("Curve", Curve);
            TargetPose = UnityEditor.EditorGUILayout.ObjectField("Pose", TargetPose, typeof(MusclePose), false) as MusclePose;
            if (AnimationCurveExtension.ClipBoardAnimationCurve != null) {
                if (GUILayout.Button("Paste Animation Curve")) {
                    Curve.keys = AnimationCurveExtension.ClipBoardAnimationCurve.keys;
                    Curve.postWrapMode = AnimationCurveExtension.ClipBoardAnimationCurve.postWrapMode;
                    Curve.preWrapMode = AnimationCurveExtension.ClipBoardAnimationCurve.preWrapMode;
                }
            }
#endif
        }
    }

    public class RuntimePoseAnimation : IRuntimeSequenceObject {

        private PoseAnimationClip _originalClip;
        private PoseAnimationClip _previous;
        private AnimationCurve _curve;
        private bool _started;
        private List<SavedMuscleInstance> _pose = new List<SavedMuscleInstance>();
        private PoseAnimator _animator;
        private IRuntimeSequence _owner;
        public float StartTime { get => _originalClip.StartTime; }
        public float EndTime { get => _originalClip.EndTime; }

        public void Dispose() {
            _owner = null;
            _originalClip = null;
            _curve = null;
            _animator = null;
            _previous = null;
            _started = false;
            _pose.Clear();
        }

        public RuntimePoseAnimation(PoseAnimationClip original, IRuntimeSequence owner) {
            _originalClip = original;
            _owner = owner;
            _curve = original.Curve;
            
            float minDiff = float.MaxValue;
            for (int i = 0; i < _owner.Sequence.Objects.Count; i++) {
                var clip = _owner.Sequence.Objects[i] as PoseAnimationClip;
                if (clip == null || clip == original) {
                    continue;
                }
                if (clip.EndTime > original.StartTime) {
                    continue;
                }
                var diff = original.StartTime - clip.EndTime;
                if (diff > minDiff) {
                    continue;
                }
                minDiff = diff;
                _previous = clip;
            }
            if (original.TargetPose != null) {
                SetupPose(original.TargetPose);
            }
        }
        
        public void OnEnter() {
            Setup();
        }

        public void OnUpdate(float dt) {
            if (!_started) {
                Setup();
            }
            var time = _owner.CurrentTime - _originalClip.StartTime;
            var percent = time / _originalClip.Duration;
            var animationPercent = _curve.Evaluate(percent);
            for (int i = 0; i < _pose.Count; i++) {
                var muscle = _pose[i];
                _animator.HumanPose.muscles[muscle.MuscleIndex] = Mathf.Lerp(muscle.Start, muscle.Target, animationPercent);
            }
            _animator.RefreshPose();
        }

        public void OnExit() {
            _started = false;
        }

        private void Setup() {
            _started = true;
            if (_animator == null) {
                _animator = PoseAnimator.Main;
            }
            _animator.UpdatePose();
            if (_originalClip.TargetPose == null) {
                _pose.Clear();
                SetupDefaultPose();
                return;
            }
            if (_previous != null) {
                return;
            }
            for (int i = 0; i < _pose.Count; i++) {
                _pose[i].Start = _animator.HumanPose.muscles[_pose[i].MuscleIndex];
            }
        }

        private void SetupDefaultPose() {
            var targetPose = _animator.DefaultPose;
            for (int i = 0; i < targetPose.Count; i++) {
                var muscleIndex = targetPose.Pose[i].MuscleIndex;
                float startPose;
                if (_previous != null && _previous.TargetPose.HasPose(muscleIndex)) {
                    startPose = _previous.TargetPose.GetMuscle(muscleIndex).Value;
                }
                else {
                    startPose = _animator.HumanPose.muscles[muscleIndex];
                }
                _pose.Add(new SavedMuscleInstance(targetPose.Pose[i], startPose));
            }
        }

        private void SetupPose(MusclePose targetPose) {
            for (int i = 0; i < targetPose.Count; i++) {
                _pose.Add(new SavedMuscleInstance(targetPose.Pose[i]));
                if (_previous == null) {
                    continue;
                }
                var muscle = _previous.TargetPose.GetMuscle(targetPose.Pose[i].MuscleIndex);
                if (muscle != null) {
                    _pose[i].Start = muscle.Value;
                }
            }
        }
    }
}
