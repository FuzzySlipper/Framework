using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class ActorAnimator : MonoBehaviour {

        public virtual bool AnimFinished(ActorAnimations anim) {
            return true;
        }

        public virtual bool AnimEventFinished(ActorAnimations anim) {
            return true;
        }

        public virtual float GetAnimationLength(ActorAnimations anim) {
            return 0.5f;
        }

        public virtual bool AnimFinished(AnimationClip anim) {
            return true;
        }
        public virtual bool AnimEventFinished(AnimationClip anim) {
            return true;
        }

        public virtual ActorAnimations CurrentAnimation{get { return ActorAnimations.Idle; }}
        public virtual void PlayOverrideAnimation(ActorAnimations anim, bool unscaled) {}
        public virtual void ApplyMaterialBlocks(MaterialPropertyBlock[] blocks) { }
        public virtual MaterialPropertyBlock[] GetMatBlocks { get { return null; } }
        public virtual Renderer[] GetRenderers{ get { return null; } }
        public virtual void SetMaterialKeyword(string keyword, bool status) { }
        public abstract Transform AnimTr { get; }
    }
}