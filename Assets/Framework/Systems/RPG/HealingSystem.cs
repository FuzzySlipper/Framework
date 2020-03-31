using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class HealingSystem : SystemBase, IRuleEventRun<HealingEvent> {

        public HealingSystem() {
            World.Get<RulesSystem>().AddHandler<HealingEvent>(this);
        }

        public void RuleEventRun(ref HealingEvent context) {
            var stats = context.Target.Stats;
            var vital = stats.GetVital(context.TargetVital);
            if (vital == null) {
                vital = stats.GetVital(context.TargetVital);
            }
            if (vital != null) {
                vital.Current += context.Amount;
                if (context.Amount > 0) {
                    Color color = context.TargetVital == Stats.Health ? Color.green : Color.yellow;
                    context.Target.Post(new CombatStatusUpdate(context.Target, context.Amount.ToString("F1"), color));
                }
            }
        }
    }
}
