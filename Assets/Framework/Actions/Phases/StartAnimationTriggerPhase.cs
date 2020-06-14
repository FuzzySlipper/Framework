using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class StartAnimationTriggerPhase : ActionPhases {
        [SerializeField, DropdownList(typeof(GraphTriggers), "GetValues")] private string _animation;

        public override bool CanResolve(ActionCommand cmd) {
            cmd.Owner.AnimGraph.TriggerGlobal(_animation);
            return true;
        }
    }
}
