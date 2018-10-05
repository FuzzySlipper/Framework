using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class WaitForAnimation : ICommandElement, IReceive<AnimatorEvent> {

        public CommandSequence Owner { get; set; }
        public ActionStateEvents StateEvent { get; }

        private string _animationClip;
        private bool _onEvent;
        private bool _onComplete;

        public WaitForAnimation(ActionStateEvents statEvent, string animationClip, bool onEvent, bool onComplete) {
            _animationClip = animationClip;
            _onEvent = onEvent;
            _onComplete = onComplete;
            StateEvent = statEvent;
        }

        public void Start(Entity entity) {
            var anim = entity.Get<AnimatorData>();
            if (anim != null) {
                var target = anim.GetEntity();
                target.AddObserver(this);
                target.Post(new PlayAnimation(target, _animationClip, new AnimatorEvent(target, _animationClip, _onEvent, _onComplete)));
            }
            else {
                Owner.DefaultPostAdvance(this);
            }
        }

        public void Handle(AnimatorEvent arg) {
            arg.Target.RemoveObserver(this);
            Owner.DefaultPostAdvance(this);
        }
    }
}
