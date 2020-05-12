using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class RaiseDeadRuleTemplate : RuleTemplate, IRuleEventEnded<ImpactEvent> {

        private CachedComponent<RaiseDeadImpact> _component = new CachedComponent<RaiseDeadImpact>();

        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _component, EntityStats
        };


        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(RaiseDeadImpact),
            };
        }

        public void RuleEventEnded(ref ImpactEvent context) {
            context.Target.Post(new RaiseDeadEvent(context.Action, context.Origin, context.Target));
        }
    }
}