using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ActionTemplate : BaseTemplate {

        private CachedComponent<ActionConfig> _config = new CachedComponent<ActionConfig>();
        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<AmmoComponent> _ammo = new CachedComponent<AmmoComponent>();
        private CachedComponent<SpawnPivotComponent> _spawnPivot = new CachedComponent<SpawnPivotComponent>();
        private CachedComponent<WeaponModelComponent> _weapon = new CachedComponent<WeaponModelComponent>();
        private CachedComponent<ActionFxComponent> _fx = new CachedComponent<ActionFxComponent>();
        private CachedComponent<RuleEventListenerComponent> _ruleEvent = new CachedComponent<RuleEventListenerComponent>();
        
        public ActionConfig Config { get => _config; }
        public TransformComponent Tr { get => _tr; }
        public AmmoComponent Ammo { get => _ammo; }
        public SpawnPivotComponent SpawnPivot { get => _spawnPivot; }
        public WeaponModelComponent Weapon { get => _weapon; }
        public ActionFxComponent Fx { get => _fx; }
        public RuleEventListenerComponent RuleEvents => _ruleEvent.Value;
        
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _config, _tr, _ammo, _spawnPivot, _weapon, _fx, _ruleEvent
        };

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(ActionConfig),
            };
        }
    }
}
