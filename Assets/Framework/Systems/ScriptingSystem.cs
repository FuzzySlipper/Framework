using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public class ScriptingSystem : SystemBase {

        public static IActionEventHandler ParseMessage(string[] splitWords) {
            var param0 = splitWords.Length < 2 ? "" : splitWords[1];
            var param1 = splitWords.Length < 3 ? "" : splitWords[2];
            switch (splitWords[0].ToLower()) {
                case "fov":
                case "zoom":
                    return new CameraFovShakeEvent( ParseUtilities.TryParse(param0, 5f), ParseUtilities.TryParse(param1, 4));
                case "pitch":
                case "kick":
                    return new CameraShakeEvent(ParseUtilities.TryParse(param0, Vector3.up), ParseUtilities.TryParse(param1, 4), false);
                case "shake":
                case "rotate":
                    return new CameraShakeEvent(ParseUtilities.TryParse(param0, Vector3.up), ParseUtilities.TryParse(param1, 4), true);
            }
            return null;
        }
    }

    public interface IActionEventHandler {
        void Trigger(ActionEvent ae, string eventName);
    }


    [System.Serializable]
    public class WaitEvent : IActionEventHandler, ISerializable {
        
        private Timer _timer;

        public WaitEvent(float waitTime, bool unscaled) {
            _timer = new Timer(waitTime, unscaled);
        }

        public void Trigger(ActionEvent ae, string eventName) {
            _timer.Restart();
        }

        public WaitEvent(SerializationInfo info, StreamingContext context) {
            _timer = info.GetValue(nameof(_timer), _timer);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_timer), _timer);
        }
    }

    [System.Serializable]
    public class CameraShakeEvent : IActionEventHandler, ISerializable {
        public Vector3 Force { get; }
        public int Frames { get; }
        public bool IsRotation { get; }

        public CameraShakeEvent(Vector3 force, int frames, bool isRotation) {
            Force = force;
            Frames = frames;
            IsRotation = isRotation;
        }

        public void Trigger(ActionEvent stateEvent, string eventName) {
            if (!stateEvent.Origin.IsPlayer()) {
                return;
            }
            if (IsRotation) {
                World.Get<EntityEventSystem>().Post(new CameraRotationForceEvent(Force, Frames));
            }
            else {
                World.Get<EntityEventSystem>().Post(new CameraPositionForceEvent(Force, Frames));
            }
        }

        public CameraShakeEvent(SerializationInfo info, StreamingContext context) {
            Force = info.GetValue(nameof(Force), Force);
            Frames = info.GetValue(nameof(Frames), Frames);
            IsRotation = info.GetValue(nameof(IsRotation), IsRotation);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Force), Force);
            info.AddValue(nameof(Frames), Frames);
            info.AddValue(nameof(IsRotation), IsRotation);
        }
    }

    [System.Serializable]
    public class CameraStateShakeEvent : IActionEventHandler, ISerializable {
        public int Frames { get; }
        public bool IsRotation { get; }

        public CameraStateShakeEvent(int frames, bool isRotation) {
            Frames = frames;
            IsRotation = isRotation;
        }

        public void Trigger(ActionEvent stateEvent, string eventName) {
            if (!stateEvent.Origin.IsPlayer()) {
                return;
            }
            if (IsRotation) {
                World.Get<EntityEventSystem>().Post(new CameraRotationForceEvent(stateEvent.Rotation.eulerAngles, Frames));
            }
            else {
                World.Get<EntityEventSystem>().Post(new CameraPositionForceEvent(stateEvent.Rotation.eulerAngles, Frames));
            }
        }

        public CameraStateShakeEvent(SerializationInfo info, StreamingContext context) {
            Frames = info.GetValue(nameof(Frames), Frames);
            IsRotation = info.GetValue(nameof(IsRotation), IsRotation);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Frames), Frames);
            info.AddValue(nameof(IsRotation), IsRotation);
        }
    }

    [System.Serializable]
    public struct CameraFovShakeEvent : IActionEventHandler, ISerializable {
        public float Force { get; }
        public int Frames { get; }

        public CameraFovShakeEvent(float force, int frames) {
            Force = force;
            Frames = frames;
        }

        public void Trigger(ActionEvent stateEvent, string eventName) {
            if (!stateEvent.Origin.Tags.Contain(EntityTags.Player)) {
                return;
            }
            World.Get<EntityEventSystem>().Post(new CameraZoomForceEvent(Force, Frames));
        }

        public CameraFovShakeEvent(SerializationInfo info, StreamingContext context) {
            Force = (float) info.GetValue(nameof(Force), typeof(float));
            Frames = (int) info.GetValue(nameof(Frames), typeof(int));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Force), Force);
            info.AddValue(nameof(Frames), Frames);
        }
    }
    
}
