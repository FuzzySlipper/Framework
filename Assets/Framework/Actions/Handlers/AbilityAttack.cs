using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    
    [ActionProvider(Label)]
    public sealed class AbilityAttack : IActionProvider {

        private const string Label = "AbilityAttack";

        public void SetupEntity(Entity entity, SimpleDataLine lineData, DataEntry allData) {
            var damageStat = DiceStat.Parse(entity, Stats.Damage, lineData.Config);
            if (damageStat != null) {
                entity.Get<StatsContainer>().Add(damageStat);
            }
        }

        public void OnUsage(ActionEvent ae, ActionCommand cmd) {
            var prepareDamage = new PrepareDamageEvent(ae.Origin, ae.Target, cmd.Action, cmd.HitResult);
            var damageStat = cmd.Action.Stats.Get<DiceStat>(Stats.Damage);
            if (damageStat == null) {
                return;
            }
            var total = cmd.HitResult.Result == CollisionResult.CriticalHit ? damageStat.GetMax() : damageStat.Value;
            prepareDamage.Entries.Add(new DamageEntry(total, cmd.Action.Data.GetString(AbilityDataEntries.DamageType), Stats.Health, cmd
            .Action.GetName()));
            World.Get<RulesSystem>().Post(prepareDamage);
        }
    }
}
