using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class ExternalGraphNode : StateGraphNode {
        public StateGraph ExternalGraph;

        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            ExternalGraph = UnityEditor.EditorGUILayout.ObjectField(ExternalGraph, typeof(StateGraph), false) as
                StateGraph;
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
#endif
            return false;
        }

        public override string Title { get { return ExternalGraph != null ? ExternalGraph.name : "Graph"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeExternalGraphNode(this, graph);
        }

        public class RuntimeExternalGraphNode : RuntimeStateNode {
            
            private RuntimeStateNode _exitNode;
            private RuntimeStateGraph _runtimeGraph;
            private bool _completed;
            
            public override RuntimeStateNode GetExitNode() {
                return _exitNode;
            }

            public RuntimeExternalGraphNode(ExternalGraphNode node, RuntimeStateGraph graph) : base(node,graph) {
                _exitNode = GetOriginalNodeExit();
                _runtimeGraph = node.ExternalGraph.GetRuntimeGraph(graph.Entity);
                _runtimeGraph.OnComplete += ExternalGraphCompleted;
            }

            private void ExternalGraphCompleted() {
                _completed = true;
            }

            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                _completed = false;
                _runtimeGraph.Start();
            }

            public override void OnExit() {
                base.OnExit();
                _runtimeGraph.Stop();
            }

            public override bool TryComplete(float dt) {
                if (_completed) {
                    return true;
                }
                _runtimeGraph.Update(dt);
                return !_runtimeGraph.IsActive;
            }
        }
    }
}
