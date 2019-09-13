using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Serializable]
    public class AnimationLayer : ActionLayer, ISerializable {

        public string Animation;

        public AnimationLayer(Action action, string animation) : base(action) {
            Animation = animation;
        }

        public AnimationLayer(SerializationInfo info, StreamingContext context) : base(info, context) {
            Animation = info.GetValue(nameof(Animation), Animation);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Animation), Animation);
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
