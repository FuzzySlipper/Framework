using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class PauseMovementForAnimation : IComponent, IReceive<DamageEvent> {

        private string _damageClip;

        public PauseMovementForAnimation(SerializationInfo info, StreamingContext context) {
            _damageClip = info.GetValue(nameof(_damageClip), _damageClip);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_damageClip), _damageClip);
        }
        
        public PauseMovementForAnimation(string damageClip) {
            _damageClip = damageClip;
        }

        public void PauseForClip(string clip) {
            var entity = this.GetEntity();
            TimeManager.StartTask(PauseMovementForDamage(entity, entity.Get<AnimatorData>(), clip));
        }

        public void Handle(DamageEvent arg) {
            var entity = this.GetEntity();
            TimeManager.StartTask(PauseMovementForDamage(entity, entity.Get<AnimatorData>(), _damageClip));
        }

        private IEnumerator PauseMovementForDamage(Entity owner, AnimatorData animator, string clip) {
            owner.Tags.Add(EntityTags.CantMove);
            yield return null;
            while (!animator.Animator.IsAnimationComplete(clip)) {
                yield return null;
            }
            owner.Tags.Remove(EntityTags.CantMove);
        }
    }
}
