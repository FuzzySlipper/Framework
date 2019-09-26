using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    [AutoRegister]
    public sealed class DamageSystem : SystemBase, IReceiveGlobal<DamageEvent>, IReceiveGlobal<HealEvent>, IReceiveGlobal<RaiseDeadEvent> {
        
        private static Color _damageColor = new Color(1f, 0.53f, 0.04f);
        private static Color _deathColor = new Color(0.54f, 0f, 0.05f);
        
        public DamageSystem(){}
        
        private StringBuilder _dmgMsg = new StringBuilder(100);
        private StringBuilder _dmgHoverMsg = new StringBuilder(100);
        private CircularBuffer<DamageEvent> _eventLog = new CircularBuffer<DamageEvent>(10, true);

        [Command("PrintDamageEventLog")]
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
        
        public void HandleGlobal(DamageEvent msg) {
            _eventLog.Add(msg);
            if (msg.Amount <= 0) {
                return;
            }
            var node = msg.Target;
            if (node == null) {
                return;
            }
            _dmgMsg.Clear();
            _dmgHoverMsg.Clear();
            var blockDamage = node.Entity.Get<BlockDamage>();
            if (blockDamage != null) {
                for (int i = 0; i < blockDamage.Dels.Count; i++) {
                    if (blockDamage.Dels[i](msg)) {
                        _dmgMsg.Append(msg.Target.GetName());
                        _dmgMsg.Append(" completely blocked damage from ");
                        _dmgMsg.Append(msg.Origin.GetName());
                        MessageKit<UINotificationWindow.Msg>.post(Messages.MessageLog, new UINotificationWindow.Msg(_dmgMsg.ToString(),
                            _dmgMsg.ToString(), _damageColor));
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
                    var defAmt = RuleSystem.GetDefenseAmount(damageAmount, node.StatDefend[i].Stat.Value);
                    if (defAmt <= 0) {
                        continue;
                    }
                    damageAmount = Mathf.Max(0, damageAmount - defAmt);
                    _dmgHoverMsg.Append(node.StatDefend[i].Stat.Stat.Label);
                    _dmgHoverMsg.Append(" Reduced Damage by ");
                    _dmgHoverMsg.AppendNewLine(defAmt.ToString("F1"));
                }
            }
            if (node.DamageAbsorb != null) {
                for (int i = 0; i < node.DamageAbsorb.Count; i++) {
                    if (node.DamageAbsorb[i].DamageType != msg.DamageType) {
                        continue;
                    }
                    var defAmt = RuleSystem.GetDefenseAmount(damageAmount, node.DamageAbsorb[i].Amount);
                    if (defAmt <= 0) {
                        continue;
                    }
                    if (defAmt > node.DamageAbsorb[i].Remaining) {
                        defAmt = node.DamageAbsorb[i].Remaining;
                    }
                    damageAmount = Mathf.Max(0, damageAmount - defAmt);
                    _dmgHoverMsg.Append(node.DamageAbsorb[i].Source);
                    _dmgHoverMsg.Append(" Absorbed ");
                    _dmgHoverMsg.Append(defAmt.ToString("F1"));
                    _dmgHoverMsg.AppendNewLine(" Damage");
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
                _dmgHoverMsg.Append(vital.ToLabelString());
                _dmgHoverMsg.Append(" - ");
                _dmgHoverMsg.Append(damageAmount);
                _dmgHoverMsg.Append(" = ");
                previousValue = vital.Current;
                vital.Current -= damageAmount;
                _dmgHoverMsg.Append(vital.ToLabelString());
            }
            var origin = msg.Origin.GetName();
            _dmgMsg.Append(origin);
            _dmgMsg.Append(" hit ");
            _dmgMsg.Append(msg.Target.GetName());
            _dmgMsg.Append(" for ");
            _dmgMsg.Append(damageAmount.ToString("F1"));
            if (damageAmount > 0) {
                node.Entity.Post(new CombatStatusUpdate(node.Entity,damageAmount.ToString("F1"), Color.red));
                MessageKit<UINotificationWindow.Msg>.post(Messages.MessageLog, new UINotificationWindow.Msg(_dmgMsg.ToString(), 
                _dmgHoverMsg.ToString(), _damageColor));
            }
            if (vital == null || vital != stats.HealthStat || vital.Current > 0.0001f) {
                return;
            }
            _dmgMsg.Clear();
            _dmgMsg.Append(origin);
            _dmgMsg.Append(" killed ");
            _dmgMsg.Append(msg.Target.GetName());
            MessageKit<UINotificationWindow.Msg>.post(Messages.MessageLog, new UINotificationWindow.Msg(_dmgMsg.ToString(), 
                _dmgHoverMsg.ToString(), _deathColor));
            node.Entity.Tags.Add(EntityTags.IsDead);
            node.Entity.Tags.Add(EntityTags.CantMove);
            node.Entity.Post(new DeathEvent(msg.Origin, msg.Target, damageAmount - previousValue));
        }

        public void HandleGlobal(HealEvent arg) {
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
                    entity.Post(new CombatStatusUpdate(entity,arg.Amount.ToString("F1"), color));
                }
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

        public void HandleGlobal(RaiseDeadEvent arg) {
            var entity = arg.Target;
            entity.Tags.Remove(EntityTags.IsDead);
            entity.Tags.Remove(EntityTags.CantMove);
        }
    }
}
