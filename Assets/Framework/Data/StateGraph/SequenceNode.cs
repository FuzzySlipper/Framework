using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SequenceNode : StateGraphNode {
        public GenericSequence Sequence;
        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            Sequence = UnityEditor.EditorGUILayout.ObjectField(Sequence, typeof(GenericSequence), false) as 
            GenericSequence;
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
#endif
            return false;
        }

        public override string Title { get { return Sequence != null ? Sequence.name : "Sequence"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeSequenceNode(this, graph);
        }

        private class RuntimeSequenceNode : RuntimeStateNode {
            private RuntimeSequence _sequence;
            public override string DebugInfo {
                get {
                    return string.Format(
                        "Time {0:F3} Remaining {1:F2}", _sequence.CurrentTime,
                        _sequence.Remaining);
                }
            }
            public RuntimeSequenceNode(SequenceNode node, RuntimeStateGraph graph) : base(node,graph) {
                _sequence = node.Sequence.GetRuntimeSequence(graph.Entity);
            }


            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                _sequence.Play();
            }

            public override bool TryComplete(float dt) {
                if (base.TryComplete(dt)) {
                    return true;
                }
                _sequence.Update(dt);
                return _sequence.IsComplete;
            }
        }
    }
}
