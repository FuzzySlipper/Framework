using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    
    public static class EffectResolve {
        public static void SetupEffect(Entity entity, DataEntry baseData, DataEntry lineData, DataEntry effectData) {
            var config = effectData.Get<DataList>("Config");
            switch (effectData.GetValue<string>("ActionType")) {
                case "DamageImpact":
                    var actionFxName = TryFindString(baseData, config, DatabaseFields.ActionFX, "");
                    ActionFx actionFx = null;
                    if (!string.IsNullOrEmpty(actionFxName)) {
                        actionFx = ItemPool.LoadAsset<ActionFx>(UnityDirs.ActionFx, actionFxName);
                    }
                    string dmgType;
                    if (baseData.TryGetEnum(DatabaseFields.DamageType, out var index)) {
                        dmgType = GameData.DamageTypes[index];
                    }
                    else {
                        dmgType = TryFindString(baseData, config, DatabaseFields.DamageType, GameData.DamageTypes[0]);
                    }
                    var targetVital = TryFindString(baseData, config, "TargetVital", effectData.GetValue<string>("Target"));
                    //entity.GetOrAdd<ActionImpacts>().Add(new DamageImpact(dmgType, targetVital, Mathf.Clamp(lineData.TryGetValue(DatabaseFields.Amount, 1f),0 , 1), actionFx));
                    break;
                case "StatMod":
                    var targetStat = TryFindString(baseData, config, "TargetStat", effectData.GetValue<string>("Target"));
                    var amount = effectData.TryGetValue(DatabaseFields.Amount, 1f);
                    entity.Stats.GetOrAdd(targetStat).AddToBase(amount);
                    entity.Get<Equipment>(e => e.AddStat(targetStat));
                    break;
            }
        }

        private static string TryFindString(DataEntry baseData, DataList config, string field, string defaultValue) {
            var data = baseData.GetValue<string>(field);
            if (string.IsNullOrEmpty(data)) {
                data = FindData(config, field);
            }
            if (string.IsNullOrEmpty(data)) {
                return defaultValue;
            }
            return data;
        }

        private static string FindData(DataList config, string key) {
            return config.FindData<string, string>("Key", key, "Data");
            //for (int i = 0; i < config.Value.Count; i++) {
            //    if (config.Value[i].GetValue<string>("Key") == key) {
            //        return config.Value[i].GetValue<string>("Data");
            //    }
            //}
            //return "";
        }
    }
}
