using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class WeaponPose : ScriptableObject {

        [SerializeField] private TransformState _pivot = new TransformState();
        [SerializeField] private HandPose _handPose = null;
        [SerializeField] private bool _isPrimary = true;
        
        private Transform TargetPivot { get { return _isPrimary ? PoseAnimator.Main.PrimaryPivot : PoseAnimator.Main.SecondaryPivot; } }

        [Button]
        public void CopyCurrentPivot() {
            _pivot.Set(TargetPivot);
        }

        [Button]
        public void CopyPositionTracker() {
            if (PoseAnimationHelper.PositionTracker != null) {
                _pivot.SetInverse(TargetPivot, PoseAnimationHelper.PositionTracker);
            }
        }

        [Button]
        public void SetPose() {
            SetPose(PoseAnimator.Main);
        }

        public void SetPose(PoseAnimator animator) {
            _pivot.Restore(_isPrimary ? animator.PrimaryPivot : animator.SecondaryPivot);
            _handPose.SetPose(animator);
        }
    }
}
