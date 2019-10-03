using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class AnimationObject : ScriptableObject {

        public float StartTime = 0;
        public float Duration = 0.1f;
        public int EditingTrack;

        protected GUIStyle GuiStyle = null;
        public float EndTime { get => StartTime + Duration; }

        public virtual void DrawTimelineGui(Rect rect) {
            if (GuiStyle == null) {
                GuiStyle = new GUIStyle(GUI.skin.box);
            }
        }
        public abstract void DrawEditorGui();
        public abstract IRuntimeAnimationObject GetRuntime(IRuntimeAnimationHolder owner);
        public abstract void DisposeRuntime(IRuntimeAnimationObject runtime);
    }

    

    public interface IRuntimeAnimationObject {
        float StartTime { get; }
        float EndTime { get; }
        void OnEnter();
        void OnUpdate(float dt);
        void OnExit();
    }
}
