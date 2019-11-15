using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class ChangeEquipmentNode : StateGraphNode {
        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
            return false;
        }

        public override string Title { get { return "Change Equipment"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }

        private class RuntimeNode : RuntimeStateNode {
            
            public RuntimeNode(ChangeEquipmentNode node, RuntimeStateGraph graph) : base(node, graph) {}

            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                var readyActions = Graph.Entity.Get<ReadyActions>();
                if (readyActions == null) {
                    return;
                }
                var actionConfig = readyActions.QueuedChange;
                var targetIndex = readyActions.QueuedSlot;
                if (actionConfig == null || readyActions.GetAction(targetIndex) == actionConfig) {
                    readyActions.RemoveAction(targetIndex);
                }
                else {
                    readyActions.EquipAction(actionConfig, targetIndex);
                }
                if (targetIndex == 0 && actionConfig != null) {
                    Graph.SetVariable(GraphVariables.Equipment, actionConfig.Config.EquipVariable);
                    Graph.SetVariable(GraphVariables.WeaponModel, actionConfig.Config.WeaponModel);
                }
                readyActions.QueuedChange = null;
            }

            public override void OnExit() {
                base.OnExit();
            }

            public override bool TryComplete(float dt) {
                base.TryComplete(dt);
                return true;
            }
        }
    }
}
