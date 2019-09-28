using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    [AutoRegister]
    public class ModifierSystem : SystemBase, IMainSystemUpdate, IReceive<ImpactEvent> {
        
        public ModifierSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(AddModImpact)
            }));
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

        public void Handle(ImpactEvent arg) {
            if (arg.Hit <= 0) {
                return;
            }
            var addMod = arg.Source.Find<AddModImpact>();
            var sourceEntity = addMod.GetEntity();
            var stats = sourceEntity.Get<StatsContainer>();
            if (addMod == null || stats == null) {
                return;
            }
            var stat = stats.Get(addMod.TargetStat);
            if (stat == null) {
                return;
            }
            if (stat.HasMod(addMod.ID)) {
                RemoveStatMod(addMod.ID);
            }
            var power = RulesSystem.CalculateTotal(stats, Stats.Power, addMod.NormalizedPercent);
            stat.AddValueMod(new BaseStat.StatValueMod(power, addMod.ID));
            _fastString.Clear();
            _fastString.Append("+");
            _fastString.Append(power);
            _fastString.Append(" ");
            _fastString.Append(stat.Label);
            var label = _fastString.ToString();
            AddStatRemovalTimer(
                new RemoveStatModifier(
                    stat, new ModEntry(
                        label, label, addMod.ID, addMod.Length,
                        arg.Origin.Entity, arg.Target.Entity, addMod.Icon)));
            arg.Target.Post(new ModifiersChanged(arg.Target.Entity));
            var logSystem = World.Get<GameLogSystem>();
            logSystem.StartNewMessage(out var logMsg, out var hoverMsg);
            logMsg.Append(arg.Origin.GetName());
            logMsg.Append(" added ");
            logMsg.Append(stat.Label);
            logMsg.Append(" modifier to ");
            logMsg.Append(arg.Target.GetName());

            hoverMsg.AppendNewLine(RulesSystem.LastQueryString.ToString());
            hoverMsg.Append(label);
            hoverMsg.Append(" for ");
            hoverMsg.Append(addMod.Length);
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
}
