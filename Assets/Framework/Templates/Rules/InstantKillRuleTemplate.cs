using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class InstantKillRuleTemplate : RuleTemplate, IRuleEventEnded<ImpactEvent> {

        private CachedComponent<InstantKillImpact> _component = new CachedComponent<InstantKillImpact>();

        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _component, EntityStats
        };


        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(InstantKillImpact),
            };
        }

        public void RuleEventEnded(ref ImpactEvent context) {
            World.Get<RulesSystem>().Post(new InstantKillEvent(context, _component));
        }
    }

    public struct InstantKillEvent : IRuleEvent {
        public CharacterTemplate Origin { get { return ImpactEvent.Origin; } }
        public CharacterTemplate Target { get { return ImpactEvent.Target; } }
        public ActionTemplate Action { get { return ImpactEvent.Action; } }
        public ImpactEvent ImpactEvent { get; }
        public InstantKillImpact InstantKill { get; }

        public InstantKillEvent(ImpactEvent impactEvent, InstantKillImpact instantKill) {
            ImpactEvent = impactEvent;
            InstantKill = instantKill;
        }
    }
}
