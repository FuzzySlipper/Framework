using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister, Priority(Priority.Higher)]
    public sealed class AnimationEventSystem : SystemBase, IReceiveGlobal<AnimationEventTriggered> {
        
        private Dictionary<string, List<IReceive<AnimationEventTriggered>>> _eventReceivers = new Dictionary<string, List<IReceive<AnimationEventTriggered>>>();

        public AnimationEventSystem() {
            TemplateFilter<AnimationEventTemplate>.Setup(AnimationEventTemplate.GetTypes());
        }

        public void Register(string eventName, IReceive<AnimationEventTriggered> receiver) {
            if (!_eventReceivers.TryGetValue(eventName, out var list)) {
                list = new List<IReceive<AnimationEventTriggered>>();
                _eventReceivers.Add(eventName, list);
            }
            list.Add(receiver);
        }
        public void HandleGlobal(AnimationEventTriggered arg) {
            var node = arg.Entity.GetTemplate<AnimationEventTemplate>();
            if (node != null) {
                node.AnimEvent.CurrentAnimationEvent = arg.Event;
                FindEventPositionRotation(node);
            }
            if (!_eventReceivers.TryGetValue(arg.Event, out var list)) {
                return;
            }
            for (int i = 0; i < list.Count; i++) {
                list[i].Handle(arg);
            }
        }

        private void FindEventPositionRotation(AnimationEventTemplate template) {
            if (template.CurrentAction.Value != null) {
                var weaponModel = template.CurrentAction.Value.Entity.Get<WeaponModelComponent>();
                if (weaponModel?.Loaded != null) {
                    template.AnimEvent.LastEventPosition = weaponModel.Loaded.Tr.position;
                    template.AnimEvent.LastEventRotation = weaponModel.Loaded.Tr.rotation;
                    return;
                }
            }
            if (template.SpriteAnimator != null) {
                template.AnimEvent.LastEventPosition = template.SpriteAnimator.CurrentAnimation.GetEventPosition(template.SpriteRenderer.Value,
                    template.SpriteAnimator.CurrentFrame);
                template.AnimEvent.LastEventRotation = template.SpriteRenderer.BaseTr.rotation;
                return;
            }
            if (template.SpawnPivot != null) {
                var isPrimary =  template.CurrentAction.Value?.Primary ?? false;
                var spawnPivot = isPrimary ? template.SpawnPivot.PrimaryPivot : template.SpawnPivot.SecondaryPivot;
                template.AnimEvent.LastEventPosition = spawnPivot.position;
                template.AnimEvent.LastEventRotation = spawnPivot.rotation;
            }
            else {
                template.AnimEvent.LastEventPosition = template.Tr.position;
                template.AnimEvent.LastEventRotation = template.Tr.rotation;
            }
        }

        public void SetTrigger(Entity entity, string trigger) {
            entity.Get<AnimationGraphComponent>()?.Value.Trigger(trigger);
        }
    }

    public class AnimationEventTemplate : BaseTemplate {

        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<CurrentAction> _currentAction = new CachedComponent<CurrentAction>();
        private CachedComponent<AnimationGraphComponent> _animGraph = new CachedComponent<AnimationGraphComponent>();
        private CachedComponent<SpawnPivotComponent> _spawnPivot = new CachedComponent<SpawnPivotComponent>();
        private CachedComponent<AnimationEventComponent> _animEvent = new CachedComponent<AnimationEventComponent>();
        private CachedComponent<SpriteAnimatorComponent> _spriteAnimator = new CachedComponent<SpriteAnimatorComponent>();
        private CachedComponent<SpriteRendererComponent> _spriteRenderer = new CachedComponent<SpriteRendererComponent>();

        public TransformComponent Tr { get => _tr.Value; }
        public SpawnPivotComponent SpawnPivot { get => _spawnPivot.Value; }
        public CurrentAction CurrentAction { get => _currentAction; }
        public AnimationGraphComponent AnimGraph { get => _animGraph; }
        public AnimationEventComponent AnimEvent { get => _animEvent; }
        public SpriteAnimatorComponent SpriteAnimator { get => _spriteAnimator; }
        public SpriteRendererComponent SpriteRenderer { get => _spriteRenderer; }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _tr, _spawnPivot, _currentAction, _animGraph, _animEvent, _spriteAnimator, _spriteRenderer
        };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(TransformComponent),
                typeof(AnimationEventComponent),
                typeof(CurrentAction),
            };
        }
    }
    
    
}
