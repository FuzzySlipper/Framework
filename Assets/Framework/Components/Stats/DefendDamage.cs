using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Higher)]
    public class DefendDamageWithStats : IComponent, IReceiveRef<DamageEvent> {

        public int Owner { get; set; }
        private List<StatEntry> _stats = new List<StatEntry>();

        public void AddStat(string type, string id, BaseStat stat) {
            var statEntry = FindStat(type);
            if (statEntry == null) {
                statEntry = new StatEntry();
                _stats.Add(statEntry);
            }
            statEntry.Stat = stat;
            statEntry.Id = id;
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
            if (statEntry.Stat == null) {
                statEntry.Stat = this.GetEntity().Stats.Get(statEntry.Id);
            }
            if (statEntry.Stat == null || statEntry.Stat.Value <= 0) {
                return;
            }
            var amtDefended = GameOptions.GetDefenseAmount(arg.Amount, statEntry.Stat.Value);
            arg.Amount = MathEx.Max(0, arg.Amount - amtDefended);
        }

        public class StatEntry {
            public BaseStat Stat;
            public string DamageType;
            public string Id;
        }
    }

    [Priority(Priority.Higher)]
    public class DefendDamageFlat : IComponent, IReceiveRef<DamageEvent> {

        public int Owner { get; set; }

        private List<DefendType> _validTypes = new List<DefendType>();

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

        public class DefendType {
            public string Type;
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
