using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class ModifyMoveSpeedRuleTemplate : RuleTemplate, IRuleEventRun<GatherMoveSpeedEvent> {

        private CachedComponent<MoveSpeedBonusComponent> _component = new CachedComponent<MoveSpeedBonusComponent>();
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _component, EntityStats
        };

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(InitiativeBonus),
            };
        }

        public void RuleEventRun(ref GatherMoveSpeedEvent context) {
            context.Bonus += _component.Value.Bonus;
        }
    }
    
    /// <summary>
    /// Move Speed default is (MoveDistance) / 5
    /// </summary>
    public struct GatherMoveSpeedEvent : IRuleEvent {
        public BaseActionTemplate Action { get; }
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public float Base;
        public float Bonus;
        public float Total { get { return Base + Bonus; } }

        public GatherMoveSpeedEvent(CharacterTemplate origin, float bonus) {
            Action = null;
            Origin = origin;
            Target = origin;
            Base = origin.Stats.GetValue(Stats.MoveSpeed);
            Bonus = bonus;
        }
    }
}
