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
        public abstract SavedSpriteCollider GetSpriteCollider(int frame);
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

        public Vector3 GetEventPosition(SpriteRenderer renderer, AnimationFrame frame) {
            var pixelsPerUnit = renderer.sprite.pixelsPerUnit;
            var width = (renderer.sprite.rect.width / 2) / pixelsPerUnit;
            var height = (renderer.sprite.rect.height / 2) / pixelsPerUnit;
            //return renderer.transform.position + new Vector3(frame.EventPosition.x * width, height + (frame.EventPosition.y * height), 0);
            return renderer.transform.TransformPoint(new Vector3(frame.EventPosition.x * width, height + (frame.EventPosition.y * height), 0));
            //return renderer.bounds.center + new Vector3(frame.EventPosition.x * -renderer.bounds.extents.x, frame.EventPosition.y * -renderer.bounds.extents.y);
        }

        public int ConvertAnimationTimeToFrame(float time) {
            float timeCheck = 0;
            int lastGood = 0;
            for (int i = 0; i < Frames.Length; i++) {
                var frameTime = FrameTime * Frames[i].Length;
                timeCheck += frameTime;
                lastGood = i;
                if (timeCheck >= time) {
                    return i;
                }
            }
            return lastGood;
        }
    }


    [System.Serializable]
    public class AnimationFrame {
        public float Length = 1;
        public int SpriteIndex;
        
        public EventType Event = EventType.None;
        public string EventName;
        public Vector2 EventPosition;
        public float EventDataFloat;
        public string EventDataString;
        public UnityEngine.Object EventDataObject;

        public bool HasEvent { get { return Event != EventType.None; } }

        public enum EventType {
            None = 0,
            Default = 1,
            Message,
        }
    }

    [System.Serializable]
    public class SavedSpriteCollider {
        public List<Vector3> CollisionVertices = new List<Vector3>();
        public List<int> CollisionIndices = new List<int>();
        public Vector3 HighPoint;
    }
}
