using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ConvertVitalRuleTemplate : RuleTemplate, IRuleEventEnded<ImpactEvent> {

        private CachedComponent<ConvertVitalImpact> _component = new CachedComponent<ConvertVitalImpact>();

        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _component, EntityStats
        };


        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(ConvertVitalImpact),
            };
        }

        public void RuleEventEnded(ref ImpactEvent context) {
            World.Get<RulesSystem>().Post(new ConvertVitalEvent(context, _component));
        }
    }
}
