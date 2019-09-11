using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public partial class EntityTags : GenericEnum<EntityTags, int> {

        public const int Player = 0;
        public const int Moving = 1;
        public const int PerformingCommand = 2;
        public const int InTransition = 3;
        public const int CantMove = 4;
        public const int Disabled = 5;
        public const int NewCharacter = 6;

        public const int RotateToMoveTarget = 7;
        public const int CanUnityCollide = 8;
        public const int InStealth = 9;
        public const int IsDead = 10;
        public const int Unscaled = 11;
        public const int IsConfused = 12;
        public const int IsSlowed = 13;
        public const int IsStunned = 14;

        public override int Parse(string value, int defValue) {
            if (int.TryParse(value, out var result)) {
                return result;
            }
            return defValue;
        }

        public const int MaxTagsLimit = 30;
    }
}
