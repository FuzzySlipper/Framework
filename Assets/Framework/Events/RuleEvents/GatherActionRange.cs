using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class GatherActionRange : IRuleEvent {
        public BaseActionTemplate Action { get; }
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public int Range;

        public GatherActionRange(BaseActionTemplate action, CharacterTemplate origin, CharacterTemplate target, int range) {
            Action = action;
            Origin = origin;
            Target = target;
            Range = range;
        }
    }
}
