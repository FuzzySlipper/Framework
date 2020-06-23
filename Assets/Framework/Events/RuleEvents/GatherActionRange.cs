using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class GatherActionRange : IRuleEvent {
<<<<<<< HEAD
        public BaseActionTemplate Action { get; }
=======
        public ActionTemplate Action { get; }
>>>>>>> FirstPersonAction
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public int Range;

<<<<<<< HEAD
        public GatherActionRange(BaseActionTemplate action, CharacterTemplate origin, CharacterTemplate target, int range) {
=======
        public GatherActionRange(ActionTemplate action, CharacterTemplate origin, CharacterTemplate target, int range) {
>>>>>>> FirstPersonAction
            Action = action;
            Origin = origin;
            Target = target;
            Range = range;
        }
    }
}
