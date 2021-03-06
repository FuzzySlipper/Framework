﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static partial class StringConst {
        public const string TagPlayer = "Player";
        public const string TagEnemy = "Enemy";
        public const string TagEnvironment = "Environment";
        public const string TagFloor = "Floor";
        public const string TagLight = "Light";
        public const string TagInteractive = "Interactive";
        public const string TagDummy = "Dummy";
        public const string TagCreated = "Created";
        public const string TagSensor = "Sensor";
        public const string TagSpriteRenderer = "SpriteRenderer";
        public const string TagConvertedTexture = "ConvertedTexture";
        public const string TagInvalidCollider = "InvalidCollider";
        public const string TagDeleteObject = "DeleteObject";
        public const string TagDoNotDisable = "DoNotDisable";
        
        public const string MainObject = "MainSystem";

        public const char MultiEntryBreak = '|';
        public const char ChildMultiEntryBreak = '/';

        public const string ParticleUI = UnityDirs.Particles + "ParticleUI";

        public const string ItemDragDrop = UnityDirs.UI + "ItemDragDrop";

        public const string ButtonNormalAnimName = "Normal";
        public const string ButtonSelectedAnimName = "Highlighted";
        public const string ButtonPressedAnimName = "Pressed";
        public const string ButtonDisabledAnimName = "Disabled";

        public const string AudioDefaultItemClick = "Sounds/DefaultItemClick";
        public const string AudioDefaultItemReturn = "Sounds/DefaultItemReturn";

        public const string TimeUnits = "seconds";
        public const string FileExtension = ".json";
    }

    public static partial class UnityDirs {
        public const string ActionFx = "Assets/GameData/ActionFx/";
        public const string Particles = "Assets/GameData/Particles/";
        public const string ItemIcons = "Assets/GameData/Icons/Items/";
        public const string AbilityIcons = "Assets/GameData/Icons/Abilities/";
        public const string ActionSpawn = "Assets/GameData/ActionSpawn/";
        public const string Items = "Assets/GameData/Items/";
        public const string Weapons = "Assets/GameData/Weapons/";
        public const string UI = "Assets/GameData/UI/";
        public const string Materials = "Assets/GameData/Materials/";
        public const string Models = "Assets/GameData/Models/";
        public const string Dialogue = "Dialogue/";
        public const string Characters = "Assets/GameData/Characters/";
        public const string CharacterAnimations = "Assets/GameData/CharacterAnimations/";
        public const string Audio = "Assets/GameData/Audio/";
        public const string Levels = "Assets/GameData/Levels/";
        public const string Towns = "Towns/";
        public const string LevelObjects = "Assets/GameData/Levels/Objects/";
        public const string LevelTileSets = "Assets/GameData/Levels/TileSets/";
        public const string LevelMatConfigs = "Assets/GameData/MatConfigs/";
    }

    public static class Stats {
        public static readonly string Health = Vitals.GetID(0);
        public static readonly string Energy = Vitals.GetID(1);
        public const string Evasion = "Evasion";
        public const string CriticalHit = "CriticalHit";
        public const string CriticalMulti = "CriticalMulti";
        public const string Power = "Power";
        public const string Weight = "Weight";
        public const string AttackStats = "AttackStats";
        public const string BonusPowerMelee = "BonusPowerMelee";
        public const string BonusCritMelee = "BonusCritMelee";
        public const string BonusToHitMelee = "BonusToHitMelee";
        public const string BonusPowerRanged = "BonusPowerRanged";
        public const string BonusCritRanged = "BonusCritRanged";
        public const string BonusToHitRanged = "BonusToHitRanged";
        public const string BonusPowerMagic = "BonusPowerMagic";
        public const string BonusCritMagic = "BonusCritMagic";
        public const string BonusToHitMagic = "BonusToHitMagic";

        public const string Recovery = "Vitals.Recovery";
        public const string CombatRating = "CombatPower";
        public const string MaxWeight = "MaxWeight";
  
        public const string Strength = "Attributes.Strength";
        public const string Dexterity = "Attributes.Dexterity";
        public const string Constitution = "Attributes.Constitution";
        public const string Intelligence = "Attributes.Intelligence";
        public const string Wisdom = "Attributes.Wisdom";
        public const string Charisma = "Attributes.Charisma";
        public const string Level = "Level";
        public const string MoveSpeed = "DerivedStats.MoveSpeed";
        public const string WeaponAttackDamage = "DerivedStats.WeaponAttackDamage";
        public const string WeaponAttackAccuracy = "DerivedStats.WeaponAttackAccuracy";
        public const string WeaponAttackRange = "DerivedStats.WeaponAttackRange";
        public const string UnarmedAttackDamage = "DerivedStats.UnarmedAttackDamage";
        public const string UnarmedAttackRange = "DerivedStats.UnarmedAttackRange";
        public const string Reach = "DerivedStats.Reach";
        public const string Attack = "Attack";
        public const string Damage = "Damage";
    }


    public class Attributes : StringEnum<Attributes> {
        public const string Strength = "Strength";
        public const string Agility = "Agility";
        public const string Endurance = "Endurance";
        public const string Psyche = "Psyche";
        public const string Perception = "Perception";
    }
    //
    // public enum Attributes {
    //     Strength,
    //     Agility,
    //     Endurance,
    //     Psyche,
    //     Perception
    // }

    public class Vitals : StringEnum<Vitals> {
        public const string Health = "Health";
        public const string Energy = "Energy";

        public static int GetStartingValue(int attribute) {
            switch (attribute) {
                case 0:
                    return 150;
                case 1:
                    return 100;
            }
            return 0;
        }

        public static float GetRecoveryValue(int attribute) {
            switch (attribute) {
                case 0:
                    return 0;
                case 1:
                    return 0.02f;
            }
            return 0;
        }
        
    }
    
    public class Defenses : StringEnum<Defenses> {
        public const string Physical = "Physical";
        public const string Burn = "Burn";
        public const string Freeze = "Freeze";
        public const string Mental = "Mental";
        public const string Spirit = "Spirit";
    }

    public class Skills : StringEnum<Skills> {
        public const string Firearms = "Firearms";
        public const string Melee = "Melee";
        public const string Magic = "Magic";
        public const string Science = "Science";
    }

    public class Currencies : StringEnum<Currencies> {
        public const string Credits = "Credits";
        public const string Metal = "Metal";
        public const string Magic = "Magic";
        public const string Tech = "Tech";
        public const string Energy = "Energy";
        public const string RiftGas = "Rift Power";
    }

    public static partial class DatabaseSheets {
        public const string ItemModifiers = "ItemModifiers";
        public const string Items = "Items";
        public const string Equipment = "Equipment";
        public const string Weapons = "Weapons";

        public static string[] ItemSheets = new[] {
            Equipment, Weapons
        };
    }

    public static partial class DatabaseFields {
        public const string ToHit = "ToHit";
        public const string Radius = "Radius";
        public const string PowerMin = "PowerMin";
        public const string PowerMax = "PowerMax";
        public const string Components = "Components";
        public const string Type = "Type";
        public const string Component = "Component";
        public const string MaxStack = "MaxStack";
        public const string Rarity = "Rarity";
        public const string Price = "Price";
        public const string Name = "Name";
        public const string Description = "Description";
        public const string Icon = "Icon";
        public const string Model = "Model";
        public const string ModifierGroup = "ModifierGroup";
        public const string IsPrefix = "IsPrefix";
        public const string Chance = "Chance";
        public const string MinLevel = "MinLevel";
        public const string ItemType = "ItemType";
        public const string Weight = "Weight";
        public const string ActionFx = "ActionFx";
        public const string DamageType = "DamageType";
        public const string EquipmentSlot = "EquipmentSlot";
        public const string Equipment = "Equipment";
        public const string Weapons = "Weapons";
        public const string Weapon = "Weapon";
        public const string Bonuses = "Bonuses";
        public const string Bonus = "Bonus";
        public const string Stat = "Stat";
        public const string Stats = "Stats";
        public const string Vitals = "Vitals";
        public const string Skill = "Skill";
        public const string Amount = "Amount";
        public const string Multiplier = "Multiplier";
        public const string AddToEquipList = "AddToEquipList";
        public const string Config = "Config";
        public const string Recovery = "Recovery";
        public const string ActionSpawn = "ActionSpawn";
        public const string Speed = "Speed";
        public const string Rotation = "Rotation";
        public const string Animation = "Animation";
        public const string CollisionDistance = "CollisionDistance";
        public const string Value = "Value";
        public const string ID = "id";
        public const string Timeout = "Timeout";
        public const string CritChance = "CritChance";
        public const string CritMulti = "CritMulti";
    }

    public partial class GraphNodeTags : GenericEnum<GraphNodeTags, string> {
        public const string None = "";
        public const string Idle = "Idle";
        public const string GetHit = "GetHit";
        public const string Action = "Action";
        public const string Death = "Death";
        public const string Move = "Move";
        public const string ChangingEquipment = "ChangingEquipment";

        public override string Parse(string value, string defaultValue) {
            return value;
        }
    }

    public partial class AbilityTypes : GenericEnum<AbilityTypes, string> {
        public const string None = "";
        public const string Attack = "Attack";
        public override string Parse(string value, string defaultValue) {
            return value;
        }
    }

    public class GraphVariables : GenericEnum<GraphVariables, string> {
        public const string None = "";
        public const string Reloading = "Reloading";
        public const string Attacking = "Attacking";
        public const string UsingAbility = "UsingAbility";
        public const string IsMoving = "IsMoving";
        public const string Equipment = "Equipment";
        public const string WeaponModel = "WeaponModel";
        public const string PainChance = "PainChance";
        
        public override string Parse(string value, string defaultValue) {
            return value;
        }
    }

    public class GraphTriggers : GenericEnum<GraphTriggers, string> {
        public const string None = "";
        public const string Death = "Death";
        public const string GetHit = "GetHit";
        public const string Reload = "Reload";
        public const string Attack = "Attack";
        public const string UseAbility = "UseAbility";
        public const string ChangeEquipment = "ChangeEquipment";
        public const string ResetToIdle = "ResetToIdle";
        public const string CastSpell = "CastSpell";
        public const string Punch = "Punch";

        public override string Parse(string value, string defaultValue) {
            return value;
        }
    }

//    public partial class GraphTags : GenericEnum<GraphVariables, string> {
//        public const string None = "";
//        public const string Idle = "Idle";
//        public const string GetHit = "GetHit";
//        public const string Action = "Action";
//        public const string Death = "Death";
//        public const string Move = "Move";
//        public const string Attack = "Attack";
//        public const string CastSpell = "CastSpell";
//        public const string RangedAttack = "RangedAttack";
//        public const string SpecialAttack = "SpecialAttack";
//
//        public override string Parse(string value, string defaultValue) {
//            return value;
//        }
//    }

    public partial class AnimationEvents : GenericEnum<AnimationEvents, string> {
        public const string None = "";
        public const string ActionStart = "ActionStart";
        public const string ActionStop = "ActionStop";
        public const string Default = "Default";
        public const string FxOn = "FxOn";
        public const string FxOff = "FxOff";
        public const string ShakeRightTop = "ShakeRightTop";
        public const string ShakeRightMiddle = "ShakeRightMiddle";
        public const string ShakeRightBottom = "ShakeRightBottom";
        public const string ShakeLeftTop = "ShakeLeftTop";
        public const string ShakeLeftMiddle = "ShakeLeftMiddle";
        public const string ShakeLeftBottom = "ShakeLeftBottom";
        public const string ShakeTop = "ShakeTop";
        public const string ShakeBottom = "ShakeBottom";
        public const string PullRightTop = "PullRightTop";
        public const string PullRightMiddle = "PullRightMiddle";
        public const string PullRightBottom = "PullRightBottom";
        public const string PullLeftTop = "PullLeftTop";
        public const string PullLeftMiddle = "PullLeftMiddle";
        public const string PullLeftBottom = "PullLeftBottom";
        public const string PullTop = "PullTop";
        public const string PullBottom = "PullBottom";
        public const string Reload = "Reload";
        public const string Miss = "Miss";
        public const string Impact = "Impact";
        public const string Collision = "Collision";
        public const string CollisionOrImpact = "CollisionOrImpact";
        public const string StopMovement = "StopMovement";
        public const string AllowMovement = "AllowMovement";
        public const string Dead = "Dead";

        public static ActionState ToStateEvent(string eventName) {
            switch (eventName) {
                case AnimationEvents.FxOn:
                    return ActionState.FxOn;
                case AnimationEvents.FxOff:
                    return ActionState.FxOff;
                case AnimationEvents.Default:
                    return ActionState.Activate;
                case AnimationEvents.ActionStart:
                    return ActionState.Start;
            }
            return ActionState.None;
        }

        public override string Parse(string value, string defaultValue) {
            return value;
        }
    }

    public partial class PlayerAnimationIds : StringEnum<PlayerAnimationIds> {
        public const string Idle = "Idle";
        public const string IdleMelee1H = "IdleMelee1H";
        public const string IdleMelee2H = "IdleMelee2H";
        public const string GetHit = "GetHit";
        public const string Death = "Death";
        public const string Move = "Move";
        public const string Swing = "Swing";
        public const string SwingHeavy = "SwingHeavy";
        public const string Thrust = "Thrust";
        public const string Bash = "Bash";
        public const string Swing2H = "Swing2H";
        public const string SwingHeavy2H = "SwingHeavy2H";
        public const string Thrust2H = "Thrust2H";
        public const string Bash2H = "Bash2H";
        public const string Throw = "Throw";
        public const string CastSpell = "CastSpell";
        public const string Punch = "Punch";
        public const string Shoot1H = "Shoot1H";
        public const string Shoot2H = "Shoot2H";
        public const string Idle1H = "Idle1H";
        public const string Idle2H = "Idle2H";
        public const string Aim1H = "Aim1H";
        public const string Aim2H = "Aim2H";
        public const string AimFire1H = "AimFire1H";
        public const string AimFire2H = "AimFire2H";
        public const string LoopCastStart = "LoopCastStart";
        public const string LoopCast = "LoopCast";
        public const string LoopCastEnd = "LoopCastEnd";
    }

    public class WeaponTypes : StringEnum<WeaponTypes> {
        public const string Melee = "Melee";
        public const string Ranged = "Ranged";
    }
    //public static class AttackStats {
    //    public static string[] Power = new[] {
    //        Stats.BonusPowerMelee, Stats.BonusPowerRanged, Stats.BonusPowerMagic
    //    };
    //    public static string[] ToHit = new[] {
    //        Stats.BonusToHitMelee, Stats.BonusToHitRanged, Stats.BonusToHitMagic
    //    };
    //    public static string[] Crit = new[] {
    //        Stats.BonusCritMelee, Stats.BonusCritRanged, Stats.BonusCritMagic
    //    };

    //    public static string[] AllStats = new[] {
    //        Stats.BonusPowerMelee, Stats.BonusPowerRanged, Stats.BonusPowerMagic, Stats.BonusToHitMelee, Stats.BonusToHitRanged, Stats.BonusToHitMagic, Stats.BonusCritMelee, Stats.BonusCritRanged, Stats.BonusCritMagic,
    //    };
    //}
}
