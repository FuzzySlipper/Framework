using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class MoveFpPivotNode : StateGraphNode {
        public AnimationCurve Curve = new AnimationCurve();
        public float Duration = 0.2f;
        public Vector3 Target = Vector3.zero;

        protected override Vector2 GetNodeSize { get { return new Vector2(DefaultNodeSize.x, DefaultNodeSize.y * 1.25f); } }

        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR
            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(this);
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            UnityEditor.EditorGUILayout.PropertyField(so.FindProperty(nameof(Curve)), GUIContent.none, true);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            UnityEditor.EditorGUILayout.PropertyField(so.FindProperty(nameof(Duration)), GUIContent.none, true);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            UnityEditor.EditorGUILayout.PropertyField(so.FindProperty(nameof(Target)), GUIContent.none, true);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            
            so.ApplyModifiedProperties();
#endif
            return false;
        }

        public override string Title { get { return "MoveFpPivot " + Target.ToString(); } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }

        private class RuntimeNode : RuntimeStateNode {
            private MoveFpPivotNode _node;
            private Vector3 _origin;
            private FpPivotComponent _component;
            private float _time;
            public RuntimeNode(MoveFpPivotNode node, RuntimeStateGraph graph) : base(node, graph) {
                _node = node;
            }

            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                _component = Graph.Entity.Get<FpPivotComponent>();
                if (_component != null) {
                    _origin = _component.Tr.localPosition;
                }
                _time = 0;
            }

            public override void OnExit() {
                base.OnExit();
                _component = null;
            }

            public override bool TryComplete(float dt) {
                if (base.TryComplete(dt) || _component == null) {
                    return true;
                }
                _time += dt;
                var percent = _time / _node.Duration;
                var animationPercent = _node.Curve.Evaluate(percent);
                _component.Tr.localPosition = Vector3.Lerp(_origin, _node.Target, animationPercent);
                return percent >= 1;
            }
        }
    }
}
