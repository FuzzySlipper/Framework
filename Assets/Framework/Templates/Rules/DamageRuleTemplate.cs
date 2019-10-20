using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class DamageRuleTemplate : RuleTemplate, IRuleEventRun<PrepareDamageEvent>, IRuleEventEnded<ImpactEvent> {
        
        private CachedComponent<DamageImpact> _damageImpact = new CachedComponent<DamageImpact>();
        
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _damageImpact, EntityStats
        };
        
        public void RuleEventRun(ref PrepareDamageEvent context) {
            var power = RulesSystem.CalculateImpactTotal(EntityStats, Stats.Power, _damageImpact.Value.NormalizedPercent);
            context.Entries.Add(new DamageEntry(power, _damageImpact.Value.DamageType, _damageImpact.Value.TargetVital,
                RulesSystem.LastQueryString.ToString()));
        }

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(DamageImpact),
            };
        }

        public void RuleEventEnded(ref ImpactEvent context) {
            World.Get<RulesSystem>().Post(new PrepareDamageEvent(context, context.Origin, context.Target));
        }
    }
}