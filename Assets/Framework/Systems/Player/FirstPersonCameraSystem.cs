using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    [AutoRegister, Priority(Priority.Highest)]
    public sealed class FirstPersonCameraSystem : SystemBase, IMainLateUpdate {
        
        public FirstPersonCameraSystem(){}
        
        public void OnSystemLateUpdate(float dt, float unscaledDt) {
            
        }
    }

    [System.Serializable]
    public class SpringEventConfig {
        [ValueDropdown("SignalsList")] public string AnimationEvent;
        public Vector3 Force;
        public int Frames;
        public SpringType Type;

        public SpringEventConfig() {
        }

        public SpringEventConfig(string animationEvent, Vector3 force, int frames, SpringType type) {
            AnimationEvent = animationEvent;
            Force = force;
            Frames = frames;
            Type = type;
        }

        public enum SpringType {
            Position,
            Rotation,
            Fov
        }

        private ValueDropdownList<string> SignalsList() {
            return AnimationEvents.GetDropdownList();
        }

        public void Trigger() {
            switch (Type) {
                case SpringType.Fov:
                    ZoomForce(Force.AbsMax(), Frames);
                    break;
                case SpringType.Rotation:
                    AddRotationForce(Force, Frames);
                    break;
                case SpringType.Position:
                    AddForce(Force, Frames);
                    break;
            }
        }
    }
}
