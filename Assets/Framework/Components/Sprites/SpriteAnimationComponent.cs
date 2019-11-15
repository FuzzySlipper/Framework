using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using PixelComrades.DungeonCrawler;

namespace PixelComrades {
    [System.Serializable]
	public sealed class SpriteAnimationComponent : IComponent {
        
        private CachedUnityComponent<SpriteRenderer> _component;
        public SpriteRenderer Renderer { get { return _component.Value; } }
        public SpriteAnimation Animation { get; private set; }
        public bool Unscaled { get;}
        public BillboardMode Billboard { get; }

        public float NextUpdateTime;
        public short CurrentFrameIndex;
        public float LastAngleHeight;
        
        public SpriteAnimationComponent(SerializationInfo info, StreamingContext context) {
            _component = info.GetValue(nameof(_component), _component);
            Unscaled = info.GetValue(nameof(Unscaled), Unscaled);
            Billboard = info.GetValue(nameof(Billboard), Billboard);
            NextUpdateTime = info.GetValue(nameof(NextUpdateTime), NextUpdateTime);
            CurrentFrameIndex = info.GetValue(nameof(CurrentFrameIndex), CurrentFrameIndex);
            LastAngleHeight = info.GetValue(nameof(LastAngleHeight), LastAngleHeight);
            ItemPool.LoadAsset<SpriteAnimation>(info.GetValue(nameof(Animation), ""), a => Animation = a);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_component), _component);
            info.AddValue(nameof(Unscaled), Unscaled);
            info.AddValue(nameof(Billboard), Billboard);
            info.AddValue(nameof(NextUpdateTime), NextUpdateTime);
            info.AddValue(nameof(CurrentFrameIndex), CurrentFrameIndex);
            info.AddValue(nameof(LastAngleHeight), LastAngleHeight);
            info.AddValue(nameof(Animation), ItemPool.GetAssetLocation(Animation));
        }

        public SpriteAnimationComponent(SpriteRenderer renderer, SpriteAnimation animation, bool unscaled, BillboardMode billboard) {
            _component = new CachedUnityComponent<SpriteRenderer>(renderer);
            Animation = animation;
            Unscaled = unscaled;
            Billboard = billboard;
            CurrentFrameIndex = 0;
            UpdateFrame(Unscaled ? TimeManager.TimeUnscaled : TimeManager.Time);
        }

        public void UpdateFrame(float comparisonTime) {
            NextUpdateTime = Animation.FrameTime * CurrentFrame.Length + comparisonTime;
            Renderer.sprite = Animation.GetSprite(CurrentFrameIndex);
        }

        public AnimationFrame CurrentFrame { get { return Animation.GetFrame(CurrentFrameIndex); } }
        public bool Active { get { return CurrentFrameIndex >= 0; } }
    }
}
