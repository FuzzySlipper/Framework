using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PixelComrades {
    [System.Serializable]
    public class HandPoseMarker : Marker, INotification, INotificationOptionProvider {


        public NotificationFlags Flags;
        public HandPose Pose;

        public PropertyName id { get { return new PropertyName("HandPose"); } }
        public NotificationFlags flags { get { return Flags; } }
    }
}
