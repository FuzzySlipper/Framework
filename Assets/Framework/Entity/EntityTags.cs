using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static class EntityTags {
        public const int Moving = 0;
        public const int PerformingCommand = 1;
        public const int InTransition = 2;
        public const int CantMove = 5;
        public const int Disabeled = 6;
        public const int NewCharacter = 7;

        public const int RotateToMoveTarget = 10;
        
        public const int CheckingCollision = 20;
        public const int CanUnityCollide = 21;

        public const int InStealth = 30;
        public const int IsDead = 31;

        public const int Unscaled = 50;

        public const int MAXLIMIT = 51;
    }
}
