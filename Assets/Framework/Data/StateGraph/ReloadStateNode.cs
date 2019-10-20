using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class ReloadStateNode : StateGraphNode {
        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
            return false;
        }

        public override string Title { get { return "Reloading"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }

        public class RuntimeNode : RuntimeStateNode {

            private AmmoComponent _ammo;
            private int _totalAmmo;
            private float _reloadPerAmmo;
            private int _current;
            private float _reloadTimer;
            
            public RuntimeNode(ReloadStateNode node, RuntimeStateGraph graph) : base(node, graph) {}

            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                Graph.SetVariable(GraphVariables.Reloading, true);
                _ammo = Graph.Entity.Get<CurrentAction>().Value?.Ammo;
                if (_ammo == null) {
                    return;
                }
                UIChargeCircle.ManualStart(_ammo.Config.ReloadText);
                _reloadPerAmmo = _ammo.ReloadSpeed / _ammo.Amount.MaxValue;
                _totalAmmo = _ammo.Amount.MaxValue - _ammo.Amount.Value;
                _current = 0;
                _reloadTimer = 0;
            }

            public override void OnExit() {
                base.OnExit();
                Graph.SetVariable(GraphVariables.Reloading, false);
                UIChargeCircle.StopCharge();
            }

            public override bool TryComplete(float dt) {
                if (base.TryComplete(dt) || _ammo == null) {
                    return true;
                }
                if (!Graph.GetVariable<bool>(GraphVariables.Reloading)) {
                    return true;
                }
                if (_reloadTimer > 0) {
                    _reloadTimer -= dt;
                    return false;
                }
                _current++;
                UIChargeCircle.ManualSetPercent((float) _current / _totalAmmo);
                _reloadTimer = _reloadPerAmmo;
                if (!_ammo.TryLoadOneAmmo(Graph.Entity)) {
                    return true;
                }
                return false;
            }
        }
    }
}