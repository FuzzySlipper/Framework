using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
    public sealed class BasicAttackHandler : ActionHandler {

        public override void SetupEntity(Entity entity) { }

        public override void OnUsage(ActionEvent ae, ActionCommand cmd) {
            var total = RulesSystem.CalculateDamageTotal(cmd.Owner.Stats.Get<DiceStat>(Stat.AttackDamage), cmd, cmd.Owner.GetAttackDamageBonusStatName());
            if (total <= 0) {
                return;
            }
            var prepareDamage = new PrepareDamageEvent(ae.Origin, ae.Target, cmd.Action, cmd.HitResult);
            prepareDamage.Entries.Add(new DamageEntry(total, DamageTypes.Physical, Stat.Health, "Attack"));
            World.Get<RulesSystem>().Post(prepareDamage);
        }
    }
}