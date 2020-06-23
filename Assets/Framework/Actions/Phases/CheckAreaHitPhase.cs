using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    [System.Serializable]
    public class CheckAreaHitPhase : ActionPhases {

        [SerializeField, DropdownList(typeof(Defenses), "GetValues")]
        private string _targetDefense = Defenses.Armor;
        [SerializeField, DropdownList(typeof(Attributes), "GetValues")]
        private string _bonusStat = Attributes.Insight;
        
        [SerializeField] private int _radius = 1;
        [SerializeField] private bool _checkRequirements = false;

        public override bool CanResolve(ActionCommand cmd) {
            var center = cmd.Owner.Target.GetPositionP3;
            for (int x = 0; x < _radius; x++) {
                for (int z = 0; z < _radius; z++) {
                    var pos = center + new Point3(x, 0, z);
                    var cell = Game.CombatMap.Get(pos);
                    if (cell.Unit == null) {
                        continue;
                    }
                    if (_checkRequirements && !cmd.Action.Config.CanEffect(cmd.Action, cmd.Owner, cell.Unit)) {
                        continue;
                    }
                    cmd.CheckHit(_targetDefense, _bonusStat, cell.Unit);
                }
            }
            return true;
        }
    }
}
