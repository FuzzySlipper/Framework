using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class GlobalConditionNode : ConditionNode {
        public override bool IsGlobal { get => true; }
        public override int InputMin { get => 0; }
        public override int InputMax { get => 0; }

        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
            return false;
        }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new GlobalConditionRuntimeNode(this, graph);
        }

        public class GlobalConditionRuntimeNode : RuntimeConditionNode, IGlobalRuntimeStateNode {
            public GlobalConditionRuntimeNode(GlobalConditionNode node, RuntimeStateGraph graph) : base(node, graph) {}

            public void CheckConditions() {
                if (HasTrueCondition()) {
                    var exitNode = GetConditionExitNode();
                    if (exitNode != null) {
                        Graph.ChangeNode(exitNode);
                    }
                }
            }

            public override bool TryComplete(float dt) {
                return true;
            }
        }
    }
}
