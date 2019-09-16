using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class AmmoComponent : IComponent {

        private static GameOptions.CachedFloat _skillPercent = new GameOptions.CachedFloat("SkillAmmoReductionPerPoint");
        private static GameOptions.CachedFloat _skillMaxReduction = new GameOptions.CachedFloat("SkillAmmoMaxMultiplier");

        public IntValueHolder Amount = new IntValueHolder();
        public AmmoTemplate Template { get; }
        public float RepairSpeedPercent { get; }
        private CachedStat<BaseStat> _damageModStat;
        private float _damagePercent;
        private string _damageModId;
        private string _skill;
        public float ReloadSpeed { get { return Template.ReloadSpeed * RepairSpeedPercent; } }

        public AmmoComponent(AmmoTemplate template, string skill, float repairSpeed, BaseStat damageModStat, float damagePercent = 0f) {
            Template = template;
            RepairSpeedPercent = repairSpeed;
            _skill = skill;
            _damageModStat = new CachedStat<BaseStat>(damageModStat);
            _damagePercent = damagePercent;
            if (_damageModStat != null) {
                Amount.OnResourceChanged += CheckMod;
            }
        }

        public AmmoComponent(SerializationInfo info, StreamingContext context) {
            Amount = info.GetValue(nameof(Amount), Amount);
            Template = AmmoFactory.GetTemplate(info.GetValue(nameof(Template), Template.ID));
            RepairSpeedPercent = info.GetValue(nameof(RepairSpeedPercent), RepairSpeedPercent);
            _damageModStat = info.GetValue(nameof(_damageModStat), _damageModStat);
            _damagePercent = info.GetValue(nameof(_damagePercent), _damagePercent);
            _damageModId = info.GetValue(nameof(_damageModId), _damageModId);
            _skill = info.GetValue(nameof(_skill), _skill);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Amount), Amount);
            info.AddValue(nameof(Template), Template.ID);
            info.AddValue(nameof(RepairSpeedPercent), RepairSpeedPercent);
            info.AddValue(nameof(_damageModStat), _damageModStat);
            info.AddValue(nameof(_damagePercent), _damagePercent);
            info.AddValue(nameof(_damageModId), _damageModId);
            info.AddValue(nameof(_skill), _skill);
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
            if (!string.IsNullOrEmpty(_skill) && context.Get<StatsContainer>().GetValue(_skill, out var skillValue)) {
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
                    _damageModStat.Stat.RemoveMod(_damageModId);
                }
                return;
            }
            if (!string.IsNullOrEmpty(_damageModId)) {
                _damageModId = _damageModStat.Stat.AddValueMod(-(_damageModStat.Stat.BaseValue * _damagePercent));
            }
        }
    }
}
