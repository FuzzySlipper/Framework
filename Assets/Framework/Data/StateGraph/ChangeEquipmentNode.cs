using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class ChangeEquipmentNode : StateGraphNode {
        public float Delay = 0f;
        
        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR

            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(this);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            UnityEditor.EditorGUILayout.PropertyField(so.FindProperty(nameof(Delay)), GUIContent.none, true);
            so.ApplyModifiedProperties();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
#endif
            return false;
        }

        public override string Title { get { return "Change Equipment"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }

        private class RuntimeNode : RuntimeStateNode {

            private ChangeEquipmentNode _changeNode;

            public RuntimeNode(ChangeEquipmentNode node, RuntimeStateGraph graph) : base(node, graph) {
                _changeNode = node;
            }

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
                if (base.TryComplete(dt)) {
                    return true;
                }
                if (TimeManager.Time < TimeEntered + _changeNode.Delay) {
                    return false;
                }
                return true;
            }
        }
    }
}
