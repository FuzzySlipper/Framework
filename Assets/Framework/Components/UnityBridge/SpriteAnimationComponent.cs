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
        public SpriteAnimation Animation { get; }
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
            Animation = ItemPool.LoadAsset<SpriteAnimation>(info.GetValue(nameof(Animation), ""));
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
            Renderer.sprite = Animation.GetSpriteFrame(CurrentFrameIndex);
        }

        public AnimationFrame CurrentFrame { get { return Animation.GetFrame(CurrentFrameIndex); } }
        public bool Active { get { return CurrentFrameIndex >= 0; } }
    }

    [System.Serializable]
	public sealed class DirectionalSpriteAnimationComponent : IComponent, IAnimator, IDisposable {
        
        
        public int CurrentFrameIndex = 0;
        public bool Playing = false;
        public Timer FrameTimer;
        public AnimationFrame CurrentFrame;
        public DirectionalAnimationClipHolder CurrentClipHolder;
        public string CurrentClipID;
        public Queue<DirectionalAnimationClipHolder> AnimationClipQueue = new Queue<DirectionalAnimationClipHolder>();
        
        public List<AnimationRequest> Requests = new List<AnimationRequest>();
        public DirectionsEight Orientation = DirectionsEight.Top;
        public float LastAngleHeight;

        public BillboardMode Billboard { get; }
        public SpriteFacing Facing { get; }
        private bool _unscaled;
        private List<KeyValuePair<string, string>> _animDict = new List<KeyValuePair<string, string>>();
        private Dictionary<string, DirectionalAnimationClipHolder> _animDictionary =
            new Dictionary<string, DirectionalAnimationClipHolder>();
        public bool IsSimpleClip { get { return CurrentClipID == AnimationIds.Move || CurrentClipID == AnimationIds.Idle; } }
        public bool AnimationPlaying { get { return CurrentClipHolder != null && !CurrentClipHolder.Complete; } }
        public bool Unscaled { get { return _unscaled; } }
        public string CurrentAnimation { get { return CurrentClipID; } }
        public float CurrentAnimationLength { get { return CurrentClipHolder?.Length ?? 1f; } }
        public float CurrentAnimationRemaining { get { return CurrentClipHolder?.Remaining ?? 0f; } }
        public Vector3 GetEventPosition { get; set; }
        public Quaternion GetEventRotation { get; set; }
        public string CurrentAnimationEvent { get; set; }
        public bool Backwards { get; }
        public DirectionalSpriteAnimationComponent(SpriteHolder animator, Dictionary<string, List<DirectionalAnimation>> dict,
            List<KeyValuePair<string, string>> animDict) {
            _unscaled = animator.Unscaled;
            Billboard = animator.Billboard;
            Facing = animator.Facing;
            Backwards = animator.Backwards;
            _animDict = animDict;
            Setup(dict);
        }
        
        public DirectionalSpriteAnimationComponent(SerializationInfo info, StreamingContext context) {
            _unscaled = info.GetValue(nameof(_unscaled), _unscaled);
            Billboard = info.GetValue(nameof(Billboard), Billboard);
            Facing = info.GetValue(nameof(Facing), Facing);
            Backwards = info.GetValue(nameof(Backwards), Backwards);
            CurrentClipID = info.GetValue(nameof(CurrentClipID), CurrentClipID);
            _animDict = info.GetValue(nameof(_animDict), _animDict);

            var currentSet = new Dictionary<string, List<DirectionalAnimation>>();
            for (int i = 0; i < _animDict.Count; i++) {
                var line = _animDict[i];
                if (!currentSet.TryGetValue(line.Key, out var setList)) {
                    setList = new List<DirectionalAnimation>();
                    currentSet.Add(line.Key, setList);
                }
                var animation = ItemPool.LoadAsset<DirectionalAnimation>(UnityDirs.CharacterAnimations, line.Value);
                if (animation != null) {
                    setList.Add(animation);
                }
            }
            Setup(currentSet);
            if (!string.IsNullOrEmpty(CurrentClipID)) {
                PlayAnimation(CurrentClipID, false, null);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_unscaled), _unscaled);
            info.AddValue(nameof(Billboard), Billboard);
            info.AddValue(nameof(Facing), Facing);
            info.AddValue(nameof(Backwards), Backwards);
            info.AddValue(nameof(CurrentClipID), Playing ? CurrentClipID : "");
            info.AddValue(nameof(_animDict), _animDict);
        }

        private void Setup(Dictionary<string, List<DirectionalAnimation>> dict) {
            FrameTimer = new Timer(0, _unscaled);
            foreach (var animList in dict) {
                if (!_animDictionary.TryGetValue(animList.Key, out var holder)) {
                    _animDictionary.Add(animList.Key, new DirectionalAnimationClipHolder(animList.Key, animList.Value.ToArray()));
                    continue;
                }
                var list = holder.Clips.ToList();
                list.AddRange(animList.Value);
                holder.Clips = list.ToArray();
            }
        }

        public void Dispose() {
            CurrentFrame = null;
            CurrentClipHolder = null;
            FrameTimer = null;
            _animDictionary.Clear();
            AnimationClipQueue.Clear();
        }

        public bool IsAnimationComplete(string clip) {
            return !_animDictionary.TryGetValue(clip, out var clipHolder) || clipHolder.Complete;
        }

        public bool IsAnimationEventComplete(string clip) {
            return !_animDictionary.TryGetValue(clip, out var clipHolder) || clipHolder.EventTriggered;
        }

        public void PlayAnimation(string clip, bool overrideClip, Action action) {
            Requests.Add(new AnimationRequest(clip, overrideClip));
        }

        public DirectionalAnimationClipHolder GetClipHolder(string clip) {
            return _animDictionary.TryGetValue(clip, out var clipHolder) ? clipHolder : null;
        }

        public struct AnimationRequest {
            public string Clip { get; }
            public bool OverrideClip { get; }

            public AnimationRequest(string clip, bool overrideClip) {
                Clip = clip;
                OverrideClip = overrideClip;
            }
        }
    }
}
