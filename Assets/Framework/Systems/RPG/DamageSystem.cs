using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    [AutoRegister]
    public sealed class DamageSystem : SystemBase, IRuleEventRun<TakeDamageEvent>, IRuleEventEnded<PrepareDamageEvent> {
        
        public DamageSystem() {
            World.Get<RulesSystem>().AddHandler<TakeDamageEvent>(this);
            World.Get<RulesSystem>().AddHandler<PrepareDamageEvent>(this);
            GenericPools.Register<List<DamageEntry>>(5, l => l.Clear());
        }

        public void RuleEventEnded(ref PrepareDamageEvent context) {
            if (context.CurrentTotal() > 0) {
                World.Get<RulesSystem>().Post(new TakeDamageEvent(ref context));
            }
        }
        
        private CircularBuffer<TakeDamageEvent> _eventLog = new CircularBuffer<TakeDamageEvent>(10, true);

        [Command("printDamageEventLog")]
        public static void PrintLog() {
            var log = World.Get<DamageSystem>()._eventLog;
            foreach (var msg in log.InOrder()) {
                Console.Log(
                    string.Format(
                        "{0}: Damage {1} hit {2} Amount {3}",
                        log.GetTime(msg),
                        msg.Origin?.Entity.DebugId ?? "null",
                        msg.Target?.Entity.DebugId ?? "null",
                        msg.Amount));
            }
        }

        public void RuleEventRun(ref TakeDamageEvent msg) {
            _eventLog.Add(msg);
            if (msg.Target.IsDead) {
                msg.Clear();
                return;
            }
            var target = msg.Target;
            var logSystem = World.Get<GameLogSystem>();
            logSystem.StartNewMessage(out var dmgMsg, out var dmgHoverMsg);
            if (msg.Hit.Result == CollisionResult.CriticalHit) {
                dmgMsg.Append("<b>");
                dmgHoverMsg.Append("<b>");
            }
            if (target.StatDefend != null) {
                for (int i = 0; i < target.StatDefend.Count; i++) {
                    for (int d = msg.Entries.Count - 1; d >= 0; d--) {
                        var dmg = msg.Entries[d];
                        if (target.StatDefend[i].DamageType != dmg.DamageType) {
                            continue;
                        }
                        var defAmt = RulesSystem.GetDefenseAmount(dmg.Amount, target.StatDefend[i].Stat.Value);
                        if (defAmt <= 0) {
                            continue;
                        }
                        var amount = Mathf.Max(0, dmg.Amount - defAmt);
                        dmgHoverMsg.Append(target.StatDefend[i].Stat.Stat.Label);
                        dmgHoverMsg.Append(" Reduced Damage by ");
                        dmgHoverMsg.AppendNewLine(defAmt.ToString("F1"));
                        msg.Entries.Remove(dmg);
                        msg.Entries.Add(new DamageEntry(amount, dmg.DamageType, dmg.TargetVital, dmg.Description));
                    }
                }
            }
            if (target.DamageAbsorb != null) {
                for (int i = 0; i < target.DamageAbsorb.Count; i++) {
                    for (int d = msg.Entries.Count - 1; d >= 0; d--) {
                        var dmg = msg.Entries[d];
                        if (target.DamageAbsorb[i].DamageType != dmg.DamageType) {
                            continue;
                        }
                        var defAmt = RulesSystem.GetDefenseAmount(dmg.Amount, target.DamageAbsorb[i].Amount);
                        if (defAmt <= 0) {
                            continue;
                        }
                        if (defAmt > target.DamageAbsorb[i].Remaining) {
                            defAmt = target.DamageAbsorb[i].Remaining;
                        }
                        var amount = Mathf.Max(0, dmg.Amount - defAmt);
                        dmgHoverMsg.Append(target.DamageAbsorb[i].Source);
                        dmgHoverMsg.Append(" Absorbed ");
                        dmgHoverMsg.Append(defAmt.ToString("F1"));
                        dmgHoverMsg.AppendNewLine(" Damage");
                        target.DamageAbsorb[i].Remaining -= defAmt;
                        target.DamageAbsorb.CheckLimits();
                        msg.Entries.Remove(dmg);
                        msg.Entries.Add(new DamageEntry(amount, dmg.DamageType, dmg.TargetVital, dmg.Description));
                    }
                }
            }
            float totalDmg = 0;
            for (int i = 0; i < msg.Entries.Count; i++) {
                if (msg.Target.IsDead) {
                    break;
                }
                var dmg = msg.Entries[i];
                var damageAmount = dmg.Amount;
                totalDmg += damageAmount;
                float previousValue = 0;
                var stats = target.Stats;
                var vital = stats.GetVital(dmg.TargetVital);
                if (vital == null) {
                    vital = stats.GetVital(dmg.TargetVital);
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
                if (vital != null && vital.Current <= 0 && dmg.TargetVital == Stat.Health) {
                    target.Entity.Post(new DeathEvent(msg.Origin, msg.Target, msg.Hit.Point, damageAmount - previousValue));
                }
            }
            if (totalDmg > 0) {
                target.Entity.Post(new CombatStatusUpdate(target.Entity, totalDmg.ToString("F1"), Color.red));
                msg.Origin.Post(new CausedDamageEvent(totalDmg, msg));
                msg.Target.Post(new ReceivedDamageEvent(totalDmg, msg));
            }
            logSystem.PostCurrentStrings(GameLogSystem.DamageColor);
            msg.Clear();
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

    }
}
