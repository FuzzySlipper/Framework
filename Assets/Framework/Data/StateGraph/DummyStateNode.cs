using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class DummyStateNode : StateGraphNode {

        [SerializeField] private float _timer = 0.1f;
        
        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR

            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(this);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            UnityEditor.EditorGUILayout.PropertyField(so.FindProperty(nameof(_timer)), GUIContent.none, true);
            so.ApplyModifiedProperties();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
#endif
            return false;
        }

        public override string Title { get { return "DummyNode"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }

        public class RuntimeNode : RuntimeStateNode {
            
            private RuntimeStateNode _exitNode;
            private UnscaledTimer _timer;
            
            public override RuntimeStateNode GetExitNode() {
                return _exitNode;
            }

            public RuntimeNode(DummyStateNode node, RuntimeStateGraph graph) : base(node,graph) {
                _exitNode = GetOriginalNodeExit();
                _timer = new UnscaledTimer(node._timer);
            }

            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
            }

            public override void OnExit() {
                base.OnExit();
            }

            public override bool TryComplete(float dt) {
                if (base.TryComplete(dt)) {
                    return true;
                }
                return !_timer.IsActive;
            }
        }
    }
}
