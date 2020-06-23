using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ActionTemplate : BaseTemplate {

        private CachedComponent<ActionConfig> _config = new CachedComponent<ActionConfig>();
        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<AmmoComponent> _ammo = new CachedComponent<AmmoComponent>();
        private CachedComponent<SpawnPivotComponent> _spawnPivot = new CachedComponent<SpawnPivotComponent>();
        private CachedComponent<ActionFxComponent> _fx = new CachedComponent<ActionFxComponent>();
        private CachedComponent<RuleEventListenerComponent> _ruleEvent = new CachedComponent<RuleEventListenerComponent>();
        private CachedComponent<StatsContainer> _stats = new CachedComponent<StatsContainer>();
        private CachedComponent<GenericDataComponent> _data = new CachedComponent<GenericDataComponent>();
        private CachedComponent<IconComponent> _icon = new CachedComponent<IconComponent>();

        public IconComponent Icon { get => _icon; }
        public ActionConfig Config { get => _config; }
        public TransformComponent Tr { get => _tr; }
        public AmmoComponent Ammo { get => _ammo; }
        public SpawnPivotComponent SpawnPivot { get => _spawnPivot; }
        public ActionFxComponent Fx { get => _fx; }
        public RuleEventListenerComponent RuleEvents => _ruleEvent.Value;
        public StatsContainer Stats => _stats.Value;
        public GenericDataComponent Data { get => _data.Value; }

        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _config, _tr, _ammo, _spawnPivot, _fx, _ruleEvent, _data, _stats, _icon
        };

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(ActionConfig),
            };
        }

        public bool CanAct(ActionTemplate action, CharacterTemplate owner) {
            for (int c = 0; c < Config.Costs.Count; c++) {
                if (!Config.Costs[c].CanAct(action, owner)) {
                    return false;
                }
            }
            return true;
        }
    }
}
