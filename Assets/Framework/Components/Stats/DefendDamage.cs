using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Priority(Priority.Higher)]
    [System.Serializable]
	public sealed class DefendDamageWithStats : IComponent, IReceiveRef<DamageEvent> {

        private List<StatEntry> _stats = new List<StatEntry>();

        public DefendDamageWithStats() {}

        public DefendDamageWithStats(SerializationInfo info, StreamingContext context) {
            _stats = info.GetValue(nameof(_stats), _stats);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_stats), _stats);
        }
        
        public void AddStat(string type, string id, BaseStat stat) {
            var statEntry = FindStat(type);
            if (statEntry == null) {
                statEntry = new StatEntry(stat);
                _stats.Add(statEntry);
            }
            statEntry.DamageType = type;
        }

        private StatEntry FindStat(string type) {
            for (int i = 0; i < _stats.Count; i++) {
                if (_stats[i].DamageType == type) {
                    return _stats[i];
                }
            }
            return null;
        }

        public void Handle(ref DamageEvent arg) {
            if (arg.Amount <= 0) {
                return;
            }
            var statEntry = FindStat(arg.DamageType);
            if (statEntry == null) {
                return;
            }
            if (statEntry.Stat == null || statEntry.Stat.Value <= 0) {
                return;
            }
            var amtDefended = GameOptions.GetDefenseAmount(arg.Amount, statEntry.Stat.Value);
            arg.Amount = MathEx.Max(0, arg.Amount - amtDefended);
        }

        [System.Serializable]
        public class StatEntry {
            public CachedStat<BaseStat> Stat;
            public string DamageType;
            public StatEntry(){}

            public StatEntry(BaseStat stat) {
                Stat = new CachedStat<BaseStat>(stat);
            }
        }
    }

    [Priority(Priority.Higher)]
    [System.Serializable]
	public sealed class DefendDamageFlat : IComponent, IReceiveRef<DamageEvent> {

        private List<DefendType> _validTypes = new List<DefendType>();

        public DefendDamageFlat() {
        }

        public DefendDamageFlat(SerializationInfo info, StreamingContext context) {
            _validTypes = info.GetValue(nameof(_validTypes), _validTypes);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_validTypes), _validTypes);
        }

        public DefendDamageFlat(string[] validTypes, float amount, float limitAmount = -1f) {
            if (validTypes != null) {
                for (int i = 0; i < validTypes.Length; i++) {
                    _validTypes.Add(new DefendType(validTypes[i], amount, limitAmount));
                }
            }
        }

        public void AddDefend(string type, float amount, float limitAmount = -1) {
            var defendType = GetType(type);
            if (defendType == null) {
                defendType = new DefendType(type, amount, limitAmount);
                _validTypes.Add(defendType);
            }
            else {
                defendType.Amount += amount;
                if (limitAmount > 0 && defendType.DefendLimit >= 0) {
                    defendType.DefendLimit += limitAmount;
                }
            }
        }

        public DefendType GetType(string type) {
            for (int i = 0; i < _validTypes.Count; i++) {
                if (_validTypes[i].Type == type) {
                    return _validTypes[i];
                }
            }
            return null;
        }

        public void Handle(ref DamageEvent arg) {
            var defendType = GetType(arg.DamageType);
            if (defendType == null || Math.Abs(defendType.Amount) < 0.01f) {
                return;
            }
            var amtDefended = GameOptions.GetDefenseAmount(arg.Amount, defendType.Amount);
            arg.Amount = MathEx.Max(0, arg.Amount - amtDefended);
            if (defendType.DefendLimit < 0) {
                return;
            }
            defendType.DefendLimit -= amtDefended;
            if (defendType.DefendLimit <= 0) {
                defendType.Amount = 0;
            }
        }

        [System.Serializable]
        public class DefendType {
            public readonly string Type;
            public float Amount;
            public float DefendLimit;

            public DefendType(string type, float amount, float defendLimit) {
                Type = type;
                Amount = amount;
                DefendLimit = defendLimit;
            }
        }
    }
}
