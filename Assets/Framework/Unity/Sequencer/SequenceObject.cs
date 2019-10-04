using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class SequenceObject : ScriptableObject {

        public float StartTime = 0;
        public int EditingTrack;
        
        public float EndTime { get => StartTime + Duration; }

        public abstract float Duration { get; set; }
        public abstract bool CanResize { get; }
        public abstract void DrawTimelineGui(Rect rect);
        public abstract void DrawEditorGui();
        public abstract IRuntimeSequenceObject GetRuntime(IRuntimeSequence owner);
        public abstract void DisposeRuntime(IRuntimeSequenceObject runtime);
    }
}
