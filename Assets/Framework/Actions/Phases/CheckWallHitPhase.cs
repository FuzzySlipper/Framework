using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CheckWallHitPhase : ActionPhases {

        [SerializeField, DropdownList(typeof(Defenses), "GetValues")] private string _targetDefense;
        [SerializeField, DropdownList(typeof(Attributes), "GetValues")] private string _bonusStat;
        [SerializeField] private int _radius;
        [SerializeField] private int _axisDirection;
        [SerializeField] private bool _checkRequirements;

        public override bool CanResolve(ActionCommand cmd) {
            var center = cmd.Owner.Target.GetPositionP3;
            for (int i = 0; i < _radius; i++) {
                var pos = center;
                pos[_axisDirection] += i;
                var cell = Game.CombatMap.Get(pos);
                if (cell.Unit == null) {
                    continue;
                }
                if (_checkRequirements && !cmd.Action.Config.CanEffect(cmd.Action, cmd.Owner, cell.Unit)) {
                    continue;
                }
                cmd.CheckHit(_targetDefense, _bonusStat, cell.Unit);
            }
            return true;
        }
    }
}
