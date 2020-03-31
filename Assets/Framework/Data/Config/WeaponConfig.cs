using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class WeaponConfig : EquipmentConfig, IActionConfig {
        private static GameOptions.CachedFloat _brokenWeaponPercent = new GameOptions.CachedFloat("WeaponBrokenPercentDamage");

        public string EquipVariable;
        public string WeaponModel;
        
        [Header("IActionConfig")]
        [SerializeField] private string _actionTrigger = "Attacking";
        [SerializeField] private ActionDistance _range = ActionDistance.Short;
        [SerializeField] private FloatRange _power = new FloatRange();
        [SerializeField] private float _critMulti = 1.5f;
        [SerializeField] private CollisionType _collision = CollisionType.Point;
        [SerializeField] private TargetType _targeting = TargetType.Enemy;
        [SerializeField] private ImpactRadiusTypes _radius = ImpactRadiusTypes.Single;
        [SerializeField] private string _damageType = Defenses.Physical;
        [SerializeField] private ProjectileConfig _projectile = null;
        [SerializeField] private ActionFx _actionFx = null;
        [SerializeField] private ScriptedEventConfig[] _scriptedEvents = new ScriptedEventConfig[0];
        
        [Header("Ammo")] 
        public AmmoConfig Ammo;
        public int AmmoAmount = 100;
        public ReloadType ReloadType = ReloadType.Repair;
        [Range(0, 5)] public float ReloadSpeedMulti = 1;
        
        public string ActionTrigger { get => _actionTrigger; }
        public ActionDistance Range { get => _range; }
        public FloatRange Power { get => _power; }
        public CollisionType Collision { get => _collision; }
        public ImpactRadiusTypes Radius { get => _radius; }
        public string DamageType { get => _damageType; }
        public ProjectileConfig Projectile { get => _projectile; }
        public ActionFx ActionFx { get => _actionFx; }
        public ScriptedEventConfig[] ScriptedEvents { get => _scriptedEvents; }
        public float CritMulti { get => _critMulti; }
        public TargetType Targeting { get => _targeting; }
        public string AbilityType { get { return "Attack"; } }


        public override void AddComponents(Entity entity) {
            base.AddComponents(entity);
            var stats = entity.Get<StatsContainer>();
            stats.AddRange(StatExtensions.GetBasicCommandStats(stats));
            
            var action = entity.Add(new ActionConfig());
            action.Primary = true;
            action.WeaponModel = WeaponModel;
            action.AnimationTrigger = GraphTriggers.Attack;
            action.EquipVariable = EquipVariable;
            entity.Add(new DamageImpact(DamageType, Stats.Health, 1f));
            ActionProvider.AddComponent(entity, this, action);
            ActionProvider.AddCheckForCollision(action, this, true);
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
