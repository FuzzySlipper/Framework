using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SequenceAnimationEvent : SequenceEvent {
        public override void DrawEditorGui() {
#if UNITY_EDITOR
            var animationLabels = AnimationEvents.GetNames().ToArray();
            var index = System.Array.IndexOf(animationLabels, EventName);
            var newIndex = UnityEditor.EditorGUILayout.Popup("Event", index, animationLabels);
            if (newIndex >= 0) {
                EventName = animationLabels[newIndex];
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        public override void TriggerEvent(RuntimeSequenceEvent sequenceEvent) {
            sequenceEvent.Owner.Entity.Post(new AnimationEventTriggered(EventName));
        }
    }
}
