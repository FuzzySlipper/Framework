using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class DirectionalSpriteSystem : SystemBase, IMainSystemUpdate {

        private NodeList<DirectionalSpriteNode> _directionalComponents;
        
        public DirectionalSpriteSystem() {
            NodeFilter<DirectionalSpriteNode>.Setup(DirectionalSpriteNode.GetTypes());
            _directionalComponents = EntityController.GetNodeList<DirectionalSpriteNode>();
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            _directionalComponents.Run(UpdateNode);
        }

        private void UpdateNode(ref DirectionalSpriteNode node) {
            if (node.Animator.Requests.Count > 0) {
                var request = node.Animator.Requests[0];
                if (node.Animator.Requests.Count > 1) {
                    for (int r = 1; r < node.Animator.Requests.Count; r++) {
                        if (!request.OverrideClip && node.Animator.Requests[r].OverrideClip) {
                            request = node.Animator.Requests[r];
                        }
                    }
                }
#if DEBUG
                DebugLog.Add("Playing " + request.Clip + " for " + node.Entity.DebugId);
#endif
                node.PlayAnimation(request.Clip, request.OverrideClip);
                node.Animator.Requests.Clear();
            }
            if (node.Animator.CurrentClipHolder == null || node.Animator.IsSimpleClip) {
                node.CheckMoving();
            }
            node.Animator.Billboard.Apply(node.Renderer.SpriteTr, node.Animator.Backwards, ref node.Animator.LastAngleHeight);
            var orientation = SpriteFacingControl.GetCameraSide(
                node.Animator.Facing, node.Renderer.SpriteTr,
                node.Renderer.BaseTr, 5, out var inMargin);
            if (node.Animator.Orientation == orientation || (inMargin && (orientation.IsAdjacent(node.Animator.Orientation)))) {
                if (node.CheckFrameUpdate()) {
                    node.UpdateSpriteFrame();
                }
                return;
            }
            node.Animator.Orientation = orientation;
            node.UpdateSpriteFrame();
        }

    }

    public class DirectionalSpriteNode : BaseNode {

        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<DirectionalSpriteAnimationComponent> _animator = new CachedComponent<DirectionalSpriteAnimationComponent>();
        private CachedComponent<SpriteRendererComponent> _renderer = new CachedComponent<SpriteRendererComponent>();
        private CachedComponent<SpriteColliderComponent> _collider = new CachedComponent<SpriteColliderComponent>();

        public TransformComponent Tr { get => _tr.Value; }
        public DirectionalSpriteAnimationComponent Animator { get => _animator; }
        public SpriteRendererComponent Renderer { get => _renderer; }
        public SpriteColliderComponent Collider { get => _collider; }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _tr, _collider, _renderer, _animator
        };

        public void PlayAnimation(string clip, bool overrideClip) {
            var clipHolder = Animator.GetClipHolder(clip);
            if (clipHolder == null || clipHolder.Clips.Length == 0) {
                return;
            }
            if (Animator.AnimationPlaying && !Animator.IsSimpleClip) {
                if (Animator.CurrentClipHolder == clipHolder) {
                    return;
                }
                if (!overrideClip) {
                    clipHolder.ResetBools();
                    if (!Animator.AnimationClipQueue.Contains(clipHolder)) {
                        Animator.AnimationClipQueue.Enqueue(clipHolder);
                    }
                    return;
                }
            }
            PlayAnimation(clipHolder);
        }

        public void UpdateSpriteFrame() {
            var facing = Animator.Orientation;
            if (Animator.Facing.RequiresFlipping()) {
                facing = Animator.Orientation.GetFlippedSide();
                Renderer.Value.flipX = Animator.Orientation.IsFlipped();
            }
            if (Animator.CurrentClipHolder == null) {
                return;
            }
            var sprite = Animator.CurrentClipHolder.CurrentClip.GetSpriteFrame(facing, Animator.CurrentFrameIndex);
            if (sprite == null) {
                return;
            }
            Renderer.Value.sprite = sprite;
            if (Collider != null) {
                Collider.Value.UpdateCollider();
            }
        }

        public bool CheckFrameUpdate() {
            if (!Animator.Playing || Animator.FrameTimer.IsActive || Animator.CurrentClipHolder == null || Entity.Tags.IsStunned) {
                return false;
            }
            Animator.CurrentFrameIndex++;
            SetFrame();
            if (Animator.CurrentFrame == null) {
                Animator.CurrentClipHolder.Complete = true;
                if (Animator.CurrentClipHolder.CurrentClip.Looping) {
                    Animator.CurrentFrameIndex = 0;
                    SetFrame();
                    return true;
                }
                Animator.Playing = false;
                if (!ClipFinished(Entity)) {
                    return false;
                }
                Animator.CurrentFrameIndex = 0;
                SetFrame();
                return true;
            }
            if (Animator.CurrentFrame.HasEvent) {
                if (Animator.CurrentFrame.Event == AnimationFrame.EventType.Default) {
                    Animator.CurrentClipHolder.EventTriggered = true;
                    Animator.CurrentAnimationEvent = AnimationEvents.Default;
                }
                else {
                    Animator.CurrentAnimationEvent = Animator.CurrentFrame.EventName;
                }
                Animator.GetEventPosition = Animator.CurrentClipHolder.CurrentClip.GetEventPosition(Renderer.Value, Animator.CurrentFrame);
                Animator.GetEventRotation = Renderer.BaseTr.rotation;
            }
            return true;
        }

        public void SetFrame() {
            Animator.CurrentFrame = Animator.CurrentClipHolder.CurrentClip.GetFrame(Animator.CurrentFrameIndex);
            if (Animator.CurrentFrame != null) {
                Animator.FrameTimer.StartNewTime(Animator.CurrentClipHolder.CurrentClip.FrameTime * Animator.CurrentFrame.Length);
            }
            //_frameTimer.StartNewTime(_currentAnimation.CurrentClip.FrameTime * _currentFrame?.Length ?? 1);
        }

        public bool ClipFinished(Entity entity) {
            Animator.CurrentClipHolder.EventTriggered = true;
            Animator.CurrentClipHolder = null;
            if (entity.IsDead()) {
                return false;
            }
            if (Animator.AnimationClipQueue.Count > 0) {
                PlayAnimation(Animator.AnimationClipQueue.Dequeue());
            }
            else {
                CheckMoving();
            }
            return true;
        }

        public void CheckMoving() {
            if (Entity == null || Entity.IsDead()) {
                return;
            }
            if (Animator.CurrentClipHolder == null) {
                PlayAnimation(Entity.Tags.Contain(EntityTags.Moving) ? AnimationIds.Move : AnimationIds.Idle, false);
            }
            else if (Animator.CurrentClipID == AnimationIds.Move && !Entity.Tags.Contain(EntityTags.Moving)) {
                PlayAnimation(AnimationIds.Idle, false);
            }
            else if (Animator.CurrentClipID == AnimationIds.Idle && Entity.Tags.Contain(EntityTags.Moving)) {
                PlayAnimation(AnimationIds.Move, false);
            }
        }

        public void PlayAnimation(DirectionalAnimationClipHolder clipHolder) {
            clipHolder.SetRandomIndex();
            if (clipHolder.CurrentClip == null) {
                return;
            }
            Animator.CurrentClipID = clipHolder.Id;
            Animator.CurrentClipHolder = clipHolder;
            clipHolder.ResetBools();
            Animator.GetEventPosition = Renderer.BaseTr.position;
            Animator.CurrentFrameIndex = -1;
            Animator.Playing = true;
            Animator.FrameTimer.Cancel();
            var block = Renderer.MaterialBlocks[0];
            block.SetTexture("_BumpMap", clipHolder.CurrentClip.NormalMap);
            block.SetTexture("_EmissionMap", clipHolder.CurrentClip.EmissiveMap);
            if (clipHolder.CurrentClip.EmissiveMap != null) {
                Renderer.Value.material.EnableKeyword("_EMISSION");
            }
            else {
                Renderer.Value.material.DisableKeyword("_EMISSION");
            }
            Renderer.Value.SetPropertyBlock(block);
        }

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(TransformComponent),
                typeof(DirectionalSpriteAnimationComponent),
                typeof(SpriteRendererComponent),
            };
        }
    }
}
