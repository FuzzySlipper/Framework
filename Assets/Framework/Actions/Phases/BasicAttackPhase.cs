using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
    public class BasicAttackPhase : ActionPhases {
        [SerializeField, DropdownList(typeof(Defenses), "GetValues")]
        private string _targetDefense = Defenses.Armor;
        public override bool CanResolve(ActionCommand cmd) {
            var target = cmd.Owner.Target.TargetChar != null ? cmd.Owner.Target.TargetChar : cmd.Owner;
            cmd.CheckBasicAttackHit(target, _targetDefense);
            return true;
        }
    }
}
