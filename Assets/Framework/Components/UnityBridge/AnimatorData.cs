using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
    public struct AnimatorData : IComponent {
        public IAnimator Animator;
        public AnimatorEvent? Event;
        public int Owner { get; set; }

        public AnimatorData(IAnimator animator) {
            Animator = animator;
            Event = null;
            Owner = -1;
        }
        
        public void Dispose() {
            Animator = null;
        }
    }

    public interface IAnimator {
        bool IsAnimationComplete();
        bool IsAnimationComplete(string clip);
        bool IsAnimationEventComplete(string clip);
        string CurrentAnimation { get; }
        void PlayAnimation(string clip);
    }

    [Priority(Priority.Normal)]
    public struct AnimatorEvent : IEntityMessage {
        public Entity Target;
        public string Clip;
        public bool OnEventComplete;
        public bool OnAnimationComplete;

        public AnimatorEvent(Entity target, string clip, bool onEventComplete, bool onAnimationComplete) {
            Target = target;
            Clip = clip;
            OnEventComplete = onEventComplete;
            OnAnimationComplete = onAnimationComplete;
        }
    }

    [Priority(Priority.Lowest)]
    public struct PlayAnimation : IEntityMessage {
        public Entity Target;
        public string Clip;
        public AnimatorEvent? Event;

        public PlayAnimation(Entity target, string clip, AnimatorEvent? @event) {
            Target = target;
            Clip = clip;
            Event = @event;
        }
    }

    [Priority(Priority.Highest)]
    public struct AnimationComplete : IEntityMessage {
        public string Clip;
        public Entity Target;
    }

    public static partial class AnimatorClips {
        public const string Move = "Move";
        public const string GetHit = "GetHit";
        public const string Action = "Action";
        public const string SpecialAction = "SpecialAction";
        public const string Death = "Death";
        public const string Idle = "Idle";
    }
}
