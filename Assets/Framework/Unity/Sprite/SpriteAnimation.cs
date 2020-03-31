using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace PixelComrades {
    public abstract class SpriteAnimation : ScriptableObject {
        [SerializeField, FormerlySerializedAs("FramesPerSecond")]
        private float _framesPerSecond = 12;
        [SerializeField, FormerlySerializedAs("Looping")]
        private bool _looping = false;
        [SerializeField, FormerlySerializedAs("NormalMap")]
        private Texture2D _normalMap;
        [SerializeField, FormerlySerializedAs("EmissiveMap")]
        private Texture2D _emissiveMap;
        [SerializeField, FormerlySerializedAs("Frames")] 
        private AnimationFrame[] _frames = new AnimationFrame[0];
        public string LastModified;
        public float FrameTime { get { return 1 / _framesPerSecond; } }
        public virtual int LengthFrames { get { return _frames.Length; } }
        public virtual AnimationFrame[] Frames { get => _frames; set => _frames = value; }
        public float FramesPerSecond { get => _framesPerSecond; set => _framesPerSecond = value; }
        public bool Looping { get => _looping; set => _looping = value; }
        public Texture2D NormalMap { get => _normalMap; set => _normalMap = value; }
        public Texture2D EmissiveMap { get => _emissiveMap; set => _emissiveMap = value; }
        public abstract int LengthSprites { get; }
        public abstract Sprite[] Sprites { get; set; }
        public abstract SavedSpriteCollider[] Colliders { get; set; }

        public int NumberOfEvents {
            get {
                int cnt = 0;
                for (int i = 0; i < Frames.Length; i++) {
                    if (Frames[i].Event != AnimationEvent.Type.None) {
                        cnt++;
                    }
                }
                return cnt;
            }
        }

        public virtual Sprite GetSprite(int spriteIdx) {
            return Sprites[Mathf.Clamp(spriteIdx, 0, Sprites.Length - 1)];
        }

        public virtual SavedSpriteCollider GetSpriteCollider(int spriteIdx) {
            if (Colliders == null || Colliders.Length == 0) {
                return null;
            }
            return Colliders[Mathf.Clamp(spriteIdx, 0, Colliders.Length - 1)];
        }

        public float LengthTime {
            get {
                float time = 0;
                for (int i = 0; i < _frames.Length; i++) {
                    time += FrameTime * _frames[i].Length;
                }
                return time;
            }
        }

        public AnimationFrame GetFrameClamped(int frame) {
            return _frames[Mathf.Clamp(frame, 0, _frames.Length - 1)];
        }

        public bool IsComplete(int index) {
            return index > _frames.Length - 1;
        }

        public AnimationFrame GetFrame(int index) {
            if (index < _frames.Length) {
                return _frames[index];
            }
            return null;
        }

        public float GetFrameStartTime(int frameIdx) {
            float time = 0;
            for (int i = 0; i < frameIdx; i++) {
                time += FrameTime * _frames[i].Length;
            }
            return time;
        }

        public Vector3 GetEventPosition(SpriteRenderer renderer, AnimationFrame frame) {
            return GetEventPosition(renderer.sprite, renderer.transform, frame);
        }

        public Vector3 GetEventPosition(Sprite sprite, Transform tr, AnimationFrame frame) {
            var size = new Vector2(sprite.rect.width / sprite.pixelsPerUnit,
                sprite.rect.height / sprite.pixelsPerUnit);
            return tr.TransformPoint(Mathf.Lerp(-(size.x * 0.5f), (size.x * 0.5f), frame.EventPosition.x), size.y * frame.EventPosition.y, 0);
        }

        public AnimationFrame GetFrameAtTime(float time) {
            return Frames[Mathf.Clamp(ConvertAnimationTimeToFrame(time), 0, Frames.Length - 1)];
        }

        public int ConvertAnimationTimeToFrame(float time) {
            float timeCheck = 0;
            int lastGood = 0;
            for (int i = 0; i < _frames.Length; i++) {
                var frameTime = FrameTime * _frames[i].Length;
                timeCheck += frameTime;
                lastGood = i;
                if (timeCheck >= time) {
                    return i;
                }
            }
            return lastGood;
        }
    }

    public struct AnimationEvent {
        public readonly Type EventType;
        public readonly Vector3 EventPosition;
        public readonly string EventDataString;
        public readonly UnityEngine.Object EventDataObject;

        public enum Type {
            None = 0,
            Default = 1,
            Message,
            Camera,
            Fx,
            StateEnter,
            StateExit,
        }

        public AnimationEvent(Type type, string eventName) : this() {
            EventType = type;
            EventDataString = eventName;
            EventPosition = Vector3.zero;
            EventDataString = null;
            EventDataObject = null;
        }

        public AnimationEvent(AnimationFrame frame, Vector3 pos) {
            EventType = frame.Event;
            EventPosition = pos;
            EventDataString = frame.EventDataString;
            EventDataObject = frame.EventDataObject;
        }
    }


    [System.Serializable]
    public class AnimationFrame {
        public float Length = 1;

        public AnimationEvent.Type Event = AnimationEvent.Type.None;
        public Vector2 EventPosition;
        public string EventDataString;
        public UnityEngine.Object EventDataObject;

        public bool HasEvent { get { return Event != AnimationEvent.Type.None; } }

        
    }

    [System.Serializable]
    public class SavedSpriteCollider {
        public List<Vector3> CollisionVertices = new List<Vector3>();
        public List<int> CollisionIndices = new List<int>();
        public Rect CriticalRect;
    }
}
