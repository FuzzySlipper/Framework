using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class CompletedGraphNode : StateGraphNode {
        public override int OutputMin { get => 0; }
        public override int OutputMax { get => 0; }
        public override int MaxConditions { get => 0; }

        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
            return false;
        }

        public override string Title { get { return "End"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }

        public class RuntimeNode : RuntimeStateNode {

            public override RuntimeStateNode GetExitNode() {
                return null;
            }

            public RuntimeNode(CompletedGraphNode node, RuntimeStateGraph graph) : base(node,graph) {}


            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                Graph.GraphCompleted();
            }

            public override bool TryComplete(float dt) {
                return true;
            }
        }
    }
}
