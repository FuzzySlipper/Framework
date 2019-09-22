using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Lowest)]
    public class AnimatorSystem : SystemBase, IMainSystemUpdate, IReceiveGlobal<PlayAnimation>, IReceive<DamageEvent>,
        IReceive<DeathEvent> {

        private BufferedList<PlayAnimation> _animations = new BufferedList<PlayAnimation>();
        private BufferedList<PauseMovementForClip> _moveClips = new BufferedList<PauseMovementForClip>();

        public AnimatorSystem() {
            EntityController.RegisterReceiver<HurtAnimation>(this);
            EntityController.RegisterReceiver<DeathAnimation>(this);
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            for (int i = 0; i < _animations.Count; i++) {
                ref var anim = ref _animations[i];
                if (anim.Animator?.Value == null) {
                    _animations.Remove(anim);
                    continue;
                }
                if (anim.PostEvent && anim.Animator.Value.IsAnimationEventComplete(anim.Clip)) {
                    anim.PostEvent = false;
                    anim.Target.Post(new AnimationEventComplete(anim.Target, anim.Animator, anim.Clip));
                }
                if (anim.Animator.Value.IsAnimationEventComplete(anim.Clip)) {
                    anim.Target.Post(new AnimationComplete(anim.Target, anim.Animator, anim.Clip));
                    _animations.Remove(anim);
                }
            }
            for (int i = 0; i < _moveClips.Count; i++) {
                var node = _moveClips[i];
                if (node.Animator == null) {
                    _moveClips.Remove(node);
                    continue;
                }
                if (node.Animator.Value.IsAnimationComplete(node.Clip)) {
                    var entity = node.Animator.GetEntity();
                    if (entity != null && !entity.IsDestroyed()) {
                        entity.Tags.Remove(EntityTags.CantMove);
                    }
                    _moveClips.Remove(node);
                }
            }
        }

        public void HandleGlobal(PlayAnimation msg) {
            if (msg.Animator?.Value == null) {
                return;
            }
            msg.Animator.Value.PlayAnimation(msg.Clip, msg.Override, null);
            _animations.Add(msg);
        }

        public void Handle(DamageEvent arg) {
            if (arg.Amount <= 0) {
                return;
            }
            var hurtAnimation = arg.Target.Entity.Get<HurtAnimation>();
            if (hurtAnimation != null) {
                arg.Target.Animator.Value.PlayAnimation(hurtAnimation.Clip, hurtAnimation.PauseDuring, null);
                if (hurtAnimation.PauseDuring) {
                    arg.Target.Tags.Add(EntityTags.CantMove);
                    _moveClips.Add(new PauseMovementForClip(arg.Target.Animator, hurtAnimation.Clip));
                }
            }
        }

        public void Handle(DeathEvent arg) {
            var deathAnimation = arg.Target.Entity.Get<DeathAnimation>();
            if (deathAnimation != null) {
                arg.Target.Animator.Value.PlayAnimation(deathAnimation.Clip, deathAnimation.PauseDuring, null);
                if (deathAnimation.PauseDuring) {
                    arg.Target.Tags.Add(EntityTags.CantMove);
                    _moveClips.Add(new PauseMovementForClip(arg.Target.Animator, deathAnimation.Clip));
                }
            }
        }

        private struct PauseMovementForClip {
            public AnimatorComponent Animator;
            public string Clip;

            public PauseMovementForClip(AnimatorComponent animator, string clip) {
                Animator = animator;
                Clip = clip;
            }
        }
    }
}
