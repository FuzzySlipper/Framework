using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class WeaponBobNode : StateGraphNode {
        [Range(0, 0.1f)] [SerializeField] public float VerticalSwayAmount = 0.025f;
        [Range(0, 0.1f)] [SerializeField] public float HorizontalSwayAmount = 0.075f;
        [Range(0, 15f)] [SerializeField] public float SwaySpeed = 3f;

        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR

            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(this);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            UnityEditor.EditorGUILayout.LabelField("Vertical");
            UnityEditor.EditorGUILayout.PropertyField(so.FindProperty(nameof(VerticalSwayAmount)), GUIContent.none, true);
            UnityEditor.EditorGUILayout.LabelField("Horizontal");
            UnityEditor.EditorGUILayout.PropertyField(so.FindProperty(nameof(HorizontalSwayAmount)), GUIContent.none, true);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            UnityEditor.EditorGUILayout.LabelField("Sway");
            UnityEditor.EditorGUILayout.PropertyField(so.FindProperty(nameof(SwaySpeed)), GUIContent.none, true);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            so.ApplyModifiedProperties();
#endif
            return false;
        }

        public override string Title { get { return "WeaponBobNode"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }

        private class RuntimeNode : RuntimeStateNode {
            private GameOptions.CachedBool _useWeaponBob = new GameOptions.CachedBool("UseWeaponBob");
            
            private WeaponBobNode _node;
            private WeaponBobComponent _component;
            
            public RuntimeNode(WeaponBobNode node, RuntimeStateGraph graph) : base(node, graph) {
                _node = node;
                _component = graph.Entity.Get<WeaponBobComponent>();
            }

            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                if (_component == null) {
                    _component = Graph.Entity.Get<WeaponBobComponent>();
                }
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
                if (!_useWeaponBob) {
                    return false;
                }
                _component.BobTime += dt;
                var velocity = Player.FirstPersonController.VelocityPercent;
                var y = _node.VerticalSwayAmount * Mathf.Sin((_node.SwaySpeed * 2) * _component.BobTime) * velocity;
                var x = _node.HorizontalSwayAmount * Mathf.Sin(_node.SwaySpeed * _component.BobTime) * velocity;
                _component.ArmsPivot.localPosition = _component.ResetPoint + new Vector3(x, y, 0);
                return false;
            }
        }
    }
}
