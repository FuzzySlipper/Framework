using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable, Priority(Priority.Lowest)]
	public sealed class AnimatorComponent : IComponent {
        private CachedAnimator _animator;
        public IAnimator Value { get { return _animator.Value; } }

        public AnimatorComponent(IAnimator animator) {
            _animator = new CachedAnimator(animator);
        }

        public AnimatorComponent(SerializationInfo info, StreamingContext context) {
            _animator = info.GetValue(nameof(_animator), _animator);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_animator), _animator);
        }
    }

    [System.Serializable]
	public sealed class HurtAnimation : IComponent {

        public string Clip { get; }
        public bool PauseDuring { get; }

        public HurtAnimation(string clip, bool pauseDuring) {
            Clip = clip;
            PauseDuring = pauseDuring;
        }

        public HurtAnimation(SerializationInfo info, StreamingContext context) {
            Clip = info.GetValue(nameof(Clip), Clip);
            PauseDuring = info.GetValue(nameof(PauseDuring), PauseDuring);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Clip), Clip);
            info.AddValue(nameof(PauseDuring), PauseDuring);
        }
    }

    [System.Serializable]
	public sealed class DeathAnimation : IComponent {

        public string Clip { get; }
        public bool PauseDuring { get; }

        public DeathAnimation(string clip, bool pauseDuring) {
            Clip = clip;
            PauseDuring = pauseDuring;
        }

        public DeathAnimation(SerializationInfo info, StreamingContext context) {
            Clip = info.GetValue(nameof(Clip), Clip);
            PauseDuring = info.GetValue(nameof(PauseDuring), PauseDuring);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Clip), Clip);
            info.AddValue(nameof(PauseDuring), PauseDuring);
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
        public readonly Entity Target;
        public readonly AnimatorComponent Animator;
        public readonly bool Override;
        public bool PostEvent;
        public readonly string Clip;

        public PlayAnimation(Entity target, AnimatorComponent anim, string clip, bool overrideAnim, bool postEvent) {
            Target = target;
            Clip = clip;
            Animator = anim;
            Override = overrideAnim;
            PostEvent = postEvent;
        }
    }

    [Priority(Priority.Normal)]
    public struct AnimationComplete : IEntityMessage {
        public Entity Target { get; }
        public AnimatorComponent Animator { get; }
        public string Animation { get; }

        public AnimationComplete(Entity target, AnimatorComponent animator, string animation) {
            Target = target;
            Animator = animator;
            Animation = animation;
        }
    }

    [Priority(Priority.Normal)]
    public struct AnimationEventComplete : IEntityMessage {
        public Entity Target;
        public AnimatorComponent Animator;
        public string Animation;

        public AnimationEventComplete(Entity target, AnimatorComponent animator, string animation) {
            Target = target;
            Animator = animator;
            Animation = animation;
        }
    }

    public struct AnimationEventTriggered : IEntityMessage {
        public string Event { get; }

        public AnimationEventTriggered(string @event) {
            Event = @event;
        }
    }
}
