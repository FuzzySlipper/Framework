using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Normal)]
    public class SpriteAnimationSystem : SystemBase, IMainSystemUpdate {

        private ComponentArray<SpriteAnimationComponent> _arraySpriteAnimation;

        public SpriteAnimationSystem() {
            _arraySpriteAnimation = EntityController.GetComponentArray<SpriteAnimationComponent>();
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            if (_arraySpriteAnimation.UsedCount > 0) {
                var time = TimeManager.Time;
                var timeUnscaled = TimeManager.TimeUnscaled;
                foreach (SpriteAnimationComponent value in _arraySpriteAnimation) {
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
            }
        }
    }
}
