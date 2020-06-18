using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public class AttackRollTemplate : RuleTemplate, IRuleEventRun<CheckHitEvent> {

        private CachedComponent<AttackBonusComponent> _component = new CachedComponent<AttackBonusComponent>();
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _component, EntityStats
        };

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(AttackBonusComponent),
            };
        }

        public void RuleEventRun(ref CheckHitEvent context) {
            RulesSystem.LastQueryString.Append(context.Origin.GetName());
            RulesSystem.LastQueryString.Append(": ");
            var bonus = RulesSystem.CalculateStatsWithLog(context.Origin.Stats.Get(_component.Value.Stat), _component.Value.AddLevel ?
                (int) context.Origin.Stats.GetValue(Stats.Level) : -1);
            context.AttackTotal += bonus;
            RulesSystem.LastQueryString.AppendNewLine();
            if (_component.Value.Uses > 0) {
                _component.Value.Uses--;
                if (_component.Value.Uses <= 0) {
                    Entity.Remove(_component.Value);
                    _component.Clear();
                }
            }
        }
    }

    public class AttackBonusComponent : IComponent {

        public string Stat;
        public bool AddLevel;
        public int Uses;

        public AttackBonusComponent(string stat, int uses = -1,bool addLevel = true) {
            Stat = stat;
            AddLevel = addLevel;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    public class DefendRollTemplate : RuleTemplate, IRuleEventRun<CheckHitEvent> {

        private CachedComponent<DefendBonusComponent> _component = new CachedComponent<DefendBonusComponent>();
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _component, EntityStats
        };

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(DefendBonusComponent),
            };
        }

        public void RuleEventRun(ref CheckHitEvent context) {
            for (int i = 0; i < _component.Value.Entries.Count; i++) {
                var entry = _component.Value.Entries[i];
                if (entry.TargetDefense == context.TargetDefense) {
                    RulesSystem.LastQueryString.Append(context.Target.GetName());
                    RulesSystem.LastQueryString.Append(": ");
                    var bonus = RulesSystem.CalculateStatsWithLog(context.Target.Stats.Get(entry.TargetStat));
                    context.AttackTotal += bonus;
                    RulesSystem.LastQueryString.AppendNewLine();
                }
            }
        }
    }

    public class DefendBonusComponent : IComponent {

        public List<Entry> Entries = new List<Entry>();

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
        }

        public struct Entry {
            public string TargetDefense;
            public string TargetStat;
        }
    }
}
