using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class HealingSystem : SystemBase, IReceive<ImpactEvent>, IReceiveGlobal<HealingEvent> {

        public HealingSystem() {
            EntityController.RegisterReceiver(
                new EventReceiverFilter(
                    this, new[] {
                        typeof(HealImpact)
                    }));
        }

        public void HandleGlobal(HealingEvent arg) {
            var entity = arg.Target;
            var stats = entity.Get<StatsContainer>();
            var vital = stats.GetVital(arg.TargetVital);
            if (vital == null) {
                vital = stats.GetVital(GameData.Vitals.GetID(arg.TargetVital));
            }
            if (vital != null) {
                vital.Current += arg.Amount;
                if (arg.Amount > 0) {
                    Color color = arg.TargetVital == Stats.Health ? Color.green : Color.yellow;
                    entity.Post(new CombatStatusUpdate(entity, arg.Amount.ToString("F1"), color));
                }
            }
        }

        public void Handle(ImpactEvent arg) {
            if (arg.Hit <= 0) {
                return;
            }
            var component = arg.Source.Find<HealImpact>();
            var sourceEntity = component.GetEntity();
            var stats = sourceEntity.Get<StatsContainer>();
            if (component == null || stats == null) {
                return;
            }
            var target = component.HealSelf ? arg.Origin : arg.Target;
            var power = RulesSystem.CalculateTotal(stats, Stats.Power, component.NormalizedPercent);
            var logSystem = World.Get<GameLogSystem>();
            logSystem.StartNewMessage(out var logMsg, out var hoverMsg);
            logMsg.Append(arg.Origin.GetName());
            logMsg.Append(" healed ");
            logMsg.Append(target.GetName());
            logMsg.Append(" for ");
            logMsg.Append(power.ToString("F0"));
            hoverMsg.Append(RulesSystem.LastQueryString);
            logSystem.PostCurrentStrings(GameLogSystem.DamageColor);
            target.Post(new HealingEvent(power, arg.Origin, target, component.TargetVital));
        }
    }
}
