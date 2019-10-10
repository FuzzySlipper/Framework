using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class ReloadStateNode : StateGraphNode {
        public string Trigger;
        public bool ExitWhenTriggerActive;
        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            var animationLabels = AnimationEvents.GetNames().ToArray();
            var index = System.Array.IndexOf(animationLabels, Trigger);
            var newIndex = UnityEditor.EditorGUILayout.Popup("Event", index, animationLabels);
            if (newIndex >= 0) {
                Trigger = animationLabels[newIndex];
                UnityEditor.EditorUtility.SetDirty(this);
            }
            GUILayout.Label("Exit When Trigger Active", textStyle);
            ExitWhenTriggerActive = UnityEditor.EditorGUILayout.Toggle(ExitWhenTriggerActive, textStyle);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
#endif
            return false;
        }

        public override string Title { get { return "Reload"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }

        public class RuntimeNode : RuntimeStateNode {

            private ReloadStateNode _originalNode;
            private RuntimeStateNode _exitNode;
            private ReloadWeaponComponent _reload;
            private int _totalAmmo;
            private float _reloadPerAmmo;
            private int _current;
            private float _reloadTimer;
            
            public override RuntimeStateNode GetExitNode() {
                return _exitNode;
            }

            public RuntimeNode(ReloadStateNode node, RuntimeStateGraph graph) : base(node, graph) {
                _exitNode = GetOriginalNodeExit();
                _originalNode = node;
            }

            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                Graph.ResetTrigger(AnimationEvents.ReloadStop);
                _reload = Graph.Entity.Get<ReloadWeaponComponent>();
                UIChargeCircle.ManualStart(_reload.Ammo.Template.ReloadText);
                _reloadPerAmmo = _reload.Ammo.ReloadSpeed / _reload.Ammo.Amount.MaxValue;
                _totalAmmo = _reload.Ammo.Amount.MaxValue - _reload.Ammo.Amount.Value;
                _current = 0;
                _reloadTimer = 0;
            }

            public override void OnExit() {
                base.OnExit();
                UIChargeCircle.StopCharge();
            }

            public override bool TryComplete(float dt) {
                if (!string.IsNullOrEmpty(_originalNode.Trigger)) {
                    var isActive = Graph.IsTriggerActive(_originalNode.Trigger);
                    if (isActive == _originalNode.ExitWhenTriggerActive) {
                        return true;
                    }
                }
                if (_reloadTimer > 0) {
                    _reloadTimer -= dt;
                    return false;
                }
                _current++;
                UIChargeCircle.ManualSetPercent((float) _current / _totalAmmo);
                _reloadTimer = _reloadPerAmmo;
                if (!_reload.Ammo.TryLoadOneAmmo(Graph.Entity)) {
                    return true;
                }
                return false;
            }
        }
    }
}