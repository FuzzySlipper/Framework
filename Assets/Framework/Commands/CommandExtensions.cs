using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public enum ActionCommandType {
        Attack,
        WeaponAttack,
        WeaponAttackAbility,
        WeaponAttackStealth,
        WeaponAttackDeath,
        Heal,
        SetModifier,
        ConvertVital,
        CreateScroll,
        DefendOthers,
        RaiseDead,
        RepairWeaponJam,
        Unlock,
        RemoveMod,
        DamageOverTime,
        AttackWithMods,
        Teleport,
        InstantKill,
    }

    public static class CommandExtensions {

        public static IActionImpact[] GetActionImpacts(this ActionCommandType commandType, Entity entity, DataEntry data) {
            var list = new List<IActionImpact>();
            var stat = entity.Get<StatsContainer>().Get(Stats.Power);
            switch (commandType) {
                case ActionCommandType.Attack:
                case ActionCommandType.WeaponAttackAbility:
                case ActionCommandType.WeaponAttackStealth:
                case ActionCommandType.WeaponAttackDeath:
                case ActionCommandType.AttackWithMods:
                    list.Add(new DamageImpact(data.GetValue<string>("DamageType"), "Vitals.Health", 1, stat));
                    break;
                case ActionCommandType.ConvertVital:
                    list.Add(
                        new ConvertVitalImpact(data.GetValue<float>("Percent"), data.GetValue<string>("SourceVital"), data.GetValue<string>("TargetVital")));
                    break;
            }
            switch (commandType) {
                case ActionCommandType.InstantKill:
                case ActionCommandType.WeaponAttackDeath:
                    list.Add(new InstantKill(data.TryGetValue("Chance", 0.15f)));
                    break;
            }
            return list.ToArray();
            //entity.GetOrAdd<ActionImpacts>().AddRange(list);
        }
        
    }
}
