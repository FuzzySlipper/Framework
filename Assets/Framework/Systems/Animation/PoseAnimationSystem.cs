using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class PoseAnimationSystem : SystemBase, IMainSystemUpdate {

        private ComponentArray<PoseAnimatorComponent> _array;
        private ComponentArray<PoseAnimatorComponent>.RefDelegate _del;
        
        public PoseAnimationSystem() {
            _array = EntityController.GetComponentArray<PoseAnimatorComponent>();
            _del = UpdateList;
        }
        
        public void OnSystemUpdate(float dt, float unscaledDt) {
            _array.Run(_del);
        }

        private void UpdateList(ref PoseAnimatorComponent poseAnimator) {
            poseAnimator.UpdatePose();
        }
    }
}
