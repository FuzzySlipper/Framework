using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class WeaponConfig : ItemConfig, IActionConfig {
        private static GameOptions.CachedFloat _brokenWeaponPercent = new GameOptions.CachedFloat("WeaponBrokenPercentDamage");

        public SpriteAnimationReference Idle;
        [ValueDropdown("SkillSlotList")] public string Skill;

        [Header("IActionConfig")]
        [SerializeField] private int _range = 5;
        [SerializeField] private FloatRange _power = new FloatRange();
        [SerializeField] private float _critMulti = 1.5f;
        [SerializeField] private CollisionType _collision = CollisionType.Point;
        [SerializeField] private TargetType _targeting = TargetType.Enemy;
        [SerializeField] private ImpactRadiusTypes _radius = ImpactRadiusTypes.Single;
        [SerializeField, ValueDropdown("DamageTypeList")] private string _damageType = Defenses.Physical;
        [SerializeField] private ActionFx _actionFx = null;
        [SerializeField] private ScriptedEventConfig[] _scriptedEvents = new ScriptedEventConfig[0];
        [SerializeField] private StateGraph _actionGraph = null;
        
        [Header("Ammo")] 
        public AmmoConfig Ammo;
        public int AmmoAmount = 100;
        public ReloadType ReloadType = ReloadType.Repair;
        [Range(0, 5)] public float ReloadSpeedMulti = 1;
        
        public int Range { get => _range; }
        public FloatRange Power { get => _power; }
        public CollisionType Collision { get => _collision; }
        public ImpactRadiusTypes Radius { get => _radius; }
        public string DamageType { get => _damageType; }
        public ActionFx ActionFx { get => _actionFx; }
        public ScriptedEventConfig[] ScriptedEvents { get => _scriptedEvents; }
        public float CritMulti { get => _critMulti; }
        public TargetType Targeting { get => _targeting; }
        public string AbilityType { get { return "Attack"; } }
        public StateGraph ActionGraph { get => _actionGraph; }

        private ValueDropdownList<string> DamageTypeList() {
            return Defenses.GetDropdownList();
        }

        private ValueDropdownList<string> SkillSlotList() {
            return Skills.GetDropdownList();
        }
        public override string ItemType { get { return ItemTypes.Weapon; } }

        public override void AddComponents(Entity entity) {
            base.AddComponents(entity);
            var stats = entity.Get<StatsContainer>();
            //stats.AddRange(StatExtensions.GetBasicCommandStats(stats));
            entity.Add(new SkillRequirement(Skill, 0));
            var action = entity.Add(new ActionConfig(this));
            action.Primary = true;
            action.Sprite = Idle;
            action.AnimationTrigger = GraphTriggers.Attack;
            entity.Add(new DamageImpact(DamageType, Stats.Health, 1f));
            ActionProvider.AddComponent(entity, this, action);
            //ActionProvider.AddCheckForCollision(action, this, true);
            AmmoComponent ammoComponent;
            switch (ReloadType) {
                case ReloadType.Repair:
                    ammoComponent = entity.Add(new AmmoComponent(Ammo, Skill, ReloadSpeedMulti, stats.Get(Stats.Power), 
                    _brokenWeaponPercent));
                    var handlers = entity.GetOrAdd<RuleEventListenerComponent>();
                    handlers.Handlers.Add(World.Get<AmmoSystem>());
                    break;
                default:
                case ReloadType.Reload:
                    ammoComponent = entity.Add(new AmmoComponent(Ammo, Skill, ReloadSpeedMulti, null));
                    action.Costs.Add(new CostAmmo(ammoComponent));
                    break;
            }
            ammoComponent.Amount.SetLimits(0, AmmoAmount);
            ammoComponent.Amount.SetMax();
        }
    }
}
