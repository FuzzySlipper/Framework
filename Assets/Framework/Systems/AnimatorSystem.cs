using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Lowest)]
    public class AnimatorSystem : SystemBase, IMainSystemUpdate, IReceiveGlobal<PlayAnimation> {

        private ManagedArray<AnimatorData> _list;
        private ManagedArray<AnimatorData>.RunDel<AnimatorData> _del;

        public AnimatorSystem() {
            _del = CheckPlayingAnimation;
        }

        public void OnSystemUpdate(float dt) {
            if (_list == null) {
                _list = EntityController.GetComponentArray<AnimatorData>();
            }
            if (_list != null) {
                _list.Run(_del);
            }
        }

        private void CheckPlayingAnimation(AnimatorData data) {
            if (data.Event != null) {
                var msg = data.Event.Value;
                if (msg.OnEventComplete && data.Animator.IsAnimationEventComplete(msg.Clip) || data.Animator.IsAnimationComplete(msg.Clip)) {
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
                if (msg.Target == null) {
                    continue;
                }
                var anim = msg.Target.Find<AnimatorData>();
                if (anim?.Animator == null) {
                    if (msg.Event != null) {
                        msg.Event.Value.Target.Post(msg.Event.Value);
                    }
                    return;
                }
                anim.Event = msg.Event;
                anim.Animator.PlayAnimation(msg.Clip);
            }
        }
    }
}
