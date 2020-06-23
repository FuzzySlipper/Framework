using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class AmmoComponent : IComponent {

        public IntValueHolder Amount = new IntValueHolder();
        public AmmoConfig Config { get; }
        public float RepairSpeedPercent { get; }
        public CachedStat<BaseStat> DamageModStat;
        public float DamagePercent;
        public string DamageModId;
        public string Skill;
        public float ReloadSpeed { get { return Config.ReloadSpeed * RepairSpeedPercent; } }

        public AmmoComponent(AmmoConfig config, string skill, float repairSpeed, BaseStat damageModStat, float damagePercent = 0f) {
            Config = config;
            RepairSpeedPercent = repairSpeed;
            Skill = skill;
            DamagePercent = damagePercent;
            if (damageModStat == null) {
                return;
            }
            DamageModStat = new CachedStat<BaseStat>(damageModStat);
        }

        public AmmoComponent(SerializationInfo info, StreamingContext context) {
            Amount = info.GetValue(nameof(Amount), Amount);
<<<<<<< HEAD
            Config = ItemFactory.GetData(info.GetValue(nameof(Config), Config.ID)) as AmmoConfig;
=======
>>>>>>> FirstPersonAction
            RepairSpeedPercent = info.GetValue(nameof(RepairSpeedPercent), RepairSpeedPercent);
            DamageModStat = info.GetValue(nameof(DamageModStat), DamageModStat);
            DamagePercent = info.GetValue(nameof(DamagePercent), DamagePercent);
            DamageModId = info.GetValue(nameof(DamageModId), DamageModId);
            Skill = info.GetValue(nameof(Skill), Skill);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Amount), Amount);
            info.AddValue(nameof(Config), Config.ID);
            info.AddValue(nameof(RepairSpeedPercent), RepairSpeedPercent);
            info.AddValue(nameof(DamageModStat), DamageModStat);
            info.AddValue(nameof(DamagePercent), DamagePercent);
            info.AddValue(nameof(DamageModId), DamageModId);
            info.AddValue(nameof(Skill), Skill);
        }

        
    }
}
