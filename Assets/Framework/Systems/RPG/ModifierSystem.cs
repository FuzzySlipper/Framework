using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    [AutoRegister]
    public class ModifierSystem : SystemBase<ModifierSystem>, IMainSystemUpdate, IRuleEventRun<TryApplyEntityTag>, IRuleEventRun<TryApplyMod>,
    IRuleEventStart<TryApplyEntityTag>, IRuleEventRun<ApplyModEvent> {
        
        public ModifierSystem() {
            World.Get<RulesSystem>().AddHandler<TryApplyEntityTag>(this);
            World.Get<RulesSystem>().AddHandler<TryApplyMod>(this);
            World.Get<RulesSystem>().AddHandler<ApplyModEvent>(this);
        }

        private static FastString _fastString = new FastString();
        
        private SimpleBufferedList<ModEntry> _allMods = new SimpleBufferedList<ModEntry>();
        private SimpleBufferedList<TimedModEntry> _timedMods = new SimpleBufferedList<TimedModEntry>();
        
        public void OnSystemUpdate(float dt, float unscaledDt) {
            _timedMods.Update();
            for (int i = 0; i < _timedMods.Count; i++) {
                if (_timedMods[i].PercentLeft <= 0) {
                    RemoveNoUpdate(_timedMods[i]);
                }
            }
            _allMods.Update();
            _timedMods.Update();
        }

        public void Add(ModEntry mod) {
            _allMods.Add(mod);
            if (mod is TimedModEntry timedModEntry) {
                _timedMods.Add(timedModEntry);
            }
            mod.Target.ModList.Mods.Add(mod);
            mod.Target.Post(new ModifiersChanged(mod.Target.Entity));
        }

        private void RemoveNoUpdate(ModEntry mod) {
            mod.OnEnd();
            _allMods.Remove(mod);
            if (mod is TimedModEntry timedModEntry) {
                _timedMods.Remove(timedModEntry);
            }
            mod.Target.Post(new ModifiersChanged(mod.Target));
        }

        public void Remove(ModEntry mod) {
            RemoveNoUpdate(mod);
            _allMods.Update();
            _timedMods.Update();
        }

        public void Remove(string id) {
            for (int i = 0; i < _allMods.Count; i++) {
                if (_allMods[i].Id == id) {
                    RemoveNoUpdate(_allMods[i]);
                }
            }
            _allMods.Update();
            _timedMods.Update();
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
                Remove(context.ID);
            }
            var power = RulesSystem.CalculateImpactTotal(stats, Stats.Power, context.NormalizedPercent);
            var mod = new BaseStat.StatValueMod(power, context.ID);
            stat.AddValueMod(mod);
            _fastString.Clear();
            _fastString.Append("+");
            _fastString.Append(power);
            _fastString.Append(" ");
            _fastString.Append(stat.Label);
            var label = _fastString.ToString();
            Add(new TimedStatMod(context.Action.Config, context.Origin, context.Target, new BasicValueModHolder(stat, mod), context.Length));
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

        public void RuleEventRun(ref ApplyModEvent context) {
            context.Mod.OnApply();
            Add(context.Mod);
        }
    }
    
    public abstract class ModEntry {
        public string Label { get; protected set; }
        public string Description { get; }
        public CharacterTemplate Owner { get; }
        public CharacterTemplate Target { get; }
        public Sprite Icon { get; }
        public string Id { get; protected set; }
        
        public ModEntry(string label, string description, Sprite icon, string id, CharacterTemplate owner, CharacterTemplate target) {
            Label = label;
            Description = description;
            Id = id;
            Owner = owner;
            Target = target;
            Icon = icon;
        }

        protected ModEntry(string label, ActionEvent ae, string id) {
            Label = label;
            Description = ae.Action.Config.Source.Name;
            Id = id;
            Owner = ae.Origin;
            Target = ae.Target;
            Icon = ae.Action.Config.Source.Icon.LoadedAsset;
        }

        protected ModEntry(string label, ActionConfig config, string id, CharacterTemplate owner, CharacterTemplate target) {
            Label = label;
            Description = config.Source.Name;
            Id = id;
            Owner = owner;
            Target = target;
            Icon = config.Source.Icon.LoadedAsset;
        }

        public abstract void OnApply();

        public abstract void OnEnd();

    }

    public abstract class TimedModEntry : ModEntry {
        public float Length { get; }
        public float Start { get; }

        public float PercentLeft {
            get {
                if (Length <= 0) {
                    return 0;
                }
                return ((Start + Length) - TimeManager.Time) / Length;
            }
        }


        protected TimedModEntry(string label, ActionEvent ae, string id, float length) : base(label, ae, id) {
            Length = length;
            Start = TimeManager.Time;
        }

        protected TimedModEntry(string label, ActionConfig config, string id, CharacterTemplate owner, CharacterTemplate target, float length) : base(label, config, id, owner, target) {
            Length = length;
            Start = TimeManager.Time;
        }
    }

    public class TimedStatMod : TimedModEntry {

        private string _statId;
        private float _amount;
        public StatModHolder Stat { get; private set; }

        public TimedStatMod(ActionEvent ae, float length, StatModHolder stat) : base(stat.TargetStat.Label + " Mod", ae, stat.ModID, length) {
            Stat = stat;
        }

        public TimedStatMod(ActionConfig config, CharacterTemplate owner, CharacterTemplate target, StatModHolder stat, float length) : 
            base(stat.TargetStat.Label + " Mod", config, stat.ModID, owner, target, length) {
            Stat = stat;
        }

        public TimedStatMod(ActionEvent ae, float length, string statId, float amount) : base("", ae.Action.Config, "", ae.Origin, ae.Target, length) {
            _statId = statId;
            _amount = amount;
        }

        public override void OnApply() {
            if (Stat != null) {
                return;
            }
            var targetStat = Target.Stats.Get(_statId);
            if (targetStat == null) {
                ModifierSystem.Get.Remove(this);
                return;
            }
            Stat = new BasicValueModHolder(_amount);
            Stat.Attach(targetStat);
            Id = Stat.ModID;
            Label = targetStat.Label + " Mod";
        }

        public override void OnEnd() {
            Stat.Remove();
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

    public struct ApplyModEvent : IRuleEvent {
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public ActionTemplate Action { get; }
        public ModEntry Mod { get; }

        public ApplyModEvent(ActionEvent ae, ModEntry mod) {
            Action = ae.Action;
            Origin = ae.Origin;
            Target = ae.Target;
            Mod = mod;
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
