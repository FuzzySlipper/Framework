using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class StartAnimationTriggerPhase : ActionPhases {
        [SerializeField, DropdownList(typeof(GraphTriggers), "GetValues")] private string _animation = GraphTriggers.Attack;

        public override bool CanResolve(ActionCommand cmd) {
            cmd.Owner.AnimGraph.TriggerGlobal(_animation);
            return true;
        }

        public override string ToString() {
            if (!string.IsNullOrEmpty(_animation)) {
                return "StartAnimationTriggerPhase " + _animation;
            }
            return base.ToString();
        }
    }
}
