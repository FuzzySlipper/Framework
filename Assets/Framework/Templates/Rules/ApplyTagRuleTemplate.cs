using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ApplyTagRuleTemplate : RuleTemplate, IRuleEventEnded<ImpactEvent> {

        private CachedComponent<ApplyTagImpact> _applyTag = new CachedComponent<ApplyTagImpact>();

        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _applyTag, EntityStats
        };

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(ApplyTagImpact),
            };
        }

        public void RuleEventEnded(ref ImpactEvent context) {
            if (context.Hit <= 0) {
                return;
            }
            World.Get<RulesSystem>().Post(new TryApplyEntityTag(context, _applyTag.Value));
        }
    }
}