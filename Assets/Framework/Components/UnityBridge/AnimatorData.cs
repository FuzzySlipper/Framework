using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable, Priority(Priority.Lowest)]
    public class AnimatorData : IComponent {
        public IAnimator Animator;
        public int Owner { get; set; }

        public AnimatorData(IAnimator animator) {
            Animator = animator;
            Owner = -1;
        }
        
        public void Dispose() {
            Animator = null;
        }
    }

    public class HurtAnimation : IComponent, IReceive<DamageEvent> {

        public int Owner { get;set; }
        private string _animation;
        private IAnimator _animator;

        public HurtAnimation(string animation, IAnimator animator) {
            _animation = animation;
            _animator = animator;
        }

        public void Handle(DamageEvent arg) {
            if (arg.Amount > 0 && _animator != null) {
                _animator.PlayAnimation(_animation, false, null);
            }
        }
    }

    public class DeathAnimation : IComponent, IReceive<DeathEvent> {

        public int Owner { get; set; }
        private string _animation;
        private IAnimator _animator;

        public DeathAnimation(string animation, IAnimator animator) {
            _animation = animation;
            _animator = animator;
        }

        public void Handle(DeathEvent arg) {
            if (_animator != null) {
                _animator.PlayAnimation(_animation, true, null);
            }
        }
    }

    public interface IAnimator {
        string CurrentAnimationEvent { get; }
        string CurrentAnimation { get; }
        float CurrentAnimationLength { get; }
        float CurrentAnimationRemaining { get; }
        Vector3 GetEventPosition { get; }
        Quaternion GetEventRotation { get; }
        bool IsAnimationComplete(string clip);
        bool IsAnimationEventComplete(string clip);
        void PlayAnimation(string clip, bool overrideClip, Action action);
    }


    [Priority(Priority.Lowest)]
    public struct PlayAnimation : IEntityMessage {
        public Entity Target;
        public AnimatorData Animator;
        public bool Override;
        public bool PostEvent;
        public string Clip;

        public PlayAnimation(Entity target, AnimatorData anim, string clip, bool overrideAnim, bool postEvent) {
            Target = target;
            Clip = clip;
            Animator = anim;
            Override = overrideAnim;
            PostEvent = postEvent;
        }
    }

    [Priority(Priority.Normal)]
    public struct AnimationComplete : IEntityMessage {
        public Entity Target;
        public AnimatorData Animator;
        public string Animation;

        public AnimationComplete(Entity target, AnimatorData animator, string animation) {
            Target = target;
            Animator = animator;
            Animation = animation;
        }
    }

    [Priority(Priority.Normal)]
    public struct AnimationEventComplete : IEntityMessage {
        public Entity Target;
        public AnimatorData Animator;
        public string Animation;

        public AnimationEventComplete(Entity target, AnimatorData animator, string animation) {
            Target = target;
            Animator = animator;
            Animation = animation;
        }
    }
}
