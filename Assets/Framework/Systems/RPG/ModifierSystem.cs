using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ModifierSystem : SystemBase, IMainSystemUpdate {
        
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
