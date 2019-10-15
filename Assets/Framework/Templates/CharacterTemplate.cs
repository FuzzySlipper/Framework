using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CharacterTemplate : BaseTemplate {
        
        private CachedComponent<LabelComponent> _label = new CachedComponent<LabelComponent>();
        private CachedComponent<StatusContainer> _status = new CachedComponent<StatusContainer>();
        private CachedComponent<GridPosition> _position = new CachedComponent<GridPosition>();
        private CachedComponent<FactionComponent> _faction = new CachedComponent<FactionComponent>();
        private CachedComponent<ReadyActions> _currentActions = new CachedComponent<ReadyActions>();
        private CachedComponent<EquipmentSlots> _slots = new CachedComponent<EquipmentSlots>();
        private CachedComponent<CommandTarget> _target = new CachedComponent<CommandTarget>();
        private CachedComponent<DefendDamageWithStats> _statDefend = new CachedComponent<DefendDamageWithStats>();
        private CachedComponent<DamageAbsorb> _damageAbsorb = new CachedComponent<DamageAbsorb>();
        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<ColliderComponent> _collider = new CachedComponent<ColliderComponent>();
        private CachedComponent<StatsContainer> _stats = new CachedComponent<StatsContainer>();
        private CachedComponent<SteeringInput> _steering = new CachedComponent<SteeringInput>();
        private CachedComponent<AnimationGraphComponent> _animGraph = new CachedComponent<AnimationGraphComponent>();
        private CachedComponent<CurrentAction> _currentAction = new CachedComponent<CurrentAction>();
        
        public TransformComponent Tr { get => _tr.Value; }
        public Collider Collider { get => _collider.Value.Collider; }
        public StatsContainer Stats => _stats.Value;
        public DefendDamageWithStats StatDefend => _statDefend.Value;
        public DamageAbsorb DamageAbsorb => _damageAbsorb.Value;
        public CommandTarget CommandTarget => _target.Value;
        public EquipmentSlots EquipmentSlots => _slots.Value;
        public ReadyActions ReadyActions => _currentActions.Value;
        public FactionComponent Faction => _faction.Value;
        public StatusContainer Status => _status.Value;
        public GridPosition Position => _position.Value;
        public LabelComponent Label => _label.Value;
        public CommandTarget Target => _target.Value;
        public TagsComponent Tags => Entity.Tags;
        public SteeringInput Steering => _steering.Value;
        public RuntimeStateGraph AnimGraph => _animGraph.Value.Value;
        public Action CurrentAction => _currentAction.Value?.Value;
        public CurrentAction CurrentActionComponent => _currentAction.Value;
        public bool IsDead => Entity.Tags.Contain(EntityTags.IsDead);

        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _label, _status, _position, _faction, _currentActions, _slots, _target, _statDefend, _damageAbsorb,
            _collider, _stats, _tr, _steering, _animGraph, _currentAction
        };

        public VitalStat GetVital(int vital) {
            return _stats.Value.GetVital(GameData.Vitals.GetID(vital));
        }

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(DamageComponent),
                typeof(FactionComponent),
                typeof(ReadyActions),
                typeof(StatsContainer),
            };
        }
    }
}
