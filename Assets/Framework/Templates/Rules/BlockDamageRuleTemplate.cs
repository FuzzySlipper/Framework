using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class BlockDamageRuleTemplate : RuleTemplate, IRuleEventStart<PrepareDamageEvent> {
        
        private CachedComponent<BlockDamageFlat> _block = new CachedComponent<BlockDamageFlat>();
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _block, EntityStats
        };

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(BlockDamageFlat),
            };
        }

        public bool CanRuleEventStart(ref PrepareDamageEvent context) {
            World.Get<GameLogSystem>().StartNewMessage(out var log, out var hover);
            log.Append(context.Target.GetName());
            log.Append(" completely blocked damage from ");
            log.Append(context.Origin.GetName());
            World.Get<GameLogSystem>().PostCurrentStrings(GameLogSystem.DamageColor);
            return false;
        }
    }
}
