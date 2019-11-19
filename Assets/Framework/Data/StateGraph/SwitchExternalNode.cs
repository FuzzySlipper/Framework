using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class SwitchExternalNode : StateGraphNode {

        public override int MaxConditions { get => 0; }
        public override bool HasConditions { get { return false; } }

        public string VariableName = "Variable";
        public List<string> Values = new List<string>();
        public StateGraph[] Graphs = new StateGraph[0];
        public int DefaultIndex = 0;

        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label("Variable: ");
            var graphLabels = GraphVariables.GetValues();
            var index = System.Array.IndexOf(graphLabels, VariableName);
            var newVar = UnityEditor.EditorGUILayout.Popup(
                index, graphLabels, buttonStyle, new[] {
                    GUILayout.MaxWidth (StateGraphNode.DefaultNodeSize.x * 0.5f)
                });
            if (newVar != index) {
                VariableName = graphLabels[newVar];
            }
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            for (int i = 0; i < Graphs.Length; i++) {
                if (Values.Count <= i) {
                    Values.Add("");
                    CheckSize();
                }
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("Case: ");
                Values[i] = GUILayout.TextField(Values[i], textStyle);
                Graphs[i] = UnityEditor.EditorGUILayout.ObjectField(Graphs[i], typeof(StateGraph), false, GUILayout.MaxWidth(
                        StateGraphNode.DefaultNodeSize.x * 0.5f)) as
                    StateGraph;
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
            }
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label("Else Pick ", textStyle);
            var indices = new string[Values.Count];
            for (int i = 0; i < indices.Length; i++) {
                indices[i] = i.ToString();
            }
            DefaultIndex = UnityEditor.EditorGUILayout.Popup(DefaultIndex, indices, textStyle);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (GUILayout.Button("Add Case")) {
                Values.Add("");
                System.Array.Resize(ref Graphs, Graphs.Length + 1);
                CheckSize();
            }
            if (Graphs.Length > 0 && GUILayout.Button("Remove Case")) {
                Values.RemoveLast();
                System.Array.Resize(ref Graphs, Graphs.Length - 1);
                CheckSize();
            }
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
#endif
            return false;
        }

        protected override Vector2 GetNodeSize {
            get {
                return new Vector2(
                    DefaultNodeSize.x * (Values.Count > 0 ? 1.5f : 1f),
                    DefaultNodeSize.y + ((DefaultNodeSize.y * 0.1f) * Values.Count));
            }
        }
        public override string Title { get { return "Switch Graphs"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }

        public class RuntimeNode : RuntimeStateNode {
            private SwitchExternalNode _originalNode;
            private RuntimeStateGraph[] _externalGraphs;
            private int _currentIndex = 0;
            private bool _completed;
            
            public RuntimeStateGraph ExternalGraph { get { return _externalGraphs[_currentIndex]; } }
            public RuntimeNode(SwitchExternalNode node, RuntimeStateGraph graph) : base(node,graph) {
                _originalNode = node;
                _externalGraphs = new RuntimeStateGraph[node.Graphs.Length];
                for (int i = 0; i < _externalGraphs.Length; i++) {
                    _externalGraphs[i] = node.Graphs[i].GetRuntimeGraph(graph, graph.Entity);
                    _externalGraphs[i].OnComplete += ExternalGraphCompleted;
                }
            }

            private void ExternalGraphCompleted() {
                _completed = true;
            }

            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                var variable = Graph.GetVariable<string>(_originalNode.VariableName);
                _currentIndex = _originalNode.DefaultIndex;
                for (int i = 0; i < _originalNode.Values.Count; i++) {
                    if (variable == _originalNode.Values[i]) {
                        _currentIndex = i;
                        break;
                    }
                }
                _completed = false;
                if (ExternalGraph == null) {
                    return;
                }
                ExternalGraph.Start();
            }

            public override void OnExit() {
                base.OnExit();
                if (ExternalGraph == null) {
                    return;
                }
                ExternalGraph.Stop();
            }

            public override bool TryComplete(float dt) {
                if (base.TryComplete(dt) || ExternalGraph == null) {
                    return true;
                }
                if (_completed) {
                    return true;
                }
                ExternalGraph.Update(dt);
                return !ExternalGraph.IsActive;
            }

            public override void Dispose() {
                base.Dispose();
                for (int i = 0; i < _externalGraphs.Length; i++) {
                    _externalGraphs[i].Dispose();
                    _externalGraphs[i] = null;
                }
                _externalGraphs = null;
            }
        }
    }
}
