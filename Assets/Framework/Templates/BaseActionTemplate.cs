using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class BaseActionTemplate : BaseTemplate {
        private CachedComponent<ActionConfig> _config = new CachedComponent<ActionConfig>();
        private CachedComponent<SpawnPivotComponent> _spawnPivot = new CachedComponent<SpawnPivotComponent>();
        private CachedComponent<ActionFxComponent> _fx = new CachedComponent<ActionFxComponent>();
        private CachedComponent<RuleEventListenerComponent> _ruleEvent = new CachedComponent<RuleEventListenerComponent>();
        private CachedComponent<GenericDataComponent> _data = new CachedComponent<GenericDataComponent>();
        private CachedComponent<StatsContainer> _stats = new CachedComponent<StatsContainer>();

        public GenericDataComponent Data { get => _data.Value; }
        public ActionConfig Config { get => _config; }
        public SpawnPivotComponent SpawnPivot { get => _spawnPivot; }
        public ActionFxComponent Fx { get => _fx; }
        public RuleEventListenerComponent RuleEvents => _ruleEvent.Value;
        public StatsContainer Stats => _stats.Value;


        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _config, _spawnPivot, _fx, _ruleEvent, _data, _stats
        };
        
        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(ActionConfig),
            };
        }

        public bool CanAct(Entity origin, Entity target) {
            for (int c = 0; c < Config.Costs.Count; c++) {
                if (!Config.Costs[c].CanAct(origin, target)) {
                    return false;
                }
            }
            return true;
        }
    }
}
