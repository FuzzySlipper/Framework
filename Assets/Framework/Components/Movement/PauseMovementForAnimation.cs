using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PauseMovementForAnimation : IComponent, IReceive<DamageEvent> {
        public int Owner { get; set; }

        private string _damageClip;

        public PauseMovementForAnimation(string damageClip) {
            _damageClip = damageClip;
        }

        public void PauseForClip(string clip) {
            var entity = this.GetEntity();
            TimeManager.Start(PauseMovementForDamage(entity, entity.Get<AnimatorData>(), clip));
        }

        public void Handle(DamageEvent arg) {
            var entity = this.GetEntity();
            TimeManager.Start(PauseMovementForDamage(entity, entity.Get<AnimatorData>(), _damageClip));
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
