using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class AnimationLayer : ActionLayer {

        public string Animation;

        public AnimationLayer(Action action, string animation) : base(action) {
            Animation = animation;
        }

        public override void Start(ActionUsingNode node) {
            base.Start(node);
            node.LastProcessedAnimationEvent = "";
            node.CurrentState = ActionUsingNode.State.Starting;
            node.Animator.PlayAnimation(Animation, true, node.ActionEvent.Action);
        }

        public override void Evaluate(ActionUsingNode node) {
            switch (node.CurrentState) {
                case ActionUsingNode.State.Disabled:
                    return;
                case ActionUsingNode.State.Starting:
                    if (node.Animator.CurrentAnimation == Animation) {
                        node.CurrentState = ActionUsingNode.State.Running;
                    }
                    else {
                        return;
                    }
                    break;
                case ActionUsingNode.State.Running:
                    if (node.Animator.CurrentAnimation != Animation) {
                        World.Get<ActionSystem>().AdvanceEvent(node);
                        return;
                    }
                    break;
            }
            if (node.LastProcessedAnimationEvent != node.Animator.CurrentAnimationEvent) {
                node.LastProcessedAnimationEvent = node.Animator.CurrentAnimationEvent;
                if (!string.IsNullOrEmpty(node.LastProcessedAnimationEvent)) {
                    node.ActionEvent.Current.PostAnimationEvent(node, node.LastProcessedAnimationEvent);
                }
            }
            if (node.Animator.CurrentAnimationRemaining <= 0) {
                node.AdvanceEvent();
            }
        }
    }
}
