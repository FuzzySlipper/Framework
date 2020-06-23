using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct CheckHitEvent : IRuleEvent {
        public ActionTemplate Action { get; }
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public string TargetDefense { get; }
        public float DefenseTotal;
        public float AttackTotal;
        public CollisionResult Result;

        public CheckHitEvent(ActionTemplate action, CharacterTemplate origin, CharacterTemplate target, string targetDefense) {
            Action = action;
            Origin = origin;
            Target = target;
            TargetDefense = targetDefense;
            DefenseTotal = 0;
            AttackTotal = 0;
            Result = 0;
        }
    }
}
