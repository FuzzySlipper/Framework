using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class WaitChargeNode : StateGraphNode {
        
        public float ForceRangeMin;
        public float ForceRangeMax;
        public float MaxChargeTime;
        public string ChargeInput;
        
        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
            return false;
        }

        public override string Title { get { return "Wait For Charge"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }

        public class RuntimeNode : RuntimeStateNode {
            
            private RuntimeStateNode _exitNode;
            private PlayerInputComponent _input;
            private WaitChargeNode _originalNode;
            
            public override RuntimeStateNode GetExitNode() {
                return _exitNode;
            }

            public RuntimeNode(WaitChargeNode node, RuntimeStateGraph graph) : base(node,graph) {
                _exitNode = GetOriginalNodeExit();
                _originalNode = node;
            }

            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                _input = Graph.Entity.Get<PlayerInputComponent>();
            }

            public override void OnExit() {
                base.OnExit();
            }

            public override bool TryComplete(float dt) {
                if (base.TryComplete(dt)) {
                    return true;
                }
                var elapsed = (TimeManager.Time - TimeEntered);
                if (elapsed >= _originalNode.MaxChargeTime || (_input != null && !_input.Handler.GetButton(_originalNode.ChargeInput))) {
                    var currentAction = Graph.Entity.Get<CurrentAction>().Value;
                    var chargeComponent = currentAction.Entity.GetOrAdd<ChargeComponent>();
                    chargeComponent.CurrentCharge = Mathf.Lerp(_originalNode.ForceRangeMin, _originalNode.ForceRangeMax, Mathf.Clamp01
                    (elapsed / _originalNode.MaxChargeTime));
                    return true;
                }
                return false;
            }
        }
    }
}
