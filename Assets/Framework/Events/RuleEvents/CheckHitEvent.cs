using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct CheckHitEvent : IRuleEvent {
<<<<<<< HEAD
        public BaseActionTemplate Action { get; }
=======
        public ActionTemplate Action { get; }
>>>>>>> FirstPersonAction
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public string TargetDefense { get; }
        public float DefenseTotal;
        public float AttackTotal;
<<<<<<< HEAD
        public int Result;

        public CheckHitEvent(BaseActionTemplate action, CharacterTemplate origin, CharacterTemplate target, string targetDefense) {
=======
        public CollisionResult Result;

        public CheckHitEvent(ActionTemplate action, CharacterTemplate origin, CharacterTemplate target, string targetDefense) {
>>>>>>> FirstPersonAction
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
