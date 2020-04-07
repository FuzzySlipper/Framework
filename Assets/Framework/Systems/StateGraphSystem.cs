using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class StateGraphSystem : SystemBase, IMainSystemUpdate {

        private ComponentArray<AnimationGraphComponent> _animationGraphComponents;
        private ComponentArray<AnimationGraphComponent>.RefDelegate _animationDel;

        public StateGraphSystem() {
            _animationGraphComponents = EntityController.GetComponentArray<AnimationGraphComponent>();
            _animationDel = UpdateGraphComponent;
        }
        public void OnSystemUpdate(float dt, float unscaledDt) {
            _animationGraphComponents.Run(_animationDel);
        }

        private void UpdateGraphComponent(ref AnimationGraphComponent animGraph) {
            if (animGraph.Temporary != null) {
                animGraph.Temporary.Update(TimeManager.DeltaTime);
                if (animGraph.Temporary.IsActive) {
                    return;
                }
                animGraph.Temporary.Stop();
                animGraph.Temporary = null;
            }
            if (animGraph.Value != null && animGraph.Value.IsActive) {
                animGraph.Value.Update(TimeManager.DeltaTime);
            }
        }
    }
}
