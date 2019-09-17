using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelComrades.DungeonCrawler;

namespace PixelComrades {
    public class ActionProvider : IDataFactory<Action> {
        private static GameOptions.CachedFloat _defaultAnimationTimeout = new GameOptions.CachedFloat("DefaultAnimationTimeout");
        private static GameOptions.CachedFloat _brokenWeaponPercent = new GameOptions.CachedFloat("WeaponBrokenPercentDamage");

        private const float EffectTime = 3f;
        private const float EffectChance = 10f;
        

        public void AddComponent(Entity entity, DataEntry data) {
            var action = entity.Add(new Action());
            
            var type = data.Get<DataReference>(DatabaseFields.ItemType);
            var skill = data.TryGetValue<string>(DatabaseFields.Skill, "");
            action.Primary = type?.TargetID == "WeaponUsable";
            action.WeaponModel = data.TryGetValue("WeaponModel", "");
            List<IActionImpact> impacts = new List<IActionImpact>();
            var power = new RangeStat(entity, Stats.Power, Stats.Power, data.TryGetValue(DatabaseFields.PowerMin, 0f), data.TryGetValue
            (DatabaseFields.PowerMax, 1f));
            var stats = entity.Get<StatsContainer>();
            stats.Add(power);
            var animation = data.TryGetValue("Animation", "");
            bool generateCollision = false;
            var targeting = ParseUtilities.TryParseEnum(data.TryGetValue("TargetType", "Enemy"), TargetType.Enemy);
            if (targeting == TargetType.Self || targeting == TargetType.Friendly) {
                generateCollision = true;
            }
            bool limitEnemy = true;
            var config = data.Get<DataList>("Config");
            var abilityType = data.TryGetValue("Type", "Attack");
            if (type != null && type.TargetID == "Ability") {
                var secondaryType = data.TryGetValue("SecondaryType", "");
                switch (abilityType) {
                    default:
                    case "Attack":
                        impacts.Add(
                            new DamageImpact(data.TryGetValue(
                                    DatabaseFields.DamageType,
                                    GameData.DamageTypes.GetID(0)), Stats.Health, 1, power));
                        break;
                    case "Heal":
                        impacts.Add(AddHealImpact( config, power, false));
                        generateCollision = true;
                        limitEnemy = false;
                        break;
                    case "AddModImpact":
                        impacts.Add(AddModImpact(entity, config, power));
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
                        impacts.Add(AddHealImpact(config, power, true));
                        break;
                    case "AddModImpact":
                        impacts.Add(AddModImpact(entity, config, power));
                        break;
                    case "ConvertVital":
                        impacts.Add(new ConvertVitalImpact(config.FindFloat("Percent", 1f), config.FindString("SourceVital"), config.FindString("TargetVital")));
                        break;
                    case "InstantKill":
                        impacts.Add(new InstantKill(config.FindFloat("Chance", 1f)));
                        break;
                    case "Confuse":
                        impacts.Add(new ConfuseImpact(data.TryGetValue("SecondaryPower", EffectChance), config.FindFloat("Length", EffectTime)));
                        break;
                    case "Slow":
                        impacts.Add(new SlowImpact(data.TryGetValue("SecondaryPower", EffectChance), config.FindFloat("Length", EffectTime)));
                        break;
                    case "Stun":
                        impacts.Add(new StunImpact(data.TryGetValue("SecondaryPower", EffectChance), config.FindFloat("Length", EffectTime)));
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
                impacts.Add(
                    new DamageImpact(data.TryGetValue(DatabaseFields.DamageType,
                            GameData.DamageTypes.GetID(0)), Stats.Health, 1, power));
                var reload = data.TryGetValue("ReloadType", "Repair");
                var reloadSpeed = data.TryGetValue("ReloadSpeed", 1f);
                var ammo = AmmoFactory.GetTemplate(data.Get<DataReference>("Ammo"));
                switch (reload) {
                    case "Repair":
                        action.Ammo = entity.Add(new AmmoComponent(ammo, skill, reloadSpeed, power, _brokenWeaponPercent));
                        break;
                    case "Reload":
                        action.Ammo = entity.Add(new AmmoComponent(ammo, skill, reloadSpeed, null));
                        break;

                }
                action.Ammo.Amount.SetLimits(0, data.TryGetValue("AmmoAmount", 5));
                action.Ammo.Amount.SetMax();
                action.Costs.Add(new CostAmmo(action.Ammo));
            }
            ActionLayer mainLayer;
            switch (abilityType) {
                default:
                    mainLayer = new AnimationLayer(action, animation);
                    var radius = ParseUtilities.TryParseEnum(data.TryGetValue(DatabaseFields.Radius, "Single"), ImpactRadiusTypes.Single);
                    if (radius != ImpactRadiusTypes.Single) {
                        impacts.Add(new ImpactRadius(radius));
                    }
                    action.Range = GameData.ActionDistance.GetAssociatedValue(data.TryGetValue("Range", "Medium"));
                    var spawn = data.Get<DataReference>(DatabaseFields.ActionSpawn);
                    if (spawn != null) {
                        mainLayer.Events.Add(AnimationEvents.Default, new EventSpawnProjectile(ActionStateEvents.None, impacts, spawn.TargetID));
                    }
                    else {
                        if (generateCollision) {
                            mainLayer.Events.Add(AnimationEvents.Default, new EventGenerateCollisionEvent(ActionStateEvents.None, impacts));
                        }
                        else {
                            var collisionType = data.TryGetValue("CollisionType", "Point");
                            var raycastSize = GameData.CollisionType.GetAssociatedValue(collisionType) * 0.01f;
                            switch (collisionType) {
                                case "Melee":
                                case "MeleeBig":
                                    mainLayer.ScriptedEvents.Add(new CameraShakeEvent(ActionStateEvents.CollisionOrImpact, new Vector3(0, 0, 1), 4, false));
                                    break;

                            }
                            //melee or hitscan need to make that clearer
                            mainLayer.Events.Add(AnimationEvents.Default, new EventCheckRaycastCollision(ActionStateEvents.None, impacts, action.Range, raycastSize, limitEnemy));
                        }
                    }
                    action.Sequence.Add(mainLayer);
                    break;
                case "Shield":
                    action.Sequence.Add(new AnimationLayer(action, PlayerAnimationIds.LoopCastStart));
                    action.Sequence.Add(new AnimationLayer(action, PlayerAnimationIds.LoopCast));
                    mainLayer = new BlockDamageLayer(action, config.FindString("Model", "Shield"), "Vitals.Energy", data.TryGetValue("Cost", 1f),skill, ActionInputControls.UseSecondary, 2f);
                    action.Sequence.Add(mainLayer);
                    action.Sequence.Add(new AnimationLayer(action, PlayerAnimationIds.LoopCastEnd));
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
                    action.Fx = actionFx;
                    if (actionFx.TryGetColor(out var actionColor)) {
                        entity.Add(new HitParticlesComponent(actionColor));
                    }
                    entity.Add(new ActionFxComponent(actionFx));
                }
            }
            mainLayer.IsMainLayer = true;
            var customScripting = data.Get<DataList>("ScriptedEvents");
            if (customScripting != null) {
                for (int i = 0; i < customScripting.Count; i++) {
                    var scriptingData = customScripting[i];
                    var eventType = ParseUtilities.TryParseEnum(scriptingData.TryGetValue("Event", ""), ActionStateEvents.None);
                    var scripting = scriptingData.TryGetValue("Script", "");
                    if (eventType != ActionStateEvents.None && !string.IsNullOrEmpty(scripting)) {
                        var customScript = ScriptingSystem.ParseMessage(eventType, scripting.SplitIntoWords());
                        if (customScript != null) {
                            mainLayer.ScriptedEvents.Add(customScript);
                        }
                    }
                }
            }
        }

        private HealImpact AddHealImpact(DataList config, BaseStat power, bool self) {
            return new HealImpact(config.FindString("TargetVital", Stats.Health), config.FindFloat("Percent", 1f), power, self);
        }

        private AddModImpact AddModImpact(Entity entity, DataList config, BaseStat power) {
            return new AddModImpact( config.FindFloat("Length", 1f), config.FindString("Stat", ""), config.FindFloat("Percent", 1f), 
            power, entity.Get<IconComponent>());
        }
    }
}
