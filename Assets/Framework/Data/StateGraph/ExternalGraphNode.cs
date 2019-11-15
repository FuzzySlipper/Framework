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
            return new RuntimeNode(this, graph);
        }

        public class RuntimeNode : RuntimeStateNode {
            
            public RuntimeStateGraph ExternalGraph;
            private bool _completed;
            
            public RuntimeNode(ExternalGraphNode node, RuntimeStateGraph graph) : base(node,graph) {
                ExternalGraph = node.ExternalGraph.GetRuntimeGraph(graph, graph.Entity);
                ExternalGraph.OnComplete += ExternalGraphCompleted;
            }

            private void ExternalGraphCompleted() {
                _completed = true;
            }

            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                _completed = false;
                ExternalGraph.Start();
            }

            public override void OnExit() {
                base.OnExit();
                ExternalGraph.Stop();
            }

            public override bool TryComplete(float dt) {
                if (base.TryComplete(dt)) {
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
                ExternalGraph.Dispose();
                ExternalGraph = null;
            }
        }
    }
}
