using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using PixelComrades.DungeonCrawler;

namespace PixelComrades {
    public class SpriteAnimationComponent : IComponent {
        
        private CachedUnityComponent<SpriteRenderer> _component;
        public SpriteRenderer Renderer { get { return _component.Component; } }
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

    public class DirectionalSpriteAnimationComponent : IComponent, IModelComponent, IAnimator, IDisposable {
        private BillboardMode _billboard;
        private bool _unscaled;
        private CachedUnityComponent<SpriteRenderer> _renderer;
        private CachedUnityComponent<SpriteCollider> _spriteCollider;
        private CachedTransform _spriteBaseTr;
        private CachedTransform _spriteTr;
        private SpriteFacing _facing;
        private bool _backwards;

        private int _currentFrameIndex = 0;
        private bool _playing = false;
        private Timer _frameTimer;
        private AnimationFrame _currentFrame;
        private float _lastAngleHeight;

        private List<KeyValuePair<string, string>> _animDict = new List<KeyValuePair<string, string>>();
        private Dictionary<string, DirectionalAnimationClipHolder> _animDictionary = new Dictionary<string, DirectionalAnimationClipHolder>();
        private DirectionalAnimationClipHolder _currentAnimation;
        private string _currentClipID;
        private MaterialPropertyBlock[] _blocks = new MaterialPropertyBlock[1];
        private Renderer[] _renderers = new Renderer[1];
        private Queue<DirectionalAnimationClipHolder> _animationClipQueue = new Queue<DirectionalAnimationClipHolder>();
        private DirectionsEight _orientation = DirectionsEight.Top;

        private bool IsSimpleClip { get { return _currentClipID == AnimationIds.Move || _currentClipID == AnimationIds.Idle; } }
        public bool AnimationPlaying { get { return _currentAnimation != null && !_currentAnimation.Complete; } }
        public bool Unscaled { get { return _unscaled; } }
        public Renderer[] Renderers { get { return _renderers; } }
        public string CurrentAnimation { get { return _currentClipID; } }
        public Transform Tr { get { return _spriteBaseTr; } }
        public float CurrentAnimationLength { get { return _currentAnimation?.Length ?? 1f; } }
        public float CurrentAnimationRemaining { get { return _currentAnimation?.Remaining ?? 0f; } }
        public Vector3 GetEventPosition { get; private set; }
        public Quaternion GetEventRotation { get { return _spriteBaseTr.Tr.rotation; } }
        public SpriteCollider SpriteCollider { get => _spriteCollider; }
        public DirectionsEight Orientation { get => _orientation; }
        public string CurrentAnimationEvent { get; private set; }
        public MaterialPropertyBlock[] MaterialBlocks {
            get {
                if (_renderer == null) {
                    return null;
                }
                _renderer.Component.GetPropertyBlock(_blocks[0]);
                return _blocks;
            }
        }

        public DirectionalSpriteAnimationComponent(SpriteHolder animator, Dictionary<string, List<DirectionalAnimation>> dict,
            List<KeyValuePair<string, string>> animDict) {
            _unscaled = animator.Unscaled;
            _billboard = animator.Billboard;
            _renderer = new CachedUnityComponent<SpriteRenderer>(animator.Renderer);
            _spriteBaseTr = new CachedTransform(animator.SpriteBaseTr);
            _spriteTr = new CachedTransform(animator.SpriteTr);
            _facing = animator.Facing;
            _backwards = animator.Backwards;
            _spriteCollider = new CachedUnityComponent<SpriteCollider>(_renderer.Component.GetComponent<SpriteCollider>());
            Setup(dict);
        }
        
        public DirectionalSpriteAnimationComponent(SerializationInfo info, StreamingContext context) {
            _unscaled = info.GetValue(nameof(_unscaled), _unscaled);
            _billboard = info.GetValue(nameof(_billboard), _billboard);
            _renderer = info.GetValue(nameof(_renderer), _renderer);
            _spriteBaseTr = info.GetValue(nameof(_spriteBaseTr), _spriteBaseTr);
            _spriteTr = info.GetValue(nameof(_spriteTr), _spriteTr);
            _facing = info.GetValue(nameof(_facing), _facing);
            _backwards = info.GetValue(nameof(_backwards), _backwards);
            _spriteCollider = info.GetValue(nameof(_spriteCollider), _spriteCollider);
            _currentClipID = info.GetValue(nameof(_currentClipID), _currentClipID);
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
            if (!string.IsNullOrEmpty(_currentClipID)) {
                PlayAnimation(_currentClipID, false, null);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_unscaled), _unscaled);
            info.AddValue(nameof(_billboard), _billboard);
            info.AddValue(nameof(_renderer), _renderer);
            info.AddValue(nameof(_spriteBaseTr), _spriteBaseTr);
            info.AddValue(nameof(_spriteTr), _spriteTr);
            info.AddValue(nameof(_facing), _facing);
            info.AddValue(nameof(_backwards), _backwards);
            info.AddValue(nameof(_spriteCollider), _spriteCollider);
            info.AddValue(nameof(_currentClipID), _playing ? _currentClipID : "");
            info.AddValue(nameof(_animDict), _animDict);
        }

        private void Setup(Dictionary<string, List<DirectionalAnimation>> dict) {
            _frameTimer = new Timer(0, _unscaled);
            _renderers[0] = _renderer;
            _blocks[0] = new MaterialPropertyBlock();
            _renderer.Component.GetPropertyBlock(_blocks[0]);
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
            _spriteBaseTr = _spriteTr = null;
            _renderer = null;
            _renderers = null;
            _spriteCollider = null;
            _blocks = null;
            _currentFrame = null;
            _currentAnimation = null;
            _frameTimer = null;
            _animDictionary.Clear();
            _animationClipQueue.Clear();
        }

        public void Update(float dt) {
            var entity = this.GetEntity();
            if (_currentAnimation == null || IsSimpleClip) {
                CheckMoving(entity);
            }
            _billboard.Apply(_spriteTr, _backwards, ref _lastAngleHeight);
            var orientation = SpriteFacingControl.GetCameraSide(_facing, _spriteTr, _spriteBaseTr, 5, out var inMargin);
            if (_orientation == orientation || (inMargin && (orientation.IsAdjacent(_orientation)))) {
                if (CheckFrameUpdate(entity)) {
                    UpdateSpriteFrame();
                }
                return;
            }
            _orientation = orientation;
            UpdateSpriteFrame();
        }

        private void CheckMoving(Entity entity) {
            if (entity == null || entity.IsDead()) {
                return;
            }
            if (_currentAnimation == null) {
                PlayAnimation(entity.Tags.Contain(EntityTags.Moving) ? AnimationIds.Move : AnimationIds.Idle, false, null);
            }
            else if (_currentClipID == AnimationIds.Move && !entity.Tags.Contain(EntityTags.Moving)) {
                PlayAnimation(AnimationIds.Idle, false, null);
            }
            else if (_currentClipID == AnimationIds.Idle && entity.Tags.Contain(EntityTags.Moving)) {
                PlayAnimation(AnimationIds.Move, false, null);
            }
        }

        private void UpdateSpriteFrame() {
            var facing = _orientation;
            if (_facing.RequiresFlipping()) {
                facing = _orientation.GetFlippedSide();
                _renderer.Component.flipX = _orientation.IsFlipped();
            }
            if (_currentAnimation == null) {
                return;
            }
            var sprite = _currentAnimation.CurrentClip.GetSpriteFrame(facing, _currentFrameIndex);
            if (sprite == null) {
                return;
            }
            _renderer.Component.sprite = sprite;
            if (_spriteCollider != null) {
                _spriteCollider.Component.UpdateCollider();
            }
        }

        public void SetVisible(bool status) {
            if (_renderer == null) {
                return;
            }
            _renderer.Component.enabled = status;
        }

        public bool IsAnimationComplete(string clip) {
            return !_animDictionary.TryGetValue(clip, out var clipHolder) || clipHolder.Complete;
        }

        public bool IsAnimationEventComplete(string clip) {
            return !_animDictionary.TryGetValue(clip, out var clipHolder) || clipHolder.EventTriggered;
        }

        public void PlayAnimation(string clip, bool overrideClip, Action action) {
            if (!_animDictionary.TryGetValue(clip, out var clipHolder)) {
                return;
            }
            if (clipHolder.Clips.Length == 0) {
                return;
            }
            if (AnimationPlaying && !IsSimpleClip) {
                if (_currentAnimation == clipHolder) {
                    return;
                }
                if (!overrideClip) {
                    clipHolder.ResetBools();
                    if (!_animationClipQueue.Contains(clipHolder)) {
                        _animationClipQueue.Enqueue(clipHolder);
                    }
                    return;
                }
            }
            PlayAnimation(clipHolder);
        }

        private void PlayAnimation(DirectionalAnimationClipHolder clipHolder) {
            clipHolder.SetRandomIndex();
            if (clipHolder.CurrentClip == null) {
                return;
            }
            _currentClipID = clipHolder.Id;
            _currentAnimation = clipHolder;
            clipHolder.ResetBools();
            GetEventPosition = _spriteBaseTr.Tr.position;
            _currentFrameIndex = -1;
            _playing = true;
            _frameTimer.Cancel();
            _blocks[0].SetTexture("_BumpMap", clipHolder.CurrentClip.NormalMap);
            _blocks[0].SetTexture("_EmissionMap", clipHolder.CurrentClip.EmissiveMap);
            if (clipHolder.CurrentClip.EmissiveMap != null) {
                _renderer.Component.material.EnableKeyword("_EMISSION");
            }
            else {
                _renderer.Component.material.DisableKeyword("_EMISSION");
            }
            _renderer.Component.SetPropertyBlock(_blocks[0]);
        }

        private bool CheckFrameUpdate(Entity entity) {
            if (!_playing || _frameTimer.IsActive || _currentAnimation == null || entity.Tags.IsStunned) {
                return false;
            }
            _currentFrameIndex++;
            SetFrame();
            if (_currentFrame == null) {
                _currentAnimation.Complete = true;
                if (_currentAnimation.CurrentClip.Looping) {
                    _currentFrameIndex = 0;
                    SetFrame();
                    return true;
                }
                _playing = false;
                if (!ClipFinished(entity)) {
                    return false;
                }
                _currentFrameIndex = 0;
                SetFrame();
                return true;
            }
            if (_currentFrame.HasEvent) {
                if (_currentFrame.Event == AnimationFrame.EventType.Default) {
                    _currentAnimation.EventTriggered = true;
                    CurrentAnimationEvent = AnimationEvents.Default;
                }
                else {
                    CurrentAnimationEvent = _currentFrame.EventName;
                }
                GetEventPosition =_currentAnimation.CurrentClip.GetEventPosition(_renderer, _currentFrame);
                Debug.DrawRay(GetEventPosition, _spriteBaseTr.Tr.forward, Color.red, 5f);
            }
            return true;
        }

        private void SetFrame() {
            _currentFrame = _currentAnimation.CurrentClip.GetFrame(_currentFrameIndex);
            if (_currentFrame != null) {
                _frameTimer.StartNewTime(_currentAnimation.CurrentClip.FrameTime * _currentFrame.Length);
            }
            //_frameTimer.StartNewTime(_currentAnimation.CurrentClip.FrameTime * _currentFrame?.Length ?? 1);
        }

        private bool ClipFinished(Entity entity) {
            _currentAnimation.EventTriggered = true;
            _currentAnimation = null;
            if (entity.IsDead()) {
                return false;
            }
            if (_animationClipQueue.Count > 0) {
                PlayAnimation(_animationClipQueue.Dequeue());
            }
            else {
                CheckMoving(entity);
            }
            return true;
        }

        public void ApplyMaterialBlocks(MaterialPropertyBlock[] blocks) {
            if (_renderers == null || blocks == null) {
                return;
            }
            for (int i = 0; i < _renderers.Length; i++) {
                _renderers[i].SetPropertyBlock(blocks[i]);
            }
        }
    }
}
