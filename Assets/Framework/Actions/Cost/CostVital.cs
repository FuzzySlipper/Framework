using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace PixelComrades {
    [Serializable]
    public class CostVital : CommandCost, ISerializable {

        public static GameOptions.CachedFloat SkillPercent = new GameOptions.CachedFloat("SkillVitalCostReductionPerPoint");
        public static GameOptions.CachedFloat SkillMaxReduction = new GameOptions.CachedFloat("SkillVitalCostMaxMultiplier");
        
        public float VitalAmount;
        public string TargetVital;

        private string _skill;

        public CostVital(string targetVital, float vitalAmount, string skill) {
            VitalAmount = vitalAmount;
            TargetVital = targetVital;
            _skill = skill;
        }

        public CostVital(SerializationInfo info, StreamingContext context) {
            VitalAmount = info.GetValue(nameof(VitalAmount), VitalAmount);
            TargetVital = info.GetValue(nameof(TargetVital), TargetVital);
            _skill = info.GetValue(nameof(_skill), _skill);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(VitalAmount), VitalAmount);
            info.AddValue(nameof(TargetVital), TargetVital);
            info.AddValue(nameof(_skill), _skill);
        }

        public override void ProcessCost(ActionTemplate action, CharacterTemplate owner) {
            var vital = owner.Stats.Get<VitalStat>(TargetVital);
            if (vital == null) {
                return;
                
            }
            var skillMulti = 1f;
            if (!string.IsNullOrEmpty(_skill)) {
                var skillValue = owner.Stats.GetValue(_skill);
                skillMulti = Mathf.Clamp(1 - (skillValue * SkillPercent.Value), SkillMaxReduction, 1);
            }
            vital.Current -= VitalAmount * skillMulti;
        }

        public override bool CanAct(ActionTemplate action, CharacterTemplate owner) {
            var stat = owner.Stats.Get<VitalStat>(TargetVital);
            if (stat != null && stat.Current >= VitalAmount) {
                return true;
            }
            if (stat == null) {
                owner.Post(new StatusUpdate(owner, "No stat " + TargetVital));
                return false;
            }
            //entity.Get<StatusUpdateComponent>(e => e.Status = string.Format("Not enough {0}", Vitals.GetDescriptionAt(TargetVital)));
            owner.Post(new StatusUpdate(owner, "Not enough " + TargetVital));
            return false;
        }
    }
}
