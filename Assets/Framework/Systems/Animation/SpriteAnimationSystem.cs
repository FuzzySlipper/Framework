using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Normal)]
    public class SpriteAnimationSystem : SystemBase, IMainSystemUpdate {

        private ComponentArray<SpriteAnimationComponent> _arraySpriteAnimation;
        private ComponentArray<DirectionalSpriteAnimationComponent> _arrayDirectional;

        public SpriteAnimationSystem() {
            _arraySpriteAnimation = EntityController.GetComponentArray<SpriteAnimationComponent>();
            _arrayDirectional = EntityController.GetComponentArray<DirectionalSpriteAnimationComponent>();
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            var time = TimeManager.Time;
            var timeUnscaled = TimeManager.TimeUnscaled;
            for (int i = 0; i < _arraySpriteAnimation.Count; i++) {
                if (_arraySpriteAnimation.IsInvalid(i)) {
                    continue;
                }
                ref SpriteAnimationComponent value = ref _arraySpriteAnimation[i];
                if (value.Renderer == null || !value.Active) {
                    continue;
                }
                value.Billboard.Apply(value.Renderer.transform, true, ref value.LastAngleHeight);
                var comparisonTime = value.Unscaled ? timeUnscaled : time;
                if (value.NextUpdateTime > comparisonTime) {
                    continue;
                }
                value.CurrentFrameIndex += 1;
                //TODO: post events to entities when events/finished
                if (value.CurrentFrameIndex >= value.Animation.Frames.Length) {
                    if (!value.Animation.Looping) {
                        value.CurrentFrameIndex = -1;
                        continue;
                    }
                    value.CurrentFrameIndex = 0;
                }
                value.UpdateFrame(comparisonTime);
            }
            for (int i = 0; i < _arrayDirectional.Count; i++) {
                if (_arrayDirectional.IsInvalid(i)) {
                    continue;
                }
                _arrayDirectional[i].Update(_arrayDirectional[i].Unscaled ? timeUnscaled : time);
            }
        }
    }
}
