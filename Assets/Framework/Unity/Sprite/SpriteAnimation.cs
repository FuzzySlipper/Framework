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

        public virtual Sprite GetSprite(int frame) {
            return Sprites[Mathf.Clamp(frame, 0, Sprites.Length - 1)];
        }

        public virtual SavedSpriteCollider GetSpriteCollider(int frame) {
            if (Colliders == null || Colliders.Length == 0) {
                return null;
            }
            return Colliders[Mathf.Clamp(frame, 0, Colliders.Length - 1)];
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

        public float GetFrameStart(int frameIdx) {
            float time = 0;
            for (int i = 0; i < frameIdx; i++) {
                time += FrameTime * _frames[i].Length;
            }
            return time;
        }

        public Vector3 GetEventPosition(SpriteRenderer renderer, AnimationFrame frame) {
            var pixelsPerUnit = renderer.sprite.pixelsPerUnit;
            var width = (renderer.sprite.rect.width / 2) / pixelsPerUnit;
            var height = (renderer.sprite.rect.height / 2) / pixelsPerUnit;
            return renderer.transform.TransformPoint(new Vector3(frame.EventPosition.x * width, height + (frame.EventPosition.y * height), 0));
        }

        public Vector3 GetEventPosition(Sprite sprite, Transform tr, AnimationFrame frame) {
            var pixelsPerUnit = sprite.pixelsPerUnit;
            var width = (sprite.rect.width / 2) / pixelsPerUnit;
            var height = (sprite.rect.height / 2) / pixelsPerUnit;
            return tr.TransformPoint(
                new Vector3(frame.EventPosition.x * width, height + (frame.EventPosition.y * height), 0));
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
        public Rect CriticalRect;
    }
}
