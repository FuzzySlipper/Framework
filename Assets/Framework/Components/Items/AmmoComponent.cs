using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class AmmoComponent : ComponentBase {

        private static GameOptions.CachedFloat _skillPercent = new GameOptions.CachedFloat("SkillAmmoReductionPerPoint");
        private static GameOptions.CachedFloat _skillMaxReduction = new GameOptions.CachedFloat("SkillAmmoMaxMultiplier");

        public IntValueHolder Amount = new IntValueHolder();
        public AmmoTemplate Template { get; }
        public float RepairSpeedPercent { get; }
        public float ReloadSpeed { get { return Template.ReloadSpeed * RepairSpeedPercent; } }
        private BaseStat _damageModStat;
        private float _damagePercent;
        private string _damageModId;
        private string _skill;

        public AmmoComponent(AmmoTemplate template, string skill, float repairSpeed, BaseStat damageModStat, float damagePercent = 0f) {
            Template = template;
            RepairSpeedPercent = repairSpeed;
            _skill = skill;
            _damageModStat = damageModStat;
            _damagePercent = damagePercent;
            if (_damageModStat != null) {
                Amount.OnResourceChanged += CheckMod;
            }
        }

        public bool CanLoadAmmo() {
            if (Amount.Value == Amount.MaxValue) {
                return false;
            }
            for (int i = 0; i < Template.Cost.Count; i++) {
                if (Player.GetCurrency(Template.Cost[i].Key).Value < Template.Cost[i].Value) {
                    return false;
                }
            }
            return true;
        }

        public bool TryLoadOneAmmo(Entity context) {
            if (!CanLoadAmmo()) {
                return false;
            }
            var skillMulti = 1f;
            if (!string.IsNullOrEmpty(_skill) && context.Stats.GetValue(_skill, out var skillValue)) {
                skillMulti = Mathf.Clamp(1 - (skillValue * _skillPercent.Value), _skillMaxReduction, 1);
            }
            for (int i = 0; i < Template.Cost.Count; i++) {
                Player.GetCurrency(Template.Cost[i].Key).ReduceValue(Template.Cost[i].Value * skillMulti);
            }
            Amount.AddToValue(1);
            return true;
        }

        private void CheckMod() {
            if (Amount > 0) {
                if (!string.IsNullOrEmpty(_damageModId)) {
                    _damageModStat.RemoveMod(_damageModId);
                }
                return;
            }
            if (!string.IsNullOrEmpty(_damageModId)) {
                _damageModId = _damageModStat.AddValueMod(-(_damageModStat.BaseValue * _damagePercent));
            }
        }
    }
}
