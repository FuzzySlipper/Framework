using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class WaitForAnimationEventPhase : ActionPhases {
        [SerializeField] private AnimationEvent.Type _animationEvent;

        public override bool CanResolve(ActionCommand cmd) {
            return cmd.Owner.AnimationEvent.CurrentAnimationEvent.EventType == _animationEvent;
        }

    }
}
