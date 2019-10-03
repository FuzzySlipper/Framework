using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PoseAnimationClip : AnimationObject {
        
        public AnimationCurve Curve;
        public MusclePose TargetPose;

        private GenericPool<RuntimePoseAnimation> _pool = new GenericPool<RuntimePoseAnimation>(1, t=>t.Clear());

        public override IRuntimeAnimationObject GetRuntime(IRuntimeAnimationHolder owner) {
            var obj = _pool.New();
            obj.Set(this, owner);
            return obj;
        }

        public override void DisposeRuntime(IRuntimeAnimationObject runtime) {
            if (runtime is RuntimePoseAnimation pose) {
                _pool.Store(pose);
            }
        }

        public override void DrawTimelineGui(Rect rect) {
            base.DrawTimelineGui(rect);
#if UNITY_EDITOR
            if (TargetPose != null && name != TargetPose.name) {
                name = TargetPose.name;
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
            GUI.Box(rect,string.Format("{0} {1:F1}-{2:F1} Total: {3:F1}", TargetPose != null ? TargetPose.name : "No Pose", 
                StartTime, EndTime, Duration), GuiStyle);
        }

        public override void DrawEditorGui() {
#if UNITY_EDITOR
            Curve = UnityEditor.EditorGUILayout.CurveField("Curve", Curve);
            TargetPose = UnityEditor.EditorGUILayout.ObjectField("Pose", TargetPose, typeof(MusclePose), false) as MusclePose;
#endif
        }
    }

    public class RuntimePoseAnimation : IRuntimeAnimationObject {

        private PoseAnimationClip _originalClip;
        private AnimationCurve _curve;
        private bool _started;
        private List<SavedMuscleInstance> _pose = new List<SavedMuscleInstance>();
        private PoseAnimator _animator;
        private bool _foundPrevious;
        private IRuntimeAnimationHolder _owner;
        public float StartTime { get => _originalClip.StartTime; }
        public float EndTime { get => _originalClip.EndTime; }

        public void Clear() {
            _owner = null;
            _originalClip = null;
            _curve = null;
            _started = false;
            _pose.Clear();
            _animator = null;
        }
        
        public void Set(PoseAnimationClip original, IRuntimeAnimationHolder owner) {
            _originalClip = original;
            PoseAnimationClip previous = null;
            _owner = owner;
            float minDiff = float.MaxValue;
            for (int i = 0; i < _owner.Animation.Objects.Count; i++) {
                var clip = _owner.Animation.Objects[i] as PoseAnimationClip;
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
                previous = clip;
            }
            _foundPrevious = previous != null;
            _curve = original.Curve;
            for (int i = 0; i < original.TargetPose.Count; i++) {
                _pose.Add(new SavedMuscleInstance(original.TargetPose.Pose[i]));
                if (previous == null) {
                    continue;
                }
                var muscle = previous.TargetPose.GetMuscle(original.TargetPose.Pose[i].MuscleIndex);
                if (muscle != null) {
                    _pose[i].Start = muscle.Value;
                }
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
            _animator = PoseAnimator.Main;
            _animator.UpdatePose();
            if (_foundPrevious) {
                return;
            }
            for (int i = 0; i < _pose.Count; i++) {
                _pose[i].Start = _animator.HumanPose.muscles[_pose[i].MuscleIndex];
            }
        }
    }
}
