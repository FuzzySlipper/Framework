using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelComrades.DungeonCrawler;

namespace PixelComrades {
    public class ActionProvider : IDataFactory<ActionConfig> {
        private static GameOptions.CachedFloat _defaultAnimationTimeout = new GameOptions.CachedFloat("DefaultAnimationTimeout");
        private static GameOptions.CachedFloat _brokenWeaponPercent = new GameOptions.CachedFloat("WeaponBrokenPercentDamage");

        private const float EffectTime = 3f;
        private const float EffectChance = 10f;
        

        public void AddComponent(Entity entity, DataEntry data) {
            var action = entity.Add(new ActionConfig());
            
            var type = data.Get<DataReference>(DatabaseFields.ItemType);
            var skill = data.TryGetValue<string>(DatabaseFields.Skill, "");
            action.Primary = type?.TargetID == "WeaponUsable";
            action.WeaponModel = data.TryGetValue("WeaponModel", "");
            if (!string.IsNullOrEmpty(action.WeaponModel)) {
                entity.Add(new WeaponModelComponent(action.WeaponModel));
            }
            else {
                action.WeaponModel = data.TryGetValue("SpriteWeaponModel", "");
            }
            var stats = entity.Get<StatsContainer>();
            var power = new RangeStat(entity, Stats.Power, Stats.Power, data.TryGetValue(DatabaseFields.PowerMin, 0f), data.TryGetValue
            (DatabaseFields.PowerMax, 1f));
            stats.Add(power);
            bool generateCollision = false;
            var targeting = ParseUtilities.TryParseEnum(data.TryGetValue("TargetType", "Enemy"), TargetType.Enemy);
            if (targeting == TargetType.Self || targeting == TargetType.Friendly) {
                generateCollision = true;
            }
            bool limitEnemy = true;
            var config = data.Get<DataList>("Config");
            var abilityType = data.TryGetValue("Type", "Attack");
            var damageType = data.TryGetValue(DatabaseFields.DamageType,
                GameData.DamageTypes.GetID(0));
            if (type != null && type.TargetID == "Ability") {
                action.AnimationTrigger = data.TryGetValue("Animation", GraphTriggers.UseAbility);
                action.EquipVariable = "";
                var secondaryType = data.TryGetValue("SecondaryType", "");
                switch (abilityType) {
                    default:
                    case "Attack":
                        entity.Add(
                            new DamageImpact(damageType, Stats.Health, 1));
                        break;
                    case "Heal":
                        entity.Add(AddHealImpact( config, false));
                        generateCollision = true;
                        limitEnemy = false;
                        break;
                    case "AddModImpact":
                        entity.Add(AddModImpact(entity, config));
                        generateCollision = true;
                        limitEnemy = false;
                        break;
                    case "Teleport":
                    case "Shield":
                    case "Unlock":
                        break;
                }
                switch (secondaryType) {
                    case "Heal":
                        entity.Add(AddHealImpact(config, true));
                        break;
                    case "AddModImpact":
                        entity.Add(AddModImpact(entity, config));
                        break;
                    case "ConvertVital":
                        entity.Add(new ConvertVitalImpact(config.FindFloat("Percent", 1f), config.FindString("SourceVital"), config.FindString("TargetVital")));
                        break;
                    case "InstantKill":
                        entity.Add(new InstantKillImpact(config.FindFloat("Chance", 1f)));
                        break;
                    case "Confuse":
                        entity.Add(
                            new ApplyTagImpact(EntityTags.IsConfused, data.TryGetValue("SecondaryPower", EffectChance), config
                                    .FindFloat("Length", EffectTime), damageType, "Confusion"));
                        break;
                    case "Slow":
                        entity.Add(new ApplyTagImpact(EntityTags.IsSlowed, data.TryGetValue("SecondaryPower", EffectChance), config
                            .FindFloat("Length",EffectTime), damageType, "Slow"));
                        break;
                    case "Stun":
                        entity.Add(new ApplyTagImpact(EntityTags.IsStunned, data.TryGetValue("SecondaryPower", EffectChance), config
                            .FindFloat("Length",EffectTime), damageType, "Stun"));
                        break;
                }
                switch (abilityType) {
                    default:
                        action.Costs.Add(new CostVital(Stats.Energy,  data.TryGetValue("Cost", 1f), skill));
                        break;
                    case "Shield":
                    case "Unlock":
                        break;
                }
            }
            else {
                action.AnimationTrigger = GraphTriggers.Attack;
                action.EquipVariable = data.TryGetValue("EquipVariable", "");
                entity.Add(
                    new DamageImpact(data.TryGetValue(DatabaseFields.DamageType,
                            GameData.DamageTypes.GetID(0)), Stats.Health, 1));
                var reload = data.TryGetValue("ReloadType", "Repair");
                var reloadSpeed = data.TryGetValue("ReloadSpeed", 1f);
                var ammo = AmmoFactory.GetTemplate(data.Get<DataReference>("Ammo"));
                AmmoComponent ammoComponent;
                switch (reload) {
                    case "Repair":
                        ammoComponent = entity.Add(new AmmoComponent(ammo, skill, reloadSpeed, power, _brokenWeaponPercent));
                        break;
                    default:
                    case "Reload":
                        ammoComponent = entity.Add(new AmmoComponent(ammo, skill, reloadSpeed, null));
                        break;

                }
                ammoComponent.Amount.SetLimits(0, data.TryGetValue("AmmoAmount", 5));
                ammoComponent.Amount.SetMax();
                action.Costs.Add(new CostAmmo(ammoComponent));
            }
            switch (abilityType) {
                default:
                    var radius = ParseUtilities.TryParseEnum(data.TryGetValue(DatabaseFields.Radius, "Single"), ImpactRadiusTypes.Single);
                    if (radius != ImpactRadiusTypes.Single) {
                        entity.Add(new ImpactRadius(radius, true));
                    }
                    action.Range = GameData.ActionDistance.GetAssociatedValue(data.TryGetValue("Range", "Medium"));
                    var spawn = data.Get<DataReference>(DatabaseFields.ActionSpawn);
                    if (spawn != null) {
                        action.AddEvent(AnimationEvents.Default, new EventSpawnProjectile(spawn.TargetID));
                    }
                    else {
                        if (generateCollision) {
                            action.AddEvent(AnimationEvents.Default, new EventGenerateCollisionEvent());
                        }
                        else {
                            var collisionType = data.TryGetValue("CollisionType", "Point");
                            var raycastSize = GameData.CollisionType.GetAssociatedValue(collisionType) * 0.01f;
                            switch (collisionType) {
                                case "Melee":
                                case "MeleeBig":
                                    action.AddEvent(AnimationEvents.CollisionOrImpact, new CameraShakeEvent(new Vector3
                                    (0, 0, 1), 4, false));
                                    break;

                            }
                            //melee or hitscan need to make that clearer
                            action.AddEvent(AnimationEvents.Default, new EventCheckRaycastCollision(action.Range, raycastSize, limitEnemy));
                        }
                    }
                    break;
                case "Shield":
                    entity.Add(
                        new BlockDamageAction(
                            config.FindString("Model", "Shield"), "Vitals.Energy",
                            data.TryGetValue("Cost", 1f), skill, PlayerControls.UseSecondary));
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
            var afx = data.GetValue<string>(DatabaseFields.ActionFx);
            if (!string.IsNullOrEmpty(afx)) {
                var actionFx = ItemPool.LoadAsset<ActionFx>(UnityDirs.ActionFx, afx);
                if (actionFx != null) {
                    if (actionFx.TryGetColor(out var actionColor)) {
                        entity.Add(new HitParticlesComponent(actionColor));
                    }
                    entity.Add(new ActionFxComponent(actionFx));
                }
            }
            var customScripting = data.Get<DataList>("ScriptedEvents");
            if (customScripting != null) {
                for (int i = 0; i < customScripting.Count; i++) {
                    var scriptingData = customScripting[i];
                    var eventType = scriptingData.TryGetValue("Event", "");
                    //var eventType = ParseUtilities.TryParseEnum(scriptingData.TryGetValue("Event", ""), ActionState.None);
                    var scripting = scriptingData.TryGetValue("Script", "");
                    if (!string.IsNullOrEmpty(eventType) && !string.IsNullOrEmpty(scripting)) {
                        var customScript = ScriptingSystem.ParseMessage(scripting.SplitIntoWords());
                        if (customScript != null) {
                            action.AddEvent(eventType, customScript);
                        }
                    }
                }
            }
        }

        private HealImpact AddHealImpact(DataList config, bool self) {
            return new HealImpact(config.FindString("TargetVital", Stats.Health), config.FindFloat("Percent", 1f), self);
        }

        private AddModImpact AddModImpact(Entity entity, DataList config) {
            return new AddModImpact( config.FindFloat("Length", 1f), config.FindString("Stat", ""), config.FindFloat("Percent", 1f), 
             entity.Get<IconComponent>());
        }
    }
}
