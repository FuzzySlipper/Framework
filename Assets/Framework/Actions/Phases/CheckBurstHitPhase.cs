using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CheckBurstHitPhase : ActionPhases {

        [SerializeField, DropdownList(typeof(Defenses), "GetValues")] private string _targetDefense;
        [SerializeField, DropdownList(typeof(Attributes), "GetValues")] private string _bonusStat;
        [SerializeField] private int _radius;
        [SerializeField] private bool _checkRequirements;

        public override bool CanResolve(ActionCommand cmd) {
            var center = cmd.Owner.Position;
            for (int x = 0; x < _radius; x++) {
                for (int z = 0; z < _radius; z++) {
                    var pos = center + new Point3(x, 0, z);
                    var cell = Game.CombatMap.Get(pos);
                    if (cell.Unit == null || cell.Unit == cmd.Owner) {
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
