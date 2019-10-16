using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class MusclePoseNode : StateGraphNode {

        public AnimationCurve Curve = new AnimationCurve();
        public MusclePose TargetPose;
        public float Duration = 0.2f;
        protected override Vector2 GetNodeSize { get { return new Vector2(base.GetNodeSize.x, base.GetNodeSize.y * 1.5f);} }

        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            TargetPose = UnityEditor.EditorGUILayout.ObjectField(TargetPose, typeof(MusclePose), false) as MusclePose;
            Curve = UnityEditor.EditorGUILayout.CurveField(Curve);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            Duration = UnityEditor.EditorGUILayout.Slider(Duration, 0, 2);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
#endif
            return false;
        }

        public override string Title { get { return TargetPose != null ? TargetPose.name : "MusclePose"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }

        public class RuntimeNode : RuntimeStateNode {
            
            private MusclePoseNode _originalNode;
            private PoseAnimatorComponent _poseAnimator;
            private List<SavedMuscleInstance> _pose = new List<SavedMuscleInstance>();
            private float _time;
            public override string DebugInfo {
                get {
                    return string.Format(
                        "Time {0:F3} Percent {1:F2}", _time,
                        _time / _originalNode.Duration);
                }
            }
            public RuntimeNode(MusclePoseNode node, RuntimeStateGraph graph) : base(node,graph) {
                _originalNode = node;
            }

            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                if (_poseAnimator == null) {
                    _poseAnimator = Graph.Entity.Get<PoseAnimatorComponent>();
                }
                Setup();
            }

            public override bool TryComplete(float dt) {
                if (base.TryComplete(dt)) {
                    return true;
                }
                if (_poseAnimator == null) {
                    return true;
                }
                _time += dt;
                var percent = _time / _originalNode.Duration;
                var animationPercent = _originalNode.Curve.Evaluate(percent);
                for (int i = 0; i < _pose.Count; i++) {
                    var muscle = _pose[i];
                    _poseAnimator.HumanPose.muscles[muscle.MuscleIndex] =
                        Mathf.Lerp(muscle.Start, muscle.Target, animationPercent);
                }
                _poseAnimator.RefreshPose();
                return percent >= 1;
            }

            private void Setup() {
                _time = 0;
                if (_poseAnimator == null) {
                    return;
                }
                _poseAnimator.UpdatePose();
                _pose.Clear();
                var targetPose = _originalNode.TargetPose;
                for (int i = 0; i < targetPose.Count; i++) {
                    var muscleIndex = targetPose.Pose[i].MuscleIndex;
                    float startPose = _poseAnimator.HumanPose.muscles[muscleIndex];
                    _pose.Add(new SavedMuscleInstance(targetPose.Pose[i], startPose));
                }
            }

        }
    }
}
