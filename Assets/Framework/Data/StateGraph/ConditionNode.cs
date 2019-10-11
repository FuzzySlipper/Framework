using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PixelComrades {
    public class ConditionNode : StateGraphNode {
        
        public override int OutputMax { get { return MaxConnections; } }

        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
            return false;
        }
        
        public override string Title { get { return "Condition"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeConditionNode(this, graph);
        }

        public class RuntimeConditionNode : RuntimeStateNode {

            public RuntimeConditionNode(ConditionNode node, RuntimeStateGraph graph) : base(node, graph) {}

            public override bool TryComplete(float dt) {
                return HasTrueCondition();
            }
        }
    }
}
