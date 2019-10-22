using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ApplyModifierRuleTemplate : RuleTemplate, IRuleEventEnded<ImpactEvent> {

        private CachedComponent<AddModImpact> _component = new CachedComponent<AddModImpact>();

        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _component, EntityStats
        };

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(AddModImpact),
            };
        }

        public void RuleEventEnded(ref ImpactEvent context) {
            World.Get<RulesSystem>().Post(new TryApplyMod(context, _component.Value));
        }
    }
}