using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Lowest)]
    public class AnimatorSystem : SystemBase, IMainSystemUpdate, IReceiveGlobal<PlayAnimation> {


        public void OnSystemUpdate(float dt) {
            var list = EntityController.GetComponentArray<AnimatorData>();
            if (list != null) {
                list.RunAction(CheckPlayingAnimation);
            }
        }

        private void CheckPlayingAnimation(AnimatorData data) {
            if (data.Event != null) {
                var msg = data.Event.Value;
                if (msg.OnEventComplete && data.Animator.IsAnimationEventComplete(msg.Clip) || data.Animator.IsAnimationComplete(msg.Clip)) {
                    data.Event.Value.Post(data.Event.Value.Target);
                    msg.Target.Post(data.Event.Value);
                    data.Event = null;
                }
            }
            if (data.Animator.IsAnimationComplete()) {
                data.Animator.PlayAnimation(data.GetEntity().Tags.Contain(EntityTags.Moving) ? AnimatorClips.Move : AnimatorClips.Idle);
            }
        }

        public void HandleGlobal(List<PlayAnimation> arg) {
            for (int i = 0; i < arg.Count; i++) {
                var msg = arg[i];
                var anim = msg.Target.Get<AnimatorData>();
                if (anim.Animator == null) {
                    anim = msg.Target.GetParent()?.Get<AnimatorData>() ?? anim;
                }
                if (anim.Animator == null) {
                    if (msg.Event != null) {
                        msg.Event.Value.Target.Post(msg.Event.Value);
                    }
                    return;
                }
                anim.Animator.PlayAnimation(msg.Clip);
                anim.Event = msg.Event;
            }
        }
    }
}
