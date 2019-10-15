using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    [AutoRegister]
    public sealed class DamageSystem : SystemBase, IReceiveGlobal<TakeDamageEvent>, IReceive<ImpactEvent> {
        

        public DamageSystem() {
            EntityController.RegisterReceiver(
                new EventReceiverFilter(
                    this, new[] {
                        typeof(DamageImpact)
                    }));
        }
        
        private CircularBuffer<TakeDamageEvent> _eventLog = new CircularBuffer<TakeDamageEvent>(10, true);

        [Command("printDamageEventLog")]
        public static void PrintLog() {
            var log = World.Get<DamageSystem>()._eventLog;
            foreach (var msg in log.InOrder()) {
                Console.Log(
                    string.Format(
                        "{5}: Damage {0} hit {1} Amount {2} Type {3} Vital {4}",
                        msg.Origin?.Entity.DebugId ?? "null",
                        msg.Target?.Entity.DebugId ?? "null",
                        msg.Amount, msg.DamageType, msg.TargetVital, log.GetTime(msg)));
            }
        }
        
        public void HandleGlobal(TakeDamageEvent msg) {
            _eventLog.Add(msg);
            if (msg.Amount <= 0) {
                return;
            }
            var node = msg.Target;
            if (node == null) {
                return;
            }
            var logSystem = World.Get<GameLogSystem>();
            logSystem.StartNewMessage(out var dmgMsg, out var dmgHoverMsg);
            var blockDamage = node.Entity.Get<BlockDamage>();
            if (blockDamage != null) {
                for (int i = 0; i < blockDamage.DamageBlockers.Count; i++) {
                    if (blockDamage.DamageBlockers[i](msg)) {
                        dmgMsg.Append(msg.Target.GetName());
                        dmgMsg.Append(" completely blocked damage from ");
                        dmgMsg.Append(msg.Origin.GetName());
                        logSystem.PostCurrentStrings(GameLogSystem.DamageColor);
                        return;
                    }
                }
            }
            var damageAmount = msg.Amount;
            if (node.StatDefend != null) {
                for (int i = 0; i < node.StatDefend.Count; i++) {
                    if (node.StatDefend[i].DamageType != msg.DamageType) {
                        continue;
                    }
                    var defAmt = RulesSystem.GetDefenseAmount(damageAmount, node.StatDefend[i].Stat.Value);
                    if (defAmt <= 0) {
                        continue;
                    }
                    damageAmount = Mathf.Max(0, damageAmount - defAmt);
                    dmgHoverMsg.Append(node.StatDefend[i].Stat.Stat.Label);
                    dmgHoverMsg.Append(" Reduced Damage by ");
                    dmgHoverMsg.AppendNewLine(defAmt.ToString("F1"));
                }
            }
            if (node.DamageAbsorb != null) {
                for (int i = 0; i < node.DamageAbsorb.Count; i++) {
                    if (node.DamageAbsorb[i].DamageType != msg.DamageType) {
                        continue;
                    }
                    var defAmt = RulesSystem.GetDefenseAmount(damageAmount, node.DamageAbsorb[i].Amount);
                    if (defAmt <= 0) {
                        continue;
                    }
                    if (defAmt > node.DamageAbsorb[i].Remaining) {
                        defAmt = node.DamageAbsorb[i].Remaining;
                    }
                    damageAmount = Mathf.Max(0, damageAmount - defAmt);
                    dmgHoverMsg.Append(node.DamageAbsorb[i].Source);
                    dmgHoverMsg.Append(" Absorbed ");
                    dmgHoverMsg.Append(defAmt.ToString("F1"));
                    dmgHoverMsg.AppendNewLine(" Damage");
                    node.DamageAbsorb[i].Remaining -= defAmt;
                }
                node.DamageAbsorb.CheckLimits();
            }
            
            float previousValue = 0;
            var stats = node.Stats;
            var vital = stats.GetVital(msg.TargetVital);
            if (vital == null) {
                vital = stats.GetVital(GameData.Vitals.GetID(msg.TargetVital));
            }
            if (vital != null) {
                dmgHoverMsg.Append(vital.ToLabelString());
                dmgHoverMsg.Append(" - ");
                dmgHoverMsg.Append(damageAmount);
                dmgHoverMsg.Append(" = ");
                previousValue = vital.Current;
                vital.Current -= damageAmount;
                dmgHoverMsg.Append(vital.ToLabelString());
            }
            dmgMsg.Append(msg.Target.GetName());
            dmgMsg.Append(" took ");
            dmgMsg.Append(damageAmount.ToString("F1"));
            dmgMsg.Append(" damage ");
            if (damageAmount > 0) {
                node.Entity.Post(new CombatStatusUpdate(node.Entity,damageAmount.ToString("F1"), Color.red));
                msg.Impact.Source.PostAll(new CausedDamageEvent(damageAmount, msg));
            }
            if (vital != null && vital.Current <= 0 && msg.TargetVital == Stats.Health) {
                node.Entity.Post(new DeathEvent(msg.Origin, msg.Target, msg.Impact,  damageAmount - previousValue));
            }
        }
        
        /*
         * var amt = arg.Amount;
            bool isCritical = false;
            string source = arg.Origin.Get<LabelComponent>()?.Text;
            if (string.IsNullOrEmpty(source)) {
                var msg = isCritical ? CombatMessages.DamagedCritNoActor : CombatMessages.DamagedNoActor;
                msg.Show(UIStyle.Damage.ToHex(), Name, amt.ToString("F0"));
            }
            else {
                var msg = isCritical ? CombatMessages.DamagedCritActor : CombatMessages.DamageFromActor;
                msg.Show(UIStyle.Damage.ToHex(), source, Name, amt.ToString("F0"));
            }
         */

        public void Handle(ImpactEvent arg) {
            if (arg.Hit <= 0) {
                return;
            }
            var component = arg.Source.Find<DamageImpact>();
            var sourceEntity = component.GetEntity();
            var stats = sourceEntity.Get<StatsContainer>();
            if (component == null || stats == null) {
                return;
            }
            var targetStat = arg.Target.Stats.GetVital(component.TargetVital);
            if (targetStat == null) {
                return;
            }
            var power = RulesSystem.CalculateTotal(stats, Stats.Power, component.NormalizedPercent);
            var logSystem = World.Get<GameLogSystem>();
            logSystem.StartNewMessage(out var logMsg, out var hoverMsg);
            logMsg.Append(arg.Origin.GetName());
            logMsg.Append(" hit ");
            logMsg.Append(power.ToString("F0"));
            logMsg.Append(" ");
            logMsg.Append(targetStat.Label);
            logMsg.Append(" for ");
            logMsg.Append(arg.Target.GetName());
            hoverMsg.Append(RulesSystem.LastQueryString);
            logSystem.PostCurrentStrings(GameLogSystem.DamageColor);
            //CollisionExtensions.GetHitMultiplier(impact.Hit, impact.Target)
            arg.Target.Post(new TakeDamageEvent(power, arg, component.DamageType, component.TargetVital));
        }
    }
}
