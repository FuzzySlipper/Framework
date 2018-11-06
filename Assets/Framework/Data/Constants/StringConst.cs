using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static partial class StringConst {
        public const string TagPlayer = "Player";
        public const string TagEnemy = "Enemy";
        public const string TagEnvironment = "Environment";
        public const string TagLight = "Light";
        public const string TagInteractive = "Interactive";
        public const string TagDummy = "Dummy";
        public const string TagConvertedTexture = "ConvertedTexture";

        public const string MainObject = "MainSystem";

        public const char MultiEntryBreak = '|';
        public const char ChildMultiEntryBreak = '/';

        public const string PathUI = "UI/";

        public const string ParticleUI = "Particles/ParticleUI";

        public const string ItemDragDrop = "UI/ItemDragDrop";

        public const string ButtonNormalAnimName = "Normal";
        public const string ButtonSelectedAnimName = "Highlighted";
        public const string ButtonPressedAnimName = "Pressed";
        public const string ButtonDisabledAnimName = "Disabled";

        public const string AudioDefaultItemClick = "Sounds/DefaultItemClick";
        public const string AudioDefaultItemReturn = "Sounds/DefaultItemReturn";
    }

    public static partial class UnityDirs {
        public const string EditorFolder = "Assets/GameData/Resources/";
        public const string System = "Systems/";
        public const string ActionFx = "ActionFx/";
        public const string Icons = "Icons/";
        public const string ItemIcons = "Icons/Items/";
        public const string AbilityIcons = "Icons/Abilities/";
        public const string Projectiles = "Projectiles/";
        public const string Items = "Items/";
        public const string UI = "UI/";
    }

    public static partial class Stats {
        public const string Evasion = "Evasion";
        public const string ToHit = "ToHit";
        public const string CriticalHit = "CriticalHit";
        public const string CriticalMulti = "CriticalMulti";
        public const string Power = "Power";
        public const string Weight = "Weight";
        public const string Range = "Range";
        public const string AttackStats = "AttackStats";
    }

    public static partial class DatabaseSheets {
        public const string ItemModifiers = "ItemModifiers";
        public const string Items = "Items";
        public const string Equipment = "Equipment";
        public const string Weapons = "Weapons";

        public static string[] ItemSheets = new[] {
            Items, Equipment, Weapons
        };
    }

    public static partial class DatabaseFields {
        public const string Components = "Components";
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
        public const string ActionFX = "ActionFX";
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
        public const string Amount = "Amount";
        public const string Multiplier = "Multiplier";
        public const string AddToEquipList = "AddToEquipList";
        public const string Config = "Config";
        public const string Recovery = "Recovery";
        public const string Projectile = "Projectile";
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

    public partial class AnimationIds : GenericEnum<AnimationIds, string> {
        public const string Idle = "Idle";
        public const string GetHit = "GetHit";
        public const string Action = "Action";
        public const string Death = "Death";
        public const string Move = "Move";
        public const string Attack = "Attack";

        public override string Parse(string value, string defaultValue) {
            return value;
        }
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
