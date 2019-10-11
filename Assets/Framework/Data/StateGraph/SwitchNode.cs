using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class SwitchNode : StateGraphNode {
        public override int OutputMax { get => 10; }
        public override int InputMax { get => 10; }
        public override int MaxConditions { get => 0; }
        public override bool HasConditions { get { return true; } }

        public string Variable = "Variable";
        public List<string> Values = new List<string>();
        public ConditionType Type = ConditionType.StringVariable;

        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label("Variable: ");
            Variable = GUILayout.TextField(Variable, textStyle);
            Type = (ConditionType) UnityEditor.EditorGUILayout.EnumPopup(Type, buttonStyle);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            for (int i = 0; i < OutPoints.Count; i++) {
                if (Values.Count <= i) {
                    Values.Add("");
                    Rect.size = new Vector2(
                        GetNodeSize.x * (OutPoints.Count > 0 ? 2f : 1f), GetNodeSize.y + ((GetNodeSize.y * 0.25f) * OutPoints.Count));
                }
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("Case: ");
                Values[i] = GUILayout.TextField(Values[i], textStyle);
                GUILayout.Label(" Exit " + i.ToString());
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
            }
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label("Else Exit ", textStyle);
            var indices = new string[OutPoints.Count];
            for (int i = 0; i < indices.Length; i++) {
                indices[i] = i.ToString();
            }
            DefaultExit = UnityEditor.EditorGUILayout.Popup(DefaultExit, indices, textStyle);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
#endif
            return false;
        }

        public override string Title { get { return "Switch"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }

        public class RuntimeNode : RuntimeStateNode {
            
            private RuntimeStateNode _exitNode;
            private SwitchNode _originalNode;
            
            public override RuntimeStateNode GetExitNode() {
                return _exitNode;
            }

            public RuntimeNode(SwitchNode node, RuntimeStateGraph graph) : base(node,graph) {
                _exitNode = GetOriginalNodeExit();
                _originalNode = node;
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
                return false;
            }
        }
    }
}
