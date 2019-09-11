using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelComrades.DungeonCrawler;

namespace PixelComrades {
    public struct SpriteAnimationComponent : IComponent {
        public int Owner { get; set; }
        public SpriteRenderer Renderer { get; }
        public SpriteAnimation Animation { get; }
        public bool Unscaled { get;}
        public BillboardMode Billboard { get; }

        public float NextUpdateTime;
        public short CurrentFrameIndex;
        public float LastAngleHeight;

        public SpriteAnimationComponent(SpriteRenderer renderer, SpriteAnimation animation, bool unscaled, BillboardMode billboard) : this() {
            Renderer = renderer;
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

    public class DirectionalSpriteAnimationComponent : ComponentBase, IModelComponent, IAnimator, IDisposable {
        private BillboardMode _billboard;
        private bool _unscaled;
        private SpriteRenderer _renderer;
        private SpriteCollider _spriteCollider;
        private Transform _spriteBaseTr;
        private Transform _spriteTr;
        
        private SpriteFacing _facing;
        private bool _backwards;

        private int _currentFrameIndex = 0;
        private bool _playing = false;
        private Timer _frameTimer;
        private AnimationFrame _currentFrame;
        private float _lastAngleHeight;

        private DirectionalAnimationClipHolder _currentAnimation;
        private string _currentClipID;
        private Dictionary<string, DirectionalAnimationClipHolder> _animDictionary = new Dictionary<string, DirectionalAnimationClipHolder>();
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
        public Quaternion GetEventRotation { get { return _spriteBaseTr.rotation; } }
        public SpriteCollider SpriteCollider { get => _spriteCollider; }
        public DirectionsEight Orientation { get => _orientation; }
        public string CurrentAnimationEvent { get; private set; }
        public MaterialPropertyBlock[] MaterialBlocks {
            get {
                if (_renderer == null) {
                    return null;
                }
                _renderer.GetPropertyBlock(_blocks[0]);
                return _blocks;
            }
        }

        public DirectionalSpriteAnimationComponent(SpriteHolder animator, Dictionary<string, List<DirectionalAnimation>> dict) {
            _unscaled = animator.Unscaled;
            _billboard = animator.Billboard;
            _renderer = animator.Renderer;
            _spriteBaseTr = animator.SpriteBaseTr;
            _spriteTr = animator.SpriteTr;
            _facing = animator.Facing;
            _backwards = animator.Backwards;

            _frameTimer = new Timer(0, _unscaled);
            _renderers[0] = _renderer;
            _spriteCollider = _renderer.GetComponent<SpriteCollider>();
            _blocks[0] = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(_blocks[0]);
            foreach (var animList in dict) {
                if (!_animDictionary.TryGetValue(animList.Key, out var holder)) {
                    _animDictionary.Add(animList.Key, new DirectionalAnimationClipHolder(animList.Key, animList.Value.ToArray()));
                    continue;
                }
                var list = holder.Clips.ToList();
                list.AddRange(animList.Value);
                holder.Clips = list.ToArray();
            }
            CheckMoving();
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
            if (_currentAnimation == null || IsSimpleClip) {
                CheckMoving();
            }
            _billboard.Apply(_spriteTr, _backwards, ref _lastAngleHeight);
            var orientation = SpriteFacingControl.GetCameraSide(_facing, _spriteTr, _spriteBaseTr, 5, out var inMargin);
            if (_orientation == orientation || (inMargin && (orientation.IsAdjacent(_orientation)))) {
                if (CheckFrameUpdate()) {
                    UpdateSpriteFrame();
                }
                return;
            }
            _orientation = orientation;
            UpdateSpriteFrame();
        }

        private void CheckMoving() {
            if (Entity == null || Entity.IsDead()) {
                return;
            }
            if (_currentAnimation == null) {
                PlayAnimation(Entity.Tags.Contain(EntityTags.Moving) ? AnimationIds.Move : AnimationIds.Idle, false, null);
            }
            else if (_currentClipID == AnimationIds.Move && !Entity.Tags.Contain(EntityTags.Moving)) {
                PlayAnimation(AnimationIds.Idle, false, null);
            }
            else if (_currentClipID == AnimationIds.Idle && Entity.Tags.Contain(EntityTags.Moving)) {
                PlayAnimation(AnimationIds.Move, false, null);
            }
        }

        private void UpdateSpriteFrame() {
            var facing = _orientation;
            if (_facing.RequiresFlipping()) {
                facing = _orientation.GetFlippedSide();
                _renderer.flipX = _orientation.IsFlipped();
            }
            if (_currentAnimation == null) {
                return;
            }
            var sprite = _currentAnimation.CurrentClip.GetSpriteFrame(facing, _currentFrameIndex);
            if (sprite == null) {
                return;
            }
            _renderer.sprite = sprite;
            if (_spriteCollider != null) {
                _spriteCollider.UpdateCollider();
            }
        }

        public void SetVisible(bool status) {
            if (_renderer == null) {
                return;
            }
            _renderer.enabled = status;
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
            GetEventPosition = _spriteBaseTr.position;
            _currentFrameIndex = -1;
            _playing = true;
            _frameTimer.Cancel();
            _blocks[0].SetTexture("_BumpMap", clipHolder.CurrentClip.NormalMap);
            _blocks[0].SetTexture("_EmissionMap", clipHolder.CurrentClip.EmissiveMap);
            if (clipHolder.CurrentClip.EmissiveMap != null) {
                _renderer.material.EnableKeyword("_EMISSION");
            }
            else {
                _renderer.material.DisableKeyword("_EMISSION");
            }
            _renderer.SetPropertyBlock(_blocks[0]);
        }

        public bool CheckFrameUpdate() {
            if (!_playing || _frameTimer.IsActive || _currentAnimation == null || Entity.Tags.IsStunned) {
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
                if (!ClipFinished()) {
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
                Debug.DrawRay(GetEventPosition, _spriteBaseTr.forward, Color.red, 5f);
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

        private bool ClipFinished() {
            _currentAnimation.EventTriggered = true;
            _currentAnimation = null;
            if (Entity.IsDead()) {
                return false;
            }
            if (_animationClipQueue.Count > 0) {
                PlayAnimation(_animationClipQueue.Dequeue());
            }
            else {
                CheckMoving();
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
