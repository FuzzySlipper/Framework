using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class RollInitiativeRuleTemplate : RuleTemplate, IRuleEventRun<RollInitiativeEvent> {

        private CachedComponent<InitiativeBonus> _component = new CachedComponent<InitiativeBonus>();
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _component, EntityStats
        };

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(InitiativeBonus),
            };
        }

        public void RuleEventRun(ref RollInitiativeEvent context) {
            context.Bonus += _component.Value.Bonus;
        }
    }

    public struct RollInitiativeEvent : IRuleEvent {
        public ActionTemplate Action { get; }
        public Entity Source { get; }
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public int Roll { get; }
        public int Bonus;
        public int Total { get { return Roll + Bonus; } }

        public RollInitiativeEvent(CharacterTemplate origin, int roll, int bonus) {
            Action = null;
            Source = origin.Entity;
            Origin = origin;
            Target = origin;
            Roll = roll;
            Bonus = bonus;
        }
    }
}
