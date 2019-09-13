using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable, Priority(Priority.Lowest)]
	public sealed class AnimatorData : IComponent {
        private CachedAnimator _animator;
        public IAnimator Animator { get { return _animator.Value; } }

        public AnimatorData(IAnimator animator) {
            _animator = new CachedAnimator(animator);
        }

        public AnimatorData(SerializationInfo info, StreamingContext context) {
            _animator = info.GetValue(nameof(_animator), _animator);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_animator), _animator);
        }
    }

    [System.Serializable]
	public sealed class HurtAnimation : IComponent, IReceive<DamageEvent> {

        private string _animation;
        private CachedAnimator _animator;

        public HurtAnimation(string animation, IAnimator animator) {
            _animation = animation;
            _animator = new CachedAnimator(animator);
        }

        public void Handle(DamageEvent arg) {
            if (arg.Amount > 0 && _animator != null) {
                _animator.Value.PlayAnimation(_animation, false, null);
            }
        }

        public HurtAnimation(SerializationInfo info, StreamingContext context) {
            _animator = info.GetValue(nameof(_animator), _animator);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_animator), _animator);
        }
    }

    [System.Serializable]
	public sealed class DeathAnimation : IComponent, IReceive<DeathEvent> {

        private string _animation;
        private CachedAnimator _animator;

        public DeathAnimation(string animation, IAnimator animator) {
            _animation = animation;
            _animator =  new CachedAnimator(animator);
        }

        public void Handle(DeathEvent arg) {
            if (_animator != null) {
                _animator.Value.PlayAnimation(_animation, true, null);
            }
        }

        public DeathAnimation(SerializationInfo info, StreamingContext context) {
            _animator = info.GetValue(nameof(_animator), _animator);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_animator), _animator);
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
