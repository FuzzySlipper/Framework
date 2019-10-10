using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class DirectionalSpriteSystem : SystemBase, IMainSystemUpdate {

        private TemplateList<DirectionalSpriteTemplate> _directionalComponents;
        private ManagedArray<DirectionalSpriteTemplate>.RefDelegate _del;
        
        public DirectionalSpriteSystem() {
            TemplateFilter<DirectionalSpriteTemplate>.Setup(DirectionalSpriteTemplate.GetTypes());
            _directionalComponents = EntityController.GetTemplateList<DirectionalSpriteTemplate>();
            _del = UpdateNode;
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            _directionalComponents.Run(_del);
        }

        private void UpdateNode(ref DirectionalSpriteTemplate template) {
            template.Animator.Billboard.Apply(template.Renderer.SpriteTr, template.Animator.Backwards, ref template.Animator.LastAngleHeight);
            if (template.Animator.Requests.Count > 0) {
                var request = template.Animator.Requests[0];
                if (template.Animator.Requests.Count > 1) {
                    for (int r = 1; r < template.Animator.Requests.Count; r++) {
                        if (!request.OverrideClip && template.Animator.Requests[r].OverrideClip) {
                            request = template.Animator.Requests[r];
                        }
                    }
                }
#if DEBUG
                DebugLog.Add("Playing " + request.Clip + " for " + template.Entity.DebugId);
#endif
                template.PlayAnimation(request.Clip, request.OverrideClip);
                template.Animator.Requests.Clear();
            }
            
            if (template.Animator.CurrentClipHolder == null || template.Animator.IsSimpleClip) {
                template.CheckMoving();
            }
            
            var orientation = SpriteFacingControl.GetCameraSide(template.Animator.Facing, template.Renderer.SpriteTr,
                template.Renderer.BaseTr, 5, out var inMargin);
            if (template.Animator.Orientation == orientation || (inMargin && (orientation.IsAdjacent(template.Animator.Orientation)))) {
                if (template.CheckFrameUpdate()) {
                    template.UpdateSpriteFrame();
                }
                return;
            }
            template.Animator.Orientation = orientation;
            template.CheckFrameUpdate();
            template.UpdateSpriteFrame();
        }

    }

    public class DirectionalSpriteTemplate : BaseTemplate {

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
