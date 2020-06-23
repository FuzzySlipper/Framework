using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class HealRuleTemplate : RuleTemplate, IRuleEventEnded<ImpactEvent> {

        private CachedComponent<HealImpact> _heal = new CachedComponent<HealImpact>();

        public override List<CachedComponent> GatherComponents
            => new List<CachedComponent>() {
                _heal, EntityStats
            };


        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(HealImpact),
            };
        }

        public void RuleEventEnded(ref ImpactEvent context) {
            var target = _heal.Value.HealSelf ? context.Origin : context.Target;
            var power = RulesSystem.CalculateImpactTotal(EntityStats, PixelComrades.Stat.Power, _heal.Value.NormalizedPercent);
            var logSystem = World.Get<GameLogSystem>();
            logSystem.StartNewMessage(out var logMsg, out var hoverMsg);
            logMsg.Append(context.Origin.GetName());
            logMsg.Append(" healed ");
            logMsg.Append(target.GetName());
            logMsg.Append(" for ");
            logMsg.Append(power.ToString("F0"));
            hoverMsg.Append(RulesSystem.LastQueryString);
            logSystem.PostCurrentStrings(GameLogSystem.DamageColor);
            World.Get<RulesSystem>().Post(new HealingEvent(context.Action, power, context.Origin, target, _heal.Value.TargetVital));
        }
    }
}