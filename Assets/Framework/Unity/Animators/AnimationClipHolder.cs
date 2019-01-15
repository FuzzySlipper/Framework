using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {

    [System.Serializable]
    public class AnimationClipHolder {
        [ValueDropdown("SignalsList")]
        public string Id;
        public AnimationClip[] Clips = new AnimationClip[0];
        public NormalizedFloatRange PlaySpeed = new NormalizedFloatRange(1,1);

        [HideInInspector] public bool EventTriggered;
        [HideInInspector] public bool Complete;
        [HideInInspector] public int CurrentIndex = 0;

        private ValueDropdownList<string> SignalsList() {
            return AnimationIds.GetDropdownList();
        }

        public void SetRandomIndex() {
            CurrentIndex = Clips.RandomIndex();
            Start = TimeManager.TimeUnscaled;
        }

        public AnimationClip CurrentClip { get { return Clips[CurrentIndex]; } }
        public float Start { get; private set; }
        public float Length { get { return CurrentClip != null ? CurrentClip.length : 1; } }
        public float Remaining { get { return (Start + Length) - TimeManager.TimeUnscaled; } }
    }
}
