using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    public class StatBonusModifier : ItemEquipmentModifier {


        [SerializeField] private string _stat = Stats.BonusPowerMelee;
        [SerializeField] private string _statName;
        [SerializeField] private bool _scaling = false;
        [SerializeField] private IntRange _scaleRange = new IntRange();
        [SerializeField] private FloatReference _bonus = new FloatReference(5);

        public override void Init(int level, Entity item) {
            if (IsMagic) {
                item.Get<Equipment>(m => m.SetMagic());
            }
        }

        public override void EquipDescription(Equipment item, StringBuilder sb) {
            sb.Append("<b>Bonus:</b> +");
            sb.Append(GetAmount(item.Get<EntityLevelComponent>()?.Level ?? 1).ToString("F0"));
            sb.Append(" ");
            sb.NewLineAppend(_statName);
        }

        public override void SetEquipped(Equipment item, bool equipped) {
            if (item.EquipmentSlot == null) {
                return;
            }
            var statC = item.EquipmentSlot.SlotOwner.GetEntity()?.Get<GenericStats>();
            if (statC == null) {
                return;
            }
            var stat = statC.Get(_stat);
            if (stat == null) {
                return;
            }
            if (equipped) {
                stat.AddValueMod(new BaseStat.StatValueMod(GetAmount(item.Get<EntityLevelComponent>()?.Level ?? 1), item.Template.Id + GetInstanceID()));
            }
            else {
                stat.RemoveMod(item.Template.Id + GetInstanceID());
            }
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
