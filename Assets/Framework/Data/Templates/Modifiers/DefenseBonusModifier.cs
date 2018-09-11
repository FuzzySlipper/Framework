using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class DefenseBonusModifier : ItemModifier {

        [SerializeField] private FloatReference _bonus = new FloatReference(5);
        [SerializeField] private int _targetDamageType = DamageTypes.Physical;
        [SerializeField] private bool _useMulti = false;
        [SerializeField] private bool _scaling = false;
        [SerializeField] private IntRange _scaleRange = new IntRange();

        public override void Init(int level, Entity item) {
            var armor = item.Get<Armor>();
            if (armor == null) {
                return;
            }
            if (IsMagic) {
                armor.Get<Equipment>().SetMagic();
            }
            float amount = _useMulti ? armor.Defense[(int) _targetDamageType] * GetAmount(level) : GetAmount(level);
            armor.Defense[(int) _targetDamageType] += amount;
        }

        private float GetAmount(int level) {
            if (_bonus.RangeVariable == null || !_scaling) {
                return _bonus;
            }
            var percent = Mathf.InverseLerp(_scaleRange.Min, _scaleRange.Max, level);
            return _bonus.RangeVariable.Lerp(percent);
        }
    }
}
