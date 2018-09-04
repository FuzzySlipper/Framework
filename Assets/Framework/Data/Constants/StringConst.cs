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
        public const string System = "System/";
    }

    public static partial class Stats {
        public const string Evasion = "Evasion";
        public const string ToHit = "ToHit";
        public const string CriticalHit = "CriticalHit";
        public const string CriticalMulti = "CriticalMulti";
        public const string Power = "Power";
        public const string Weight = "Weight";
        public const string Range = "Range";
        public const string Speed = "Speed";

        public const string BonusPowerMelee = "BonusPowerMelee";
        public const string BonusPowerRanged = "BonusPowerRanged";
        public const string BonusPowerMagic = "BonusPowerMagic";
        public const string BonusToHitMelee = "BonusToHitMelee";
        public const string BonusToHitRanged = "BonusToHitRanged";
        public const string BonusToHitMagic = "BonusToHitMagic";
        public const string BonusCritMelee = "BonusCritMelee";
        public const string BonusCritRanged = "BonusCritRanged";
        public const string BonusCritMagic = "BonusCritMagic";
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

    public static class AttackStats {
        public static string[] Power = new[] {
            Stats.BonusPowerMelee, Stats.BonusPowerRanged, Stats.BonusPowerMagic
        };
        public static string[] ToHit = new[] {
            Stats.BonusToHitMelee, Stats.BonusToHitRanged, Stats.BonusToHitMagic
        };
        public static string[] Crit = new[] {
            Stats.BonusCritMelee, Stats.BonusCritRanged, Stats.BonusCritMagic
        };

        public static string[] AllStats = new[] {
            Stats.BonusPowerMelee, Stats.BonusPowerRanged, Stats.BonusPowerMagic, Stats.BonusToHitMelee, Stats.BonusToHitRanged, Stats.BonusToHitMagic, Stats.BonusCritMelee, Stats.BonusCritRanged, Stats.BonusCritMagic,
        };
    }
}
