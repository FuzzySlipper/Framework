using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static partial class EntityTags {
        public const int Moving = 0;
        public const int PerformingCommand = 1;
        public const int InTransition = 2;
        public const int CantMove = 5;
        public const int Disabeled = 6;
        public const int NewCharacter = 7;

        public const int RotateToMoveTarget = 10;
        public const int CheckingCollision = 11;
        public const int CanUnityCollide = 12;
        public const int InStealth = 13;
        public const int IsDead = 14;
        public const int Unscaled = 15;
        public const int Slowed = 16;

        public const int MAXLIMIT = 41;
    }
}
