using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PixelComrades {

    [System.Serializable]
    public class WeaponPositionMarker : Marker, INotification, INotificationOptionProvider {

        public Vector3 TargetPosition;
        public Quaternion TargetRotation;
        public bool IsPrimary;

        [Button]
        public void CopyMainPosition() {
            if (PoseAnimator.Main == null) {
                return;
            }
            var tr = IsPrimary ? PoseAnimator.Main.PrimaryPivot : PoseAnimator.Main.SecondaryPivot;
            TargetPosition = tr.localPosition;
            TargetRotation = tr.localRotation;
        }

        public PropertyName id { get { return new PropertyName("WeaponPositionMarker"); } }
        public NotificationFlags flags { get { return NotificationFlags.TriggerInEditMode; } }
    }
}
