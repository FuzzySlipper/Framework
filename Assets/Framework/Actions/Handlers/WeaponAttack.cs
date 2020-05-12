using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public static class WeaponAttack {
        public static void OnUsage(ActionEvent ae, ActionCommand cmd, string stat) {
            var prepareDamage = new PrepareDamageEvent(ae.Origin, ae.Target, cmd.Action, cmd.HitResult);
            var damageStat = cmd.Owner.Stats.Get<DiceStat>(stat);
            if (damageStat == null) {
                return;
            }
            var total = cmd.HitResult.Result == CollisionResult.CriticalHit ? damageStat.GetMax() : damageStat.Value;
            prepareDamage.Entries.Add(new DamageEntry(total, cmd.Action.Data.GetString(AbilityDataEntries.DamageType), Stats.Health, "Weapon Attack"));
            World.Get<RulesSystem>().Post(prepareDamage);
        }
    }
    
    [ActionProvider(Label)]
    public sealed class WeaponMeleeAttack : IActionProvider {

        private const string Label = "WeaponAttack";
        public void SetupEntity(Entity entity, SimpleDataLine lineData, DataEntry allData) { }

        public void OnUsage(ActionEvent ae, ActionCommand cmd) {
            WeaponAttack.OnUsage(ae, cmd, Stats.WeaponAttackDamage);
        }
    }
}
