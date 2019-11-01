using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class IkPoserClip : SequenceObject {

        public AnimationCurve Curve = new AnimationCurve();
        public string StartPose;
        public string TargetPose;
        public IkPoses Source;
        
        [SerializeField] private float _duration = 0.2f;

        public override float Duration { get => _duration; set => _duration = value; }
        public override bool CanResize { get { return true; } }

        public override IRuntimeSequenceObject GetRuntime(IRuntimeSequence owner) {
            return new RuntimeObject(this, owner);
        }

        public override void DrawTimelineGui(Rect rect) {
            if (Curve == null) {
                Curve = new AnimationCurve();
            }
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(TargetPose) && name != TargetPose) {
                name = TargetPose;
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
            GUI.Box(rect,string.Format("{0} {1:F3}-{2:F3} Total: {3:F3}", TargetPose, StartTime, EndTime, Duration), GUI.skin.box);
        }

        public override void DrawEditorGui() {
#if UNITY_EDITOR
            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(this);
            UnityEditor.EditorGUILayout.PropertyField(so.FindProperty(nameof(Source)), true);
            UnityEditor.EditorGUILayout.PropertyField(so.FindProperty(nameof(Curve)), true);
            UnityEditor.EditorGUILayout.PropertyField(so.FindProperty(nameof(StartPose)), true);
            UnityEditor.EditorGUILayout.PropertyField(so.FindProperty(nameof(TargetPose)), true);
            so.ApplyModifiedProperties();
#endif
        }
        
        private class RuntimeObject : IRuntimeSequenceObject {

            private IkPoserClip _originalClip;
            private IkPoserClip _previous;
            private AnimationCurve _curve;
            private bool _started;
            private float _time;
            private List<SavedMuscleInstance> _pose = new List<SavedMuscleInstance>();
            private List<SimpleTween> _ikTargets = new List<SimpleTween>();
            private AnimationIkPoserComponent _targetComponent;
            private IRuntimeSequence _owner;
            public float StartTime { get => _originalClip.StartTime; }
            public float EndTime { get => _originalClip.EndTime; }

            public void Dispose() {
                _owner = null;
                _originalClip = null;
                _curve = null;
                _targetComponent = null;
                _previous = null;
                _started = false;
                _pose.Clear();
            }

            public RuntimeObject(IkPoserClip original, IRuntimeSequence owner) {
                _originalClip = original;
                _owner = owner;
                _curve = original.Curve;
                
                float minDiff = float.MaxValue;
                for (int i = 0; i < _owner.Sequence.Objects.Count; i++) {
                    var clip = _owner.Sequence.Objects[i] as IkPoserClip;
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
            }
            
            public void OnEnter() {
                Setup();
            }

            public void OnUpdate(float dt) {
                if (!_started) {
                    Setup();
                }
                if (_targetComponent == null) {
                    return;
                }
                _targetComponent.Poser.PoseAnimator.UpdatePose();
                var time = _owner.CurrentTime - _originalClip.StartTime;
                var percent = time / _originalClip.Duration;
                var curvePercent = _curve.Evaluate(percent);
                for (int i = 0; i < _pose.Count; i++) {
                    var muscle = _pose[i];
                    _targetComponent.Poser.PoseAnimator.HumanPose.muscles[muscle.MuscleIndex] = Mathf.Lerp(muscle.Start, muscle.Target, 
                    curvePercent);
                }
                _targetComponent.Poser.PoseAnimator.RefreshPose();
                for (int i = 0; i < _ikTargets.Count; i++) {
                    _ikTargets[i].Update(curvePercent);
                }
                _targetComponent.Poser.UpdateIk();
            }

            public void OnExit() {
                _started = false;
            }

            private void Setup() {
                _started = true;
                if (_targetComponent == null) {
                    _targetComponent = _owner.Entity.Get<AnimationIkPoserComponent>();
                }
                if (_targetComponent == null) {
                    return;
                }
                _targetComponent.Poser.AutoUpdate = false;
                _targetComponent.Poser.PoseAnimator.UpdatePose();
                SavedIkPose sourcePose = null;
                if (!string.IsNullOrEmpty(_originalClip.StartPose)) {
                    sourcePose = _originalClip.Source.GetPose(_originalClip.StartPose);
                }
                if (sourcePose == null && _previous != null) {
                    sourcePose = _previous.Source.GetPose(_previous.TargetPose);
                }
                SetupPose(_originalClip.Source.GetPose(_originalClip.TargetPose), sourcePose);
            }

            private void SetupPose(SavedIkPose targetPose, SavedIkPose sourcePose) {
                _pose.Clear();
                SetupHands(true, targetPose, sourcePose);
                SetupHands(false, targetPose, sourcePose);
                _ikTargets.Clear();
                var poser = _targetComponent.Poser;
                if (sourcePose != null) {
                    _ikTargets.Add(new TrMoveTween(poser.RightHandTarget, sourcePose.RightHandPos, targetPose.RightHandPos));
                    _ikTargets.Add(new TrMoveTween(poser.LeftHandTarget, sourcePose.LeftHandPos, targetPose.LeftHandPos));
                    _ikTargets.Add(new TrMoveTween(poser.RightShoulderTarget, sourcePose.RightShoulderPos, targetPose.RightShoulderPos));
                    _ikTargets.Add(new TrMoveTween(poser.LeftShoulderTarget, sourcePose.LeftShoulderPos, targetPose.LeftShoulderPos));

                    _ikTargets.Add(new TrRotationTween(poser.RightHandTarget, sourcePose.RightHandRot, targetPose.RightHandRot));
                    _ikTargets.Add(new TrRotationTween(poser.LeftHandTarget, sourcePose.LeftHandRot, targetPose.LeftHandRot));
                    _ikTargets.Add(new TrRotationTween(poser.RightShoulderTarget, sourcePose.RightShoulderRot, targetPose.RightShoulderRot));
                    _ikTargets.Add(new TrRotationTween(poser.LeftShoulderTarget, sourcePose.LeftShoulderRot, targetPose.LeftShoulderRot));
                }
                else {
                    _ikTargets.Add(new TrMoveTween(poser.RightHandTarget, targetPose.RightHandPos));
                    _ikTargets.Add(new TrMoveTween(poser.LeftHandTarget, targetPose.LeftHandPos));
                    _ikTargets.Add(new TrMoveTween(poser.RightShoulderTarget, targetPose.RightShoulderPos));
                    _ikTargets.Add(new TrMoveTween(poser.LeftShoulderTarget, targetPose.LeftShoulderPos));

                    _ikTargets.Add(new TrRotationTween(poser.RightHandTarget, targetPose.RightHandRot));
                    _ikTargets.Add(new TrRotationTween(poser.LeftHandTarget, targetPose.LeftHandRot));
                    _ikTargets.Add(new TrRotationTween(poser.RightShoulderTarget, targetPose.RightShoulderRot));
                    _ikTargets.Add(new TrRotationTween(poser.LeftShoulderTarget, targetPose.LeftShoulderRot));
                }
            }

            private void SetupHands(bool isRight, SavedIkPose targetPose, SavedIkPose sourcePose) {
                var muscleArray = isRight ? HumanPoseExtensions.RightHandMuscles : HumanPoseExtensions.LeftHandMuscles;
                for (int i = 0; i < muscleArray.Length; i++) {
                    var muscle = new SavedMuscleInstance();
                    muscle.MuscleIndex = muscleArray[i];
                    if (sourcePose == null) {
                        muscle.Start = _targetComponent.Poser.PoseAnimator.HumanPose.muscles[muscle.MuscleIndex];
                    }
                    else {
                        muscle.Start = isRight ? sourcePose.RightHandOpenClose : sourcePose.LeftHandOpenClose;
                    }
                    muscle.Target = isRight ? targetPose.RightHandOpenClose : targetPose.LeftHandOpenClose;
                    _pose.Add(muscle);
                }
            }
        }


        private abstract class SimpleTween {
            public abstract void Update(float percent);
        }

        private class TrMoveTween : SimpleTween {
            public Transform Tr { get; }
            public Vector3 Source { get; }
            public Vector3 Target { get; }

            public TrMoveTween(Transform tr, Vector3 source, Vector3 target) {
                Tr = tr;
                Source = source;
                Target = target;
            }

            public TrMoveTween(Transform tr, Vector3 target) {
                Tr = tr;
                Source = tr.localPosition;
                Target = target;
            }

            public override void Update(float percent) {
                Tr.localPosition = Vector3.Lerp(Source, Target, percent);
            }
        }

        private class TrRotationTween : SimpleTween {
            public Transform Tr { get; }
            public Quaternion Source { get; }
            public Quaternion Target { get; }

            public TrRotationTween(Transform tr, Quaternion source, Quaternion target) {
                Tr = tr;
                Source = source;
                Target = target;
            }

            public TrRotationTween(Transform tr, Quaternion target) {
                Tr = tr;
                Source = tr.localRotation;
                Target = target;
            }

            public override void Update(float percent) {
                Tr.localRotation = Quaternion.Slerp(Source, Target, percent);
            }
        }
    }
}
