using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public class TagTimerSystem : SystemBase, ISystemUpdate, IReceiveGlobal<ConfusionEvent>, IReceiveGlobal<TagTimerEvent> {

        public TagTimerSystem() {}

        private ManagedArray<TagTimerEvent> _timerArray = new ManagedArray<TagTimerEvent>();

        public bool Unscaled { get { return false; } }
        
        public void OnSystemUpdate(float dt) {
            if (_timerArray.UsedCount == 0) {
                return;
            }
            foreach (TagTimerEvent timer in _timerArray) {
                if (timer.TimeEnd > TimeManager.Time) {
                    continue;
                }
                timer.Entity.Tags.Remove(timer.Tag);
                timer.Entity.Post(new TagChangeEvent(timer.Entity, timer.Tag, false));
                switch (timer.Tag) {
                    case EntityTags.IsConfused:
                        if (EntityCount(timer.Entity, timer.Tag) <= 1) {
                            timer.Entity.Post(new ConfusionEvent(timer.Entity, 0, false));
                        }
                        break;
                }
                _timerArray.Remove(timer);
            }
        }

        private int EntityCount(Entity entity, int tag) {
            int cnt = 0;
            foreach (TagTimerEvent timerEvent in _timerArray) {
                if (timerEvent.Entity == entity && timerEvent.Tag == tag) {
                    cnt++;
                }
            }
            return cnt;
        }

        public void HandleGlobal(TagTimerEvent arg) {
            _timerArray.Add(arg);
        }

        public void HandleGlobal(ConfusionEvent arg) {
            if (!arg.Active) {
                return;
            }
            arg.Entity.Tags.Add(EntityTags.IsConfused);
            if (arg.Length > 0) {
                _timerArray.Add(new TagTimerEvent(arg.Entity, TimeManager.Time + arg.Length, EntityTags.IsConfused));
            }
        }
    }
}
