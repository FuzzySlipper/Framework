using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PixelComrades {
    [System.Serializable]
    public class AnimationEventMarker : Marker, INotification, INotificationOptionProvider {

        [ValueDropdown("SignalsList")]
        public string Event;

        private ValueDropdownList<string> SignalsList() {
            return AnimationEvents.GetDropdownList();
        }
        public PropertyName id { get { return new PropertyName("AnimationEventMarker"); } }
        public NotificationFlags flags { get { return NotificationFlags.TriggerInEditMode; } }
    }
}
