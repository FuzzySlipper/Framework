using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Random = UnityEngine.Random;

namespace PixelComrades {

    public sealed class RulesSystem : SystemBase {
        
        public static readonly FastString LastQueryString = new FastString();
        private Dictionary<System.Type, List<IRuleEventHandler>> _globalHandlers = new Dictionary<System.Type, List<IRuleEventHandler>>();
        private GenericPool<List<IRuleEventHandler>> _listPool = new GenericPool<List<IRuleEventHandler>>(5, t => t.Clear());
        private CircularBuffer<IRuleEvent> _eventLog = new CircularBuffer<IRuleEvent>(10, true);
        
        public RulesSystem() {
            RegisterRuleTemplates();
        }

        [Command("printRuleEventLog")]
        public static void PrintLog() {
            var log = World.Get<RulesSystem>()._eventLog;
            foreach (var msg in log.InOrder()) {
                Console.Log(
                    string.Format(
                        "{0}: Rule Event {1} Action {2} Source {3} Target {4}",
                        log.GetTime(msg),
                        msg.GetType().Name,
                        msg.Action?.Entity.DebugId ?? "null",
                        msg.Origin?.Entity.DebugId ?? "null",
                        msg.Target?.Entity.DebugId ?? "null"));
            }
        }

        public static bool DiceRollSuccess(float chance) {
            LastQueryString.Clear();
            LastQueryString.Append("Rolled D100 against ");
            if (chance < 1 && chance > 0) {
                chance *= 100;
            }
            LastQueryString.Append(chance.ToString("F0"));
            LastQueryString.Append("% Chance. Result: ");
            var roll = Game.Random.Next(0, 101);
            LastQueryString.AppendBold(roll.ToString("F0"));
            bool success = roll <= chance;
            LastQueryString.Append(success ? " Success!" : " Failure!");
            return success;
        }

        public static float CalculateImpactTotal(StatsContainer stats, string statName, float percent) {
            var range = stats.Get<RangeStat>(statName);
            if (range != null) {
                return CalculateTotal(range, percent);
            }
            var stat = stats.Get<BaseStat>(statName);
            if (stat != null) {
                return CalculateTotal(stat, percent);
            }
            return 0;
        }

        public void AddHandler<T>(IRuleEventHandler handler) where T : IRuleEvent {
            var eventName = typeof(T);
            if (!_globalHandlers.TryGetValue(eventName, out var list)) {
                list = new List<IRuleEventHandler>();
                _globalHandlers.Add(eventName, list);
            }
            list.Add(handler);
        }

        public void RemoveHandler<T>(IRuleEventHandler handler) where T : IRuleEvent {
            var eventName = typeof(T);
            if (!_globalHandlers.TryGetValue(eventName, out var list)) {
                return;
            }
            list.Remove(handler);
        }

        public void Post<T>(T context) where T : struct, IRuleEvent {
            _eventLog.Add(context);
            var list = _listPool.New();
            _globalHandlers.TryGetValue(typeof(T), out var globalList);
            if (globalList != null) {
                list.AddRange(globalList);
            }
            if (context.Origin.RuleEvents != null) {
                list.AddRange(context.Origin.RuleEvents.Handlers);
            }
            if (context.Origin.CurrentAction != null  &&
                context.Origin.CurrentAction.RuleEvents != null) {
                list.AddRange(context.Origin.CurrentAction.RuleEvents.Handlers);
            }
            for (int i = 0; i < list.Count; i++) {
                if (list[i] is IRuleEventStart<T> startHandler) {
                    if (!startHandler.CanRuleEventStart(ref context)) {
                        _listPool.Store(list);
                        return;
                    }
                }
            }
            for (int i = 0; i < list.Count; i++) {
                if (list[i] is IRuleEventRun<T> handler) {
                    handler.RuleEventRun(ref context);
                }
            }
            for (int i = 0; i < list.Count; i++) {
                if (list[i] is IRuleEventEnded<T> endHandler) {
                    endHandler.RuleEventEnded(ref context);
                }
            }
            _listPool.Store(list);
        }

        private bool TryStart<T>(List<IRuleEventHandler> list, ref T context) where T : IRuleEvent {
            for (int i = 0; i < list.Count; i++) {
                if (list[i] is IRuleEventStart<T> startHandler) {
                    if (!startHandler.CanRuleEventStart(ref context)) {
                        return false;
                    }
                }
            }
            return true;
        }

        private void PostRun<T>(List<IRuleEventHandler> list, ref T context) where T : IRuleEvent {
            for (int i = 0; i < list.Count; i++) {
                if (list[i] is IRuleEventRun<T> handler) {
                    handler.RuleEventRun(ref context);
                }
            }
        }

        private void PostEnd<T>(List<IRuleEventHandler> list, ref T context) where T : IRuleEvent {
            for (int i = 0; i < list.Count; i++) {
                if (list[i] is IRuleEventEnded<T> endHandler) {
                    endHandler.RuleEventEnded(ref context);
                }
            }
        }

        private void RegisterRuleTemplates() {
            TemplateFilter<ApplyModifierRuleTemplate>.Setup();
            TemplateFilter<ApplyTagRuleTemplate>.Setup();
            TemplateFilter<BlockDamageRuleTemplate>.Setup();
            TemplateFilter<ConvertVitalRuleTemplate>.Setup();
            TemplateFilter<DamageRuleTemplate>.Setup();
            TemplateFilter<HealRuleTemplate>.Setup();
            TemplateFilter<InstantKillRuleTemplate>.Setup();
            TemplateFilter<RaiseDeadRuleTemplate>.Setup();
            
        }

        public static float CalculateTotal(BaseStat stat, float percent) {
            LastQueryString.Clear();
            LastQueryString.Append(stat.Label);
            LastQueryString.Append(": ");
            LastQueryString.Append(stat.BaseValue.ToString("F0"));
            LastQueryString.Append(" + ");
            LastQueryString.AppendNewLine(stat.ModTotal.ToString("F0"));
            LastQueryString.Append(" = ");
            var result = stat.BaseValue + stat.ModTotal;
            LastQueryString.Append(result.ToString("F0"));
            if (Math.Abs(percent - 1) > 0.0001f) {
                LastQueryString.Append(" * ");
                LastQueryString.Append(percent.ToString("F1"));
                LastQueryString.Append(" Final: ");
                result *= percent;
                LastQueryString.Append(result.ToString("F0"));
            }
            return result;
        }
        
        public static float CalculateTotal(RangeStat stat, float percent) {
            LastQueryString.Clear();
            LastQueryString.Append("Base ");
            LastQueryString.Append(stat.Label);
            LastQueryString.Append(": ");
            LastQueryString.Append(stat.BaseValue.ToString("F0"));
            LastQueryString.Append("-");
            LastQueryString.Append((stat.BaseValue + stat.MaxModifier).ToString("F0"));
            LastQueryString.Append(" + ");
            LastQueryString.AppendNewLine(stat.ModTotal.ToString("F0"));
            var roll = Game.Random.NextFloat(stat.BaseValue, stat.BaseValue + stat.MaxModifier);
            LastQueryString.Append("Rolled ");
            LastQueryString.Append(roll.ToString("F0"));
            LastQueryString.Append(" + ");
            LastQueryString.Append(stat.ModTotal.ToString("F0"));
            LastQueryString.Append(" = ");
            var result = roll + stat.ModTotal;
            LastQueryString.Append(result.ToString("F0"));
            if (Math.Abs(percent - 1) > 0.0001f) {
                LastQueryString.Append(" * ");
                LastQueryString.Append(percent.ToString("F1"));
                LastQueryString.Append(" Final: ");
                result *= percent;
                LastQueryString.Append(result.ToString("F0"));
            }
            return result;
        }
        
        public static float GetDefenseAmount(float damage, float stat) {
            return damage * (stat / (damage * 10) * 0.5f);
        }

        public static int TotalPrice(InventoryItem item) {
            return item.Price * item.Count;
        }

        public static bool CheckVisible(CharacterTemplate npc) {
            return Game.Random.CoinFlip();
        }
    }
}
