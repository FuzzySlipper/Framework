using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
    public class WaitForAnimationEventPhase : ActionPhases {
        [SerializeField] private AnimationEvent.Type _animationEvent = AnimationEvent.Type.Default;

        public override bool CanResolve(ActionCommand cmd) {
            return cmd.Owner.AnimationEvent.CurrentAnimationEvent.EventType == _animationEvent;
        }

    }
}
