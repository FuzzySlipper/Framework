using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CheckTargetHitPhase : ActionPhases {
        [SerializeField, DropdownList(typeof(Defenses), "GetValues")] private string _targetDefense;
        [SerializeField, DropdownList(typeof(Attributes), "GetValues")] private string _bonusStat;

        public override bool CanResolve(ActionCommand cmd) {
            var target = cmd.Owner.Target.TargetChar != null ? cmd.Owner.Target.TargetChar : cmd.Owner;
            cmd.CheckHit(_targetDefense, _bonusStat, target);
            return true;
        }
    }
}
