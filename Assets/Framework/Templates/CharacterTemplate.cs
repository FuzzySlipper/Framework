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
        private CachedComponent<EquipmentSlots> _slots = new CachedComponent<EquipmentSlots>();
        private CachedComponent<CommandTarget> _target = new CachedComponent<CommandTarget>();
        private CachedComponent<DefendDamageWithStats> _statDefend = new CachedComponent<DefendDamageWithStats>();
        private CachedComponent<DamageAbsorb> _damageAbsorb = new CachedComponent<DamageAbsorb>();
        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<ColliderComponent> _collider = new CachedComponent<ColliderComponent>();
        private CachedComponent<StatsContainer> _stats = new CachedComponent<StatsContainer>();
        private CachedComponent<AnimationGraphComponent> _animGraph = new CachedComponent<AnimationGraphComponent>();
        private CachedComponent<AnimationEventComponent> _animationEvent = new CachedComponent<AnimationEventComponent>();
        private CachedComponent<RuleEventListenerComponent> _ruleEvent = new CachedComponent<RuleEventListenerComponent>();
        private CachedComponent<ActionSlots> _actionSlots = new CachedComponent<ActionSlots>();
        private CachedComponent<GenericDataComponent> _data = new CachedComponent<GenericDataComponent>();
        private CachedComponent<EntityLevelComponent> _level = new CachedComponent<EntityLevelComponent>();

        public int Level { get => _level.Value.Level; }
        public EntityLevelComponent LevelComponent { get => _level.Value; }
        public GenericDataComponent GenericData { get => _data.Value; }
        public ActionSlots ActionSlots { get => _actionSlots; }
        public TransformComponent Tr { get => _tr.Value; }
        public Collider Collider { get => _collider.Value.Collider; }
        public StatsContainer Stats => _stats.Value;
        public DefendDamageWithStats StatDefend => _statDefend.Value;
        public DamageAbsorb DamageAbsorb => _damageAbsorb.Value;
        public EquipmentSlots EquipmentSlots => _slots.Value;
        public FactionComponent Faction => _faction.Value;
        public StatusContainer Status => _status.Value;
        public GridPosition Position => _position.Value;
        public LabelComponent Label => _label.Value;
        public CommandTarget Target => _target.Value;
        public EntityTagsComponent Tags => Entity.Tags;
        public RuntimeStateGraph AnimGraph => _animGraph.Value.Value;
        public AnimationEventComponent AnimationEvent => _animationEvent.Value;
        public RuleEventListenerComponent RuleEvents => _ruleEvent.Value;
        public bool IsDead => Entity.Tags.Contain(EntityTags.IsDead);

        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _label, _status, _position, _faction, _slots, _target, _statDefend, _damageAbsorb,
            _collider, _stats, _tr, _animGraph, _animationEvent, _ruleEvent, _actionSlots, _data,_level
        };

        public VitalStat GetVital(int vital) {
            return _stats.Value.GetVital(GameData.Vitals.GetID(vital));
        }

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(DamageComponent),
                typeof(FactionComponent),
                typeof(StatsContainer),
            };
        }
    }
}
