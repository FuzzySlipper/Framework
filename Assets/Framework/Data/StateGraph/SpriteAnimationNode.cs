using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SpriteAnimationNode : StateGraphNode {
        public SpriteAnimation Animation;

        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            Animation = UnityEditor.EditorGUILayout.ObjectField(Animation, typeof(SpriteAnimation), false) as
                SpriteAnimation;
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
#endif
            return false;
        }

        public override string Title { get { return Animation != null ? Animation.name : "SpriteAnimation"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeSpriteAnimationNode(this, graph);
        }

        public class RuntimeSpriteAnimationNode : RuntimeStateNode {
            private RuntimeStateNode _exitNode;
            private RuntimeSequence _sequence;

            public override RuntimeStateNode GetExitNode() {
                return _exitNode;
            }

            public RuntimeSpriteAnimationNode(SpriteAnimationNode node, RuntimeStateGraph graph) : base(graph) {
                //_sequence = node.Sequence.GetRuntimeSequence(graph.Owner);
                var outNode = graph.OriginalGraph.GetConnectionEndpoint(node.OutPoints[0]);
                if (outNode != null) {
                    _exitNode = graph.GetRuntimeNode(outNode.Id);
                }
            }


            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                _sequence.Play();
            }

            public override bool TryComplete(float dt) {
                _sequence.Update(dt);
                return _sequence.IsComplete;
            }
        }
    }
}
