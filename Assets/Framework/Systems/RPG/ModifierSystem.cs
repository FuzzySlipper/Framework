using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    [AutoRegister]
    public class ModifierSystem : SystemBase<ModifierSystem>, IMainSystemUpdate, IRuleEventRun<TryApplyEntityTag>, IRuleEventRun<TryApplyMod>,
    IRuleEventStart<TryApplyEntityTag> {
        
        public ModifierSystem() {
            World.Get<RulesSystem>().AddHandler<TryApplyEntityTag>(this);
            World.Get<RulesSystem>().AddHandler<TryApplyMod>(this);
        }

        private static FastString _fastString = new FastString();

        private List<RemoveStatModifier> _removeStats = new List<RemoveStatModifier>();
        
        public void OnSystemUpdate(float dt, float unscaledDt) {
            for (int i = _removeStats.Count - 1; i >= 0; i--) {
                if (_removeStats[i].Entry.PercentLeft <= 0) {
                    _removeStats[i].Stat.RemoveMod(_removeStats[i].Entry.Id);
                    _removeStats[i].Entry.Target.Post(new ModifiersChanged(_removeStats[i].Entry.Target));
                    _removeStats.RemoveAt(i);
                }
            }
        }

        public void RemoveStatMod(string modId) {
            for (int i = _removeStats.Count - 1; i >= 0; i--) {
                if (_removeStats[i].Entry.Id == modId) {
                    _removeStats[i].Stat.RemoveMod(_removeStats[i].Entry.Id);
                    _removeStats.RemoveAt(i);
                }
            }
        }

        public void FillModList(List<ModEntry> entries, int entityTarget) {
            for (int i = 0; i < _removeStats.Count; i++) {
                if (_removeStats[i].Entry.Target == entityTarget) {
                    entries.Add(_removeStats[i].Entry);
                }
            }
        }


        public bool CanRuleEventStart(ref TryApplyEntityTag context) {
            var success = RulesSystem.DiceRollSuccess(context.Chance);
            PrintLog(success, context);
            return success;
        }

        public void RuleEventRun(ref TryApplyEntityTag context) {
            context.Target.Tags.Add(context.Tag);
            context.Target.Post(new TagChangeEvent(context.Target.Entity, context.Tag, true));
            if (context.Length > 0) {
                context.Target.Post(new TagTimerEvent(context.Target, TimeManager.Time + context.Length,
                    context.Tag));
            }
        }

        private void PrintLog(bool success, TryApplyEntityTag context) {
            var logSystem = World.Get<GameLogSystem>();
            logSystem.StartNewMessage(out var logMsg, out var hoverMsg);
            logMsg.Append(context.Origin.GetName());
            logMsg.Append(!success ? " failed to apply " : " applied ");
            logMsg.Append(context.Description);
            logMsg.Append(" on ");
            logMsg.Append(context.Target.GetName());
            hoverMsg.AppendNewLine(RulesSystem.LastQueryString.ToString());
            if (success) {
                hoverMsg.Append(context.Target.GetName());
                hoverMsg.Append(" has ");
                hoverMsg.Append(context.Description);
                hoverMsg.Append(" for ");
                hoverMsg.Append(context.Length);
                hoverMsg.Append(" ");
                hoverMsg.Append(StringConst.TimeUnits);
            }
            logSystem.PostCurrentStrings(!success ? GameLogSystem.NormalColor : GameLogSystem.DamageColor);
        }

        public void RuleEventRun(ref TryApplyMod context) {
            var stats = context.Origin.Stats;
            var stat = stats.Get(context.TargetStat);
            if (stat == null) {
                return;
            }
            if (stat.HasMod(context.ID)) {
                RemoveStatMod(context.ID);
            }
            var power = RulesSystem.CalculateImpactTotal(stats, Stats.Power, context.NormalizedPercent);
            stat.AddValueMod(new BaseStat.StatValueMod(power, context.ID));
            _fastString.Clear();
            _fastString.Append("+");
            _fastString.Append(power);
            _fastString.Append(" ");
            _fastString.Append(stat.Label);
            var label = _fastString.ToString();
            AddStatRemovalTimer(
                new RemoveStatModifier(
                    stat, new ModEntry(
                        label, label, context.ID, context.Length,
                        context.Origin.Entity, context.Target.Entity, context.Icon)));
            context.Target.Post(new ModifiersChanged(context.Target.Entity));
            var logSystem = World.Get<GameLogSystem>();
            logSystem.StartNewMessage(out var logMsg, out var hoverMsg);
            logMsg.Append(context.Origin.GetName());
            logMsg.Append(" added ");
            logMsg.Append(stat.Label);
            logMsg.Append(" modifier to ");
            logMsg.Append(context.Target.GetName());

            hoverMsg.AppendNewLine(RulesSystem.LastQueryString.ToString());
            hoverMsg.Append(label);
            hoverMsg.Append(" for ");
            hoverMsg.Append(context.Length);
            hoverMsg.Append(" ");
            hoverMsg.Append(StringConst.TimeUnits);
            logSystem.PostCurrentStrings( power > 0 ? GameLogSystem.HealColor : GameLogSystem.DamageColor);
        }

        public void AddStatRemovalTimer(RemoveStatModifier arg) {
            _removeStats.Add(arg);
        }
    }

    public struct ModEntry {
        public string Label { get; }
        public string Description { get; }
        public string Id { get; }
        public float Length { get; }
        public float Start { get; }
        public Entity Owner { get; }
        public Entity Target { get; }
        public Sprite Icon { get; }
        public float PercentLeft {
            get {
                if (Length <= 0) {
                    return 0;
                }
                return ((Start + Length) - TimeManager.Time) / Length;
            }
        }

        public ModEntry(string label, string description, string id, float length, Entity owner, Entity target, Sprite icon) {
            Label = label;
            Description = description;
            Id = id;
            Length = length;
            Owner = owner;
            Target = target;
            Icon = icon;
            Start = TimeManager.Time;
        }
    }

    public struct RemoveStatModifier {
        public BaseStat Stat { get; }
        public ModEntry Entry { get; }

        public RemoveStatModifier(BaseStat stat, ModEntry modEntry) {
            Stat = stat;
            Entry = modEntry;
        }
    }

    public struct TryApplyMod : IRuleEvent {
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public ActionTemplate Action { get; }
        public float Length { get; }
        public string TargetStat { get; }
        public float NormalizedPercent { get; }
        public Sprite Icon { get; }
        public string ID { get; }

        public TryApplyMod(ImpactEvent impactEvent, AddModImpact component) {
            Action = impactEvent.Action;
            Origin = impactEvent.Origin;
            Target = impactEvent.Target;
            TargetStat = component.TargetStat;
            NormalizedPercent = component.NormalizedPercent;
            Length = component.Length;
            Icon = component.Icon;
            ID = component.ID;
        }
    }
    
    public struct TryApplyEntityTag : IRuleEvent {
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public ActionTemplate Action { get; }
        public int Tag { get; }
        public float Chance { get; }
        public float Length { get; }
        public string Description { get; }
        public string Defense { get; }

        public TryApplyEntityTag(ImpactEvent impactEvent, ApplyTagImpact component) {
            Action = impactEvent.Action;
            Origin = impactEvent.Origin;
            Target = impactEvent.Target;
            Tag = component.Tag;
            Chance = component.Chance;
            Length = component.Length;
            Description = component.Description;
            Defense = component.Defense;
        }
    }
}
