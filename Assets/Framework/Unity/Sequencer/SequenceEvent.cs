using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SequenceEvent : SequenceObject {

        public string EventName;
        private const float EventDuration = 0.05f;
        
        private GenericPool<RuntimeSequenceEvent> _pool = new GenericPool<RuntimeSequenceEvent>(1, t => t.Clear());
        public override float Duration { 
            get { return EventDuration; }
            set {}
        }
        public override bool CanResize { get { return false; } }
        

        public override void DrawTimelineGui(Rect rect) {
#if UNITY_EDITOR
            if (EventName != null && name != EventName) {
                name = EventName;
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
            var guiStyle = new GUIStyle(GUI.skin.box);
            guiStyle.normal.background = TextureUtilities.MakeTexture((int) rect.width, (int) rect.height, new Color(0.54f, 1f, 0.58f),
            new RectOffset(5,5,5,5), new Color(0f, 0.35f, 0.1f));
            GUI.Box(rect, string.Format("{0:F3}:{1}", StartTime, EventName), guiStyle);
        }

        public override void DrawEditorGui() {
#if UNITY_EDITOR
            EventName = UnityEditor.EditorGUILayout.TextField("Event", EventName);
#endif
        }

        public override IRuntimeSequenceObject GetRuntime(IRuntimeSequence owner) {
            var obj = _pool.New();
            obj.Set(this, owner);
            return obj;
        }

        public override void DisposeRuntime(IRuntimeSequenceObject runtime) {
            if (runtime is RuntimeSequenceEvent pose) {
                _pool.Store(pose);
            }
        }

        public virtual void TriggerEvent(RuntimeSequenceEvent sequenceEvent) {
            Debug.Log("Triggered " + EventName + " at " + sequenceEvent.Owner.CurrentTime);
        }
    }
    
    public class RuntimeSequenceEvent : IRuntimeSequenceObject {

        private SequenceEvent _original;
        private bool _triggered = false;
        public IRuntimeSequence Owner { get; private set; }
        public float StartTime { get => _original.StartTime; }
        public float EndTime { get => _original.EndTime; }

        public void Clear() {
            Owner = null;
            _original = null;
        }
        
        public void Set(SequenceEvent original, IRuntimeSequence owner) {
            _original = original;
            Owner = owner;
            _triggered = false;
        }
        
        public void OnEnter() {
            TriggerEvent();
        }

        public void OnUpdate(float dt) {
            if (!_triggered) {
                TriggerEvent();
            }
        }

        public void OnExit() {
            _triggered = false;
        }

        private void TriggerEvent() {
            _triggered = true;
            _original.TriggerEvent(this);
        }
    }
}
