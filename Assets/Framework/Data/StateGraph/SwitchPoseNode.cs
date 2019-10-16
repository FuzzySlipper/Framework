using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class SwitchPoseNode : StateGraphNode {
    
        public AnimationCurve Curve = new AnimationCurve();
        public float Duration = 0.2f;
        public string VariableName = "Variable";
        public MusclePose Default;
        public List<string> Values = new List<string>();
        public List<MusclePose> TargetPoses = new List<MusclePose>();
        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label("Default: ");
            Default = UnityEditor.EditorGUILayout.ObjectField(Default, typeof(MusclePose), false) as MusclePose;
            Curve = UnityEditor.EditorGUILayout.CurveField(Curve);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            Duration = UnityEditor.EditorGUILayout.Slider(Duration, 0, 2);
            GUILayout.Label("Variable: ");
            var graphLabels = GraphVariables.GetValues();
            var index = System.Array.IndexOf(graphLabels, VariableName);
            var newVar = UnityEditor.EditorGUILayout.Popup(
                index, graphLabels, buttonStyle, new[] {
                    GUILayout.MaxWidth
                        (StateGraphNode.DefaultNodeSize.x * 0.5f)
                });
            if (newVar != index) {
                VariableName = graphLabels[newVar];
            }
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            
            for (int i = 0; i < TargetPoses.Count; i++) {
                if (Values.Count <= i) {
                    Values.Add("");
                    Rect.size = GetNodeSize;
                }
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("Case:");
                Values[i] = GUILayout.TextField(Values[i], textStyle);
                TargetPoses[i] = UnityEditor.EditorGUILayout.ObjectField(TargetPoses[i], typeof(MusclePose), false) as MusclePose;
                if (GUILayout.Button("X")) {
                    TargetPoses.RemoveAt(i);
                    Values.RemoveAt(i);
                    CheckSize();
                    GUILayout.Space(20);
                    GUILayout.EndHorizontal();
                    break;
                }
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (GUILayout.Button("Add Pose")) {
                TargetPoses.Add(null);
                CheckSize();
            }
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
#endif
            return false;
        }

        public override string Title { get { return "Switch Pose"; } }
        protected override Vector2 GetNodeSize {
            get {
                return new Vector2(
                    DefaultNodeSize.x * (TargetPoses.Count > 0 ? 1.5f : 1f),
                    DefaultNodeSize.y + ((DefaultNodeSize.y * 0.5f) * TargetPoses.Count));
            }
        }
        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }

        public class RuntimeNode : RuntimeStateNode {
            
            private SwitchPoseNode _originalNode;
            private PoseAnimatorComponent _poseAnimator;
            private List<SavedMuscleInstance> _pose = new List<SavedMuscleInstance>();
            private float _time;
            public override string DebugInfo { get { return string.Format("Time {0:F3} Percent {1:F2}", _time,
                _time / _originalNode.Duration); } }
            public RuntimeNode(SwitchPoseNode node, RuntimeStateGraph graph) : base(node,graph) {
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

            private MusclePose FindTargetPose() {
                var variable = Graph.GetVariable<string>(_originalNode.VariableName);
                for (int i = 0; i < _originalNode.Values.Count; i++) {
                    if (variable == _originalNode.Values[i]) {
                        return _originalNode.TargetPoses[i];
                    }
                }
                return _originalNode.Default;
            }

            private void Setup() {
                _time = 0;
                if (_poseAnimator == null) {
                    return;
                }
                _poseAnimator.UpdatePose();
                _pose.Clear();
                var targetPose = FindTargetPose();
                for (int i = 0; i < targetPose.Count; i++) {
                    var muscleIndex = targetPose.Pose[i].MuscleIndex;
                    float startPose = _poseAnimator.HumanPose.muscles[muscleIndex];
                    _pose.Add(new SavedMuscleInstance(targetPose.Pose[i], startPose));
                }
            }
        }
    }
}
