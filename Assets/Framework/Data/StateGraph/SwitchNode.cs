using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class SwitchNode : StateGraphNode {
        public override int OutputMax { get => 10; }
        public override int InputMax { get => 10; }
        public override int MaxConditions { get => 0; }
        public override bool HasConditions { get { return true; } }

        public string VariableName = "Variable";
        public List<string> Values = new List<string>();

        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label("Variable: ");
            var graphLabels = GraphVariables.GetNames().ToArray();
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
            for (int i = 0; i < OutPoints.Count; i++) {
                if (Values.Count <= i) {
                    Values.Add("");
                    CheckSize();
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

        protected override Vector2 GetNodeSize {
            get {
                return new Vector2(
                    DefaultNodeSize.x * (OutPoints.Count > 0 ? 1.5f : 1f),
                    DefaultNodeSize.y + ((DefaultNodeSize.y * 0.2f) * OutPoints.Count));
            }
        }
        public override string Title { get { return "Switch"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }

        public class RuntimeNode : RuntimeStateNode {
            
            private SwitchNode _originalNode;
            
            public override RuntimeStateNode GetExitNode() {
                var variable = Graph.GetVariable<string>(_originalNode.VariableName);
                for (int i = 0; i < _originalNode.Values.Count; i++) {
                    if (variable == _originalNode.Values[i]) {
                        var exitNode = Graph.OriginalGraph.GetConnectionEndpoint(Node.OutPoints[i]);
                        if (exitNode != null) {
                            return Graph.GetRuntimeNode(exitNode.Id);
                        }
                    }
                }
                var defNode = Graph.OriginalGraph.GetConnectionEndpoint(Node.OutPoints[_originalNode.DefaultExit]);
                if (defNode != null) {
                    return Graph.GetRuntimeNode(defNode.Id);
                }
                return null;
            }

            public RuntimeNode(SwitchNode node, RuntimeStateGraph graph) : base(node,graph) {
                _originalNode = node;
            }

            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
            }

            public override void OnExit() {
                base.OnExit();
            }

            public override bool TryComplete(float dt) {
                return true;
            }
        }
    }
}
