using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class RuleTemplate : BaseTemplate, IRuleEventHandler {

        protected CachedComponent<StatsContainer> EntityStats = new CachedComponent<StatsContainer>();
        
        public override void Register(Entity entity) {
            base.Register(entity);
            if (entity == null) {
                return;
            }
            entity.GetOrAdd<RuleEventListenerComponent>().Handlers.Add(this);
        }

        public override void Dispose() {
            if (Entity != null) {
                var ruleEvents = Entity.Get<RuleEventListenerComponent>();
                if (ruleEvents != null) {
                    ruleEvents.Handlers.Remove(this);
                }
            }
            base.Dispose();
        }
    }
}
