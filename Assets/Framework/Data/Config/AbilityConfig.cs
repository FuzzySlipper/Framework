using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace PixelComrades {
    public class AbilityConfig : ScriptableObject, IActionConfig {

        private const float EffectTime = 3f;
        private const float EffectChance = 10f;
        private static GameOptions.CachedFloat _defaultAnimationTimeout = new GameOptions.CachedFloat("DefaultAnimationTimeout");

        public string Name;
        public string Description;
        public int Level;
        public string Skill;
        public AssetReferenceSprite Icon;
        public ActionSource Source;
        public string AdditionalModel;
        public string DataDescription;
        public float Cost;
        public GenericConfigEntry[] Config = new GenericConfigEntry[0];
        
        [Header("IActionConfig")]
        [SerializeField] private string _abilityType = AbilityTypes.Attack;
        [SerializeField] private string _secondaryType = AbilityTypes.None;
        [SerializeField] private float _secondaryPower = EffectChance;
        [SerializeField] private string _actionTrigger = "";
        [SerializeField] private ActionDistance _range = ActionDistance.Short;
        [SerializeField] private FloatRange _power = new FloatRange();
        [SerializeField] private float _critMulti = 1.5f;
        [SerializeField] private CollisionType _collision = CollisionType.Point;
        [SerializeField] private TargetType _targeting= TargetType.Enemy;
        [SerializeField] private ImpactRadiusTypes _radius = ImpactRadiusTypes.Single;
        [SerializeField] private string _damageType = Defenses.Physical;
        [SerializeField] private ProjectileConfig _projectile= null;
        [SerializeField] private ActionFx _actionFx= null;
        [SerializeField] private ScriptedEventConfig[] _scriptedEvents = new ScriptedEventConfig[0];
        public string ID { get { return name; } }
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
        public string AbilityType { get { return _abilityType; } }

        public void AddComponents(Entity entity) {
            var action = entity.Add(new ActionConfig());
            action.AnimationTrigger = ActionTrigger;
            action.EquipVariable = "";
            bool generateCollision = false;
            switch (AbilityType) {
                case AbilityTypes.Heal:
                    generateCollision = true;
                    break;
                case AbilityTypes.AddModImpact:
                    generateCollision = true;
                    break;
                case AbilityTypes.Teleport:
                case AbilityTypes.Unlock:
                    break;
                //case "Teleport":
                //    sequence.Add(new PlayActionAnimation(ActionStateEvents.None, animation, true, false, true));
                //    sequence.Add(new WaitForAnimation(ActionStateEvents.Activate, animation, true, _defaultAnimationTimeout));
                //    sequence.Add(new TeleportSequence(ActionStateEvents.None, config.FindInt("Distance", 5)));
                //    break;
                //case "Unlock":
                //    sequence.Add(new PlayActionAnimation(ActionStateEvents.None, animation, true, false, true));
                //    sequence.Add(new WaitForAnimation(ActionStateEvents.Activate, animation, true, _defaultAnimationTimeout));
                //    sequence.Add(new Unlock(ActionStateEvents.None, power.UpperRange, data.TryGetValue("Cost", 1f)));
                //    break;
            }
            AddImpact(entity, AbilityType);
            AddImpact(entity, _secondaryType);
            switch (AbilityType) {
                default:
                    action.Costs.Add(new CostVital(Stats.Energy,  Cost, Skill));
                    break;
                case "Shield":
                case "Unlock":
                    break;
            }
            ActionProvider.AddComponent(entity, this, action);
            if (generateCollision) {
                action.AddEvent(AnimationEvents.Default, new EventGenerateCollisionEvent());
            }
            else {
                ActionProvider.AddCheckForCollision(action, this, false);
            }
        }

        private void AddImpact(Entity entity, string type) {
            switch (type) {
                case AbilityTypes.Attack:
                    entity.Add(new DamageImpact(DamageType, Stats.Health, 1));
                    break;
                case AbilityTypes.Heal:
                    entity.Add(new HealImpact(Stats.Health, 1, Targeting == TargetType.Self));
                    break;
                case AbilityTypes.Shield:
                    entity.Add(new BlockDamageAction(AdditionalModel, Stats.Energy, Cost, Skill, PlayerControls.UseSecondary));
                    break;
                case AbilityTypes.AddModImpact:
                    entity.Add(new AddModImpact(Config.FindFloat("Length", 1), Config.FindString("TargetStat"),1f, entity
                    .Get<IconComponent>()));
                    break;
                case AbilityTypes.ConvertHealthEnergy:
                    entity.Add(new ConvertVitalImpact(Config.FindFloat("Percent", 1f), Config.FindString("SourceVital"), Config.FindString("TargetVital")));
                    break;
                case AbilityTypes.InstantKill:
                    entity.Add(new InstantKillImpact(Config.FindFloat("Chance", 1f)));
                    break;
                case AbilityTypes.Confuse:
                    entity.Add(
                        new ApplyTagImpact(
                            EntityTags.IsConfused, _secondaryPower, Config
                                .FindFloat("Length", EffectTime), DamageType, "Confusion"));
                    break;
                case AbilityTypes.Slow:
                    entity.Add(
                        new ApplyTagImpact(
                            EntityTags.IsSlowed, _secondaryPower, Config
                                .FindFloat("Length", EffectTime), DamageType, "Slow"));
                    break;
                case AbilityTypes.Stun:
                    entity.Add(
                        new ApplyTagImpact(
                            EntityTags.IsStunned, _secondaryPower, Config
                                .FindFloat("Length", EffectTime), DamageType, "Stun"));
                    break;
            }
        }
    } 
}
