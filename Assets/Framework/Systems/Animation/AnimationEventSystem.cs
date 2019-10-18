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
            var aeTemplate = arg.Entity.GetTemplate<AnimationEventTemplate>();
            if (aeTemplate != null) {
                aeTemplate.AnimEvent.CurrentAnimationEvent = arg.Event;
                var action = aeTemplate.CurrentAction?.Value;
                FindEventPositionRotation(aeTemplate, action);
                if (action != null) {
                    World.Get<ActionSystem>().ProcessAnimationAction(aeTemplate, action, arg.Event);
                }
            }
            if (!_eventReceivers.TryGetValue(arg.Event, out var list)) {
                return;
            }
            for (int i = 0; i < list.Count; i++) {
                list[i].Handle(arg);
            }
        }

        private void FindEventPositionRotation(AnimationEventTemplate aet, ActionTemplate action) {
            var target = aet.Target;
            if (action != null) {
                if (action.Weapon?.Loaded != null) {
                    aet.AnimEvent.Position = action.Weapon.Loaded.Spawn.position;
                    aet.AnimEvent.Rotation = target?.GetLookAtTarget(aet.AnimEvent.Position) ?? action.Weapon.Loaded.Spawn.rotation;
                    return;
                }
                if (action.SpawnPivot != null) {
                    aet.AnimEvent.Position = action.SpawnPivot.position;
                    aet.AnimEvent.Rotation = target?.GetLookAtTarget(aet.AnimEvent.Position) ?? action.SpawnPivot.rotation;
                    return;
                }
            }
            if (aet.SpriteAnimator?.CurrentFrame != null) {
                aet.AnimEvent.Position = aet.SpriteRenderer.GetEventPosition(aet.SpriteAnimator.CurrentFrame);
                aet.AnimEvent.Rotation = target?.GetLookAtTarget(aet.AnimEvent.Position) ?? aet.SpriteRenderer.BaseTr.rotation;
                return;
            }
            if (aet.SpawnPivot != null) {
                aet.AnimEvent.Position = aet.SpawnPivot.position;
                aet.AnimEvent.Rotation = target?.GetLookAtTarget(aet.AnimEvent.Position) ?? aet.SpawnPivot.rotation;
            }
            else {
                aet.AnimEvent.Position = aet.Tr.position;
                aet.AnimEvent.Rotation = target?.GetLookAtTarget(aet.AnimEvent.Position) ?? aet.Tr.rotation;
            }
        }

        public void SetTrigger(Entity entity, string trigger) {
            entity.Get<AnimationGraphComponent>()?.Value.TriggerGlobal(trigger);
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
        private CachedComponent<CommandTarget> _target = new CachedComponent<CommandTarget>();
        public TransformComponent Tr { get => _tr.Value; }
        public SpawnPivotComponent SpawnPivot { get => _spawnPivot.Value; }
        public CurrentAction CurrentAction { get => _currentAction; }
        public AnimationGraphComponent AnimGraph { get => _animGraph; }
        public AnimationEventComponent AnimEvent { get => _animEvent; }
        public SpriteAnimatorComponent SpriteAnimator { get => _spriteAnimator; }
        public SpriteRendererComponent SpriteRenderer { get => _spriteRenderer; }
        public CommandTarget Target => _target.Value;
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _tr, _spawnPivot, _currentAction, _animGraph, _animEvent, _spriteAnimator, _spriteRenderer, _target
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
