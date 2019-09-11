using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PlayerClipAnimator : ClipAnimator {
#if Animancer
        [SerializeField] private AvatarMask _mask = null;
#endif
        [SerializeField] private PlayerClipAnimator _otherAnimator = null;
        [SerializeField] private bool _isSecondary = false;

        
        private Vector3 _resetPoint;

        
        private bool IsSecondary { get { return _isSecondary; } }

        public override void OnCreate(PrefabEntity entity) {
            base.OnCreate(entity);
#if Animancer
            if (_mask != null) {
                Controller.SetLayerMask(Layer, _mask);
            }
#endif
            if (!IsSecondary) {
                PlayAnimation(PlayerAnimationIds.Idle, false);
            }
            //else {
            //    Controller.SetLayerAdditive(Layer, true);
            //}
        }

        //protected override bool CheckFinish() {
        //    if (base.CheckFinish()) {
        //        return true;
        //    }
        //    if (_primary != null) {
        //        PlayAnimation(_primary.IdleAnimation, false);
        //        return true;
        //    }
        //    if (IsSecondary) {
        //        PlayAnimation(_otherAnimator.CurrentAnimationClipState);
        //        //StopCurrentAnimation();
        //        //Controller.GetLayer(Layer).StartFade(0, FadeDuration);
        //    }
        //    else {
        //        PlayAnimation(PlayerAnimationIds.Idle, false);
        //    }
        //    return false;
        //}

        protected override void PlayAnimation(AnimationClipState clip) {
            //bool syncClipState = !IsSecondary;// && _otherAnimator.CurrentAnimationClipState == CurrentAnimationClipState;
            base.PlayAnimation(clip);
            if (!IsSecondary) {
                _otherAnimator.PlayAnimation(clip);
            }
            //if (IsSecondary) {
            //    Controller.GetLayer(Layer).SetWeight(1);
            //}
        }

        private RaycastHit[] _hits = new RaycastHit[10];
        private Vector3 GetMouseRaycastPosition() {
            var ray = PlayerInput.GetTargetRay;
            var cnt = Physics.RaycastNonAlloc(ray, _hits, 500, LayerMasks.DefaultCollision);
            _hits.SortByDistanceAsc(cnt);
            for (int i = 0; i < cnt; i++) {
                if (_hits[i].transform.CompareTag(StringConst.TagPlayer)) {
                    continue;
                }
                return _hits[i].point;
            }
            return ray.origin + (ray.direction * 500);
        }
    }
}
