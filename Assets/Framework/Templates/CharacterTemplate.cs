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
        private CachedComponent<AnimationEventComponent> _animationEvent = new CachedComponent<AnimationEventComponent>();
        private CachedComponent<RuleEventListenerComponent> _ruleEvent = new CachedComponent<RuleEventListenerComponent>();
        private CachedComponent<GenericDataComponent> _genericData = new CachedComponent<GenericDataComponent>();
        private CachedComponent<ActionSlots> _actionSlots = new CachedComponent<ActionSlots>();
        private CachedComponent<ModifierListComponent> _modList = new CachedComponent<ModifierListComponent>();
        private CachedComponent<IconComponent> _icon = new CachedComponent<IconComponent>();
        public IconComponent Icon { get => _icon; }
        public ModifierListComponent ModList { get => _modList; }

        public GenericDataComponent GenericData { get => _genericData; }
        public TransformComponent Tr { get => _tr.Value; }
        public Collider Collider { get => _collider.Value.Collider; }
        public StatsContainer Stats => _stats.Value;
        public DefendDamageWithStats StatDefend => _statDefend.Value;
        public DamageAbsorb DamageAbsorb => _damageAbsorb.Value;
        public EquipmentSlots EquipmentSlots => _slots.Value;
        public ReadyActions ReadyActions => _currentActions.Value;
        public FactionComponent Faction => _faction.Value;
        public StatusContainer Status => _status.Value;
        public GridPosition Position => _position.Value;
        public LabelComponent Label => _label.Value;
        public CommandTarget Target => _target.Value;
        public TagsComponent Tags => Entity.Tags;
        public SteeringInput Steering => _steering.Value;
        public AnimationGraphComponent AnimGraphComponent => _animGraph.Value;
        public RuntimeStateGraph AnimGraph => _animGraph.Value.Value;
        public ActionTemplate CurrentAction => _currentAction.Value?.Value;
        public CurrentAction CurrentActionComponent => _currentAction.Value;
        public AnimationEventComponent AnimationEvent => _animationEvent.Value;
        public RuleEventListenerComponent RuleEvents => _ruleEvent.Value;
        public ActionSlots ActionSlots { get => _actionSlots; }

        public bool IsDead => Entity.Tags.Contain(EntityTags.IsDead);

        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _label, _status, _position, _faction, _currentActions, _slots, _target, _statDefend, _damageAbsorb,
            _collider, _stats, _tr, _steering, _animGraph, _currentAction, _animationEvent, _ruleEvent, 
            _actionSlots, _modList, _icon
        };

        public VitalStat GetVital(int vital) {
            return _stats.Value.GetVital(Vitals.GetValue(vital));
        }

        public string GetAttackAccuracyStatBonusName() {
            var dataName = GenericData.GetString(GenericDataTypes.AttackAccuracyBonusStat);
            return GenericData.GetString(dataName);
        }

        public string GetAttackDamageBonusStatName() {
            var dataName = GenericData.GetString(GenericDataTypes.AttackDamageBonusStat);
            return GenericData.GetString(dataName);
        }

        public BaseStat GetAttackAccuracyBonusStat() {
            return Stats.Get(GetAttackAccuracyStatBonusName());
        }

        public BaseStat GetAttackDamageBonusStat() {
            return Stats.Get(GetAttackDamageBonusStatName());
        }

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(DamageComponent),
                typeof(FactionComponent),
                typeof(ReadyActions),
                typeof(StatsContainer),
            };
        }
    }
}
