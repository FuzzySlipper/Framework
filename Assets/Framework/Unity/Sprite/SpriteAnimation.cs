using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class SpriteAnimation : ScriptableObject {
        public float FramesPerSecond = 12;
        public bool Looping = false;
        public Texture2D NormalMap;
        public Texture2D EmissiveMap;
        public AnimationFrame[] Frames = new AnimationFrame[0];
        public string LastModified;
        public float FrameTime { get { return 1 / FramesPerSecond; } }
        public virtual int LengthFrames { get { return Frames.Length; } }
        public abstract Sprite GetSpriteFrame(int frame);
        public float LengthTime {
            get {
                float time = 0;
                for (int i = 0; i < Frames.Length; i++) {
                    time += FrameTime * Frames[i].Length;
                }
                return time;
            }
        }

        public AnimationFrame GetFrameClamped(int frame) {
            return Frames[Mathf.Clamp(frame, 0, Frames.Length - 1)];
        }

        public bool IsComplete(int index) {
            return index > Frames.Length - 1;
        }

        public AnimationFrame GetFrame(int index) {
            if (index < Frames.Length) {
                return Frames[index];
            }
            return null;
        }

        public float GetFrameStart(int frameIdx) {
            float time = 0;
            for (int i = 0; i < frameIdx; i++) {
                time += FrameTime * Frames[i].Length;
            }
            return time;
        }

    }


    [System.Serializable]
    public class AnimationFrame {
        public float Length = 1;
        public int SpriteIndex;
        public bool DefaultEventTrigger = false;

        public EventType Event = EventType.None;
        public string EventName;
        public int EventDataInt;
        public float EventDataFloat;
        public string EventDataString;
        public GameObject EventDataGameObject;

        public enum EventType {
            None,
            Int,
            Float,
            String,
            Gameobject
        }
    }
}
