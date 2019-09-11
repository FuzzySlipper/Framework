using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Lowest)]
    public class AnimatorSystem : SystemBase, IMainSystemUpdate, IReceiveGlobal<PlayAnimation> {

        private BufferedList<PlayAnimation> _animations = new BufferedList<PlayAnimation>();

        public AnimatorSystem() {
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            _animations.Swap();
            for (int i = 0; i < _animations.PreviousList.Max; i++) {
                if (_animations.PreviousList.IsInvalid(i)) {
                    continue;
                }
                ref var anim = ref _animations.PreviousList[i];
                if (anim.Animator?.Animator == null) {
                    _animations.CurrentList.Remove(i);
                    continue;
                }
                if (anim.PostEvent && anim.Animator.Animator.IsAnimationEventComplete(anim.Clip)) {
                    anim.PostEvent = false;
                    anim.Target.Post(new AnimationEventComplete(anim.Target, anim.Animator, anim.Clip));
                }
                if (anim.Animator.Animator.IsAnimationEventComplete(anim.Clip)) {
                    anim.Target.Post(new AnimationComplete(anim.Target, anim.Animator, anim.Clip));
                    _animations.CurrentList.Remove(i);
                }
            }
        }

        public void HandleGlobal(PlayAnimation msg) {
            if (msg.Animator?.Animator == null) {
                return;
            }
            msg.Animator.Animator.PlayAnimation(msg.Clip, msg.Override, null);
            _animations.Add(msg);
        }
    }
}
