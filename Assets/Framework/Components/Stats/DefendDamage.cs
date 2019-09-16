using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class DefendDamageWithStats : IComponent {

        private List<StatEntry> _entries = new List<StatEntry>();
        public StatEntry this[int index] { get { return _entries[index]; } }
        public int Count { get { return _entries.Count; } }
        public DefendDamageWithStats() {}

        public DefendDamageWithStats(SerializationInfo info, StreamingContext context) {
            _entries = info.GetValue(nameof(_entries), _entries);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_entries), _entries);
        }
        
        public void AddStat(string type, string id, BaseStat stat) {
            _entries.Add(new StatEntry(type, id, stat));
        }

        public void Remove(string id) {
            for (int i = 0; i < _entries.Count; i++) {
                if (_entries[i].ID == id) {
                    _entries.RemoveAt(i);
                    break;
                }
            }
        }


        [System.Serializable]
        public class StatEntry {
            [SerializeField] public CachedStat<BaseStat> Stat;
            [SerializeField] public string DamageType;
            [SerializeField] public string ID;

            public StatEntry(string damageType, string id, BaseStat stat) {
                Stat = new CachedStat<BaseStat>(stat);
                DamageType = damageType;
                ID = id;
            }
        }
    }

    [System.Serializable]
	public sealed class DamageAbsorb : IComponent {

        private List<Entry> _entries = new List<Entry>();
        
        public Entry this[int index] { get { return _entries[index]; } }
        public int Count { get { return _entries.Count; } }
        
        public DamageAbsorb() {}

        public DamageAbsorb(SerializationInfo info, StreamingContext context) {
            _entries = info.GetValue(nameof(_entries), _entries);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_entries), _entries);
        }

        public string AddDefend(string source, string type, float amount, float limitAmount = -1) {
            _entries.Add(new Entry(source, type, amount, limitAmount));
            return _entries.LastElement().ID;
        }

        public void Remove(string id) {
            for (int i = 0; i < _entries.Count; i++) {
                if (_entries[i].ID == id) {
                    _entries.RemoveAt(i);
                    break;
                }
            }
        }

        public void CheckLimits() {
            for (int i = _entries.Count - 1; i >= 0; i--) {
                if (_entries[i].Remaining <= 0) {
                    _entries.RemoveAt(i);
                }
            }
        }

        [System.Serializable]
        public class Entry {
            [SerializeField] public readonly string DamageType;
            [SerializeField] public readonly string ID;
            [SerializeField] public readonly string Source;
            [SerializeField] public float Amount;
            [SerializeField] public float Remaining;

            public Entry(string source, string type, float amount, float defendLimit) {
                Source = source;
                DamageType = type;
                Amount = amount;
                Remaining = defendLimit;
                ID = System.Guid.NewGuid().ToString();
            }
        }
    }
}
