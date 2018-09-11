using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class DamageBonusModifier : ItemAttackModifier {

        [SerializeField] private FloatReference _dmgBonus = new FloatReference(5);
        [SerializeField] private bool _scaling = false;
        [SerializeField] private IntRange _scaleRange = new IntRange();

        public override void Init(int level, Entity item) {
            var weapon = item.Get<Weapon>();
            if (weapon == null) {
                return;
            }
            if (IsMagic) {
                item.Get<Equipment>()?.SetMagic();
            }
            var damageStat = item.Get<GenericStats>().Get(Stats.Power) as RangeStat;
            if (damageStat == null) {
                return;
            }
            if (!_scaling) {
                damageStat.SetValue(_dmgBonus);
                return;
            }
            if (_dmgBonus.RangeVariable != null) {
                var percent = Mathf.InverseLerp(_scaleRange.Min, _scaleRange.Max, level);
                damageStat.SetValue(_dmgBonus.RangeVariable.Lerp(percent));
            }
            else {
                damageStat.SetValue(_dmgBonus);
            }
        }
    }
}
