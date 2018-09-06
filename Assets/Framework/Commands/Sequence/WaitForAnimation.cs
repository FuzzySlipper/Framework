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
            entity.AddObserver(this);
            if (!entity.Find<AnimatorData>(
                a => {
                    var target = a.GetEntity();
                    entity.AddObserver(this);
                    target.Post(new PlayAnimation(target, _animationClip, new AnimatorEvent(entity, _animationClip, _onEvent, _onComplete)));
                })) {
                Owner.DefaultPostAdvance(this);
            }
        }

        public void Handle(AnimatorEvent arg) {
            arg.Target.RemoveObserver(this);
            Owner.DefaultPostAdvance(this);
        }
    }
}
