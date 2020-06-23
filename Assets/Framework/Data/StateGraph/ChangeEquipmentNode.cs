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
                // if (readyActions == null) {
                //     return;
                // }
                // var actionConfig = readyActions.QueuedChange;
                // var targetIndex = readyActions.QueuedSlot;
                // if (actionConfig == null || readyActions.GetAction(targetIndex) == actionConfig) {
                //     readyActions.RemoveAction(targetIndex);
                //     actionConfig = null;
                // }
                // else {
                //     readyActions.EquipAction(actionConfig, targetIndex);
                // }
                // if (targetIndex == 0) {
                //     Graph.SetVariable(GraphVariables.Equipment, actionConfig != null ? actionConfig.Config.AnimationTrigger : "");
                //     Graph.SetVariable(GraphVariables.WeaponModel, actionConfig != null ? actionConfig.Config.WeaponModel : "");
                // }
                // readyActions.QueuedChange = null;
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
