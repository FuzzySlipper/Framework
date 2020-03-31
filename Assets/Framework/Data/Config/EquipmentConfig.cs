using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EquipmentConfig : ItemConfig {
        public string Skill;
        public int RequiredSkillRank = 0;
        public string EquipmentSlot;
        public int[] AttributeBonuses = new int[0];
        public int[] DefenseBonuses = new int[0];
        public override void AddComponents(Entity entity) {
            base.AddComponents(entity);
            entity.Add(new SkillRequirement(Skill, RequiredSkillRank));
            var equipment = entity.Add(new Equipment(EquipmentSlot));
            var stats = entity.GetOrAdd<StatsContainer>();
            for (int i = 0; i < AttributeBonuses.Length; i++) {
                var attributeBonus = AttributeBonuses[i];
                if (attributeBonus == 0) {
                    continue;
                }
                equipment.StatsToEquip.Add(Attributes.GetID(i));
                stats.Get(Attributes.GetID(i)).ChangeBase(AttributeBonuses[i]);
            }
            for (int i = 0; i < DefenseBonuses.Length; i++) {
                var defBonus = DefenseBonuses[i];
                if (defBonus == 0) {
                    continue;
                }
                equipment.StatsToEquip.Add(Defenses.GetID(i));
                stats.Get(Defenses.GetID(i)).ChangeBase(DefenseBonuses[i]);
            }
        }
    }
}
