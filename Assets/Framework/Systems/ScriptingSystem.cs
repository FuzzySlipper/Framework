using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ScriptingSystem : SystemBase {

        public static IActionScriptedEvent ParseMessage(ActionStateEvents stateEvent, string[] splitWords) {
            var param0 = splitWords.Length < 2 ? "" : splitWords[1];
            var param1 = splitWords.Length < 3 ? "" : splitWords[2];
            switch (splitWords[0].ToLower()) {
                case "fov":
                case "zoom":
                    return new CameraFovShakeEvent( stateEvent, ParseUtilities.TryParse(param0, 5f), ParseUtilities.TryParse(param1, 4));
                case "pitch":
                case "kick":
                    return new CameraShakeEvent(stateEvent, ParseUtilities.TryParse(param0, Vector3.up), ParseUtilities.TryParse(param1, 4), false);
                case "shake":
                case "rotate":
                    return new CameraShakeEvent(stateEvent, ParseUtilities.TryParse(param0, Vector3.up), ParseUtilities.TryParse(param1, 4), true);
            }
            return null;
        }
    }

    public interface IActionScriptedEvent {
        ActionStateEvents Event { get; }
        void Trigger(ActionStateEvent stateEvent);
    }

    public class ActionScriptedSequence : IActionScriptedEvent {
        public ActionStateEvents Event { get; }
        public IActionScriptedEvent[] ScriptedEvents { get; }

        public ActionScriptedSequence(ActionStateEvents @event, IActionScriptedEvent[] scriptedEvents) {
            Event = @event;
            ScriptedEvents = scriptedEvents;
        }

        public void Trigger(ActionStateEvent stateEvent) {
            TimeManager.StartUnscaled(AdvanceSequence(stateEvent));
        }

        private IEnumerator AdvanceSequence(ActionStateEvent stateEvent) {
            for (int i = 0; i < ScriptedEvents.Length; i++) {
                ScriptedEvents[i].Trigger(stateEvent);
                if (ScriptedEvents[i] is WaitEvent wait) {
                    while (!wait.CanAdvance) {
                        yield return null;
                    }
                }
            }
        }
    }

    public class WaitEvent : IActionScriptedEvent {
        public ActionStateEvents Event { get; }
        public bool CanAdvance { get { return _timer.IsActive; } }
        
        private Timer _timer;

        public WaitEvent(ActionStateEvents @event, float waitTime, bool unscaled) {
            Event = @event;
            _timer = new Timer(waitTime, unscaled);
        }

        public void Trigger(ActionStateEvent stateEvent) {
            _timer.Restart();
        }

    }

    public class CameraShakeEvent : IActionScriptedEvent {
        public ActionStateEvents Event { get; }
        public Vector3 Force { get; }
        public int Frames { get; }
        public bool IsRotation { get; }

        public CameraShakeEvent(ActionStateEvents state, Vector3 force, int frames, bool isRotation) {
            Event = state;
            Force = force;
            Frames = frames;
            IsRotation = isRotation;
        }

        public void Trigger(ActionStateEvent stateEvent) {
            if (!stateEvent.Origin.Tags.Contain(EntityTags.Player)) {
                return;
            }
            FirstPersonCamera.AddForce(Force, IsRotation, Frames);
        }
    }

    public class CameraStateShakeEvent : IActionScriptedEvent {
        public ActionStateEvents Event { get; }
        public int Frames { get; }
        public bool IsRotation { get; }

        public CameraStateShakeEvent(ActionStateEvents state, int frames, bool isRotation) {
            Event = state;
            Frames = frames;
            IsRotation = isRotation;
        }

        public void Trigger(ActionStateEvent stateEvent) {
            if (!stateEvent.Origin.Tags.Contain(EntityTags.Player)) {
                return;
            }
            FirstPersonCamera.AddForce(stateEvent.Rotation.eulerAngles, IsRotation, Frames);
        }
    }

    public struct CameraFovShakeEvent : IActionScriptedEvent {
        public ActionStateEvents Event { get; }
        public float Force { get; }
        public int Frames { get; }

        public CameraFovShakeEvent(ActionStateEvents @event, float force, int frames) {
            Event = @event;
            Force = force;
            Frames = frames;
        }

        public void Trigger(ActionStateEvent stateEvent) {
            if (!stateEvent.Origin.Tags.Contain(EntityTags.Player)) {
                return;
            }
            FirstPersonCamera.ZoomForce(Force, Frames);
        }
    }
    
}
