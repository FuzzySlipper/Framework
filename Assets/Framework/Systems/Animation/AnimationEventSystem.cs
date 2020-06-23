using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister, Priority(Priority.Higher)]
    public sealed class AnimationEventSystem : SystemBase, IReceiveGlobal<AnimationEventTriggered> {
        
        private Dictionary<string, List<IReceive<AnimationEventTriggered>>> _eventReceivers = new Dictionary<string, List<IReceive<AnimationEventTriggered>>>();

        public AnimationEventSystem() {
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
                TriggerTemplateEvent(aeTemplate, arg);
            }
            if (!_eventReceivers.TryGetValue(arg.Event.EventType.ToString(), out var list)) {
                return;
            }
            for (int i = 0; i < list.Count; i++) {
                list[i].Handle(arg);
            }
        }
        
        private const bool LimitToEnemy = true;

<<<<<<< HEAD
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
                if (aet.SpriteRenderer != null) {
                    aet.AnimEvent.Position = aet.SpriteRenderer.GetEventPosition(aet.SpriteAnimator.CurrentFrame);
                    aet.AnimEvent.Rotation = target?.GetLookAtTarget(aet.AnimEvent.Position) ?? aet.SpriteRenderer.BaseTr.rotation;
                }
                else {
                    aet.AnimEvent.Rotation = target?.GetLookAtTarget(aet.AnimEvent.Position) ?? aet.SimpleSpriteRenderer.Rotation;
                }
                return;
            }
            if (aet.SpawnPivot != null) {
                aet.AnimEvent.Position = aet.SpawnPivot.position;
                aet.AnimEvent.Rotation = target?.GetLookAtTarget(aet.AnimEvent.Position) ?? aet.SpawnPivot.rotation;
=======
        private void TriggerTemplateEvent(AnimationEventTemplate aet, AnimationEventTriggered arg) {
            aet.AnimEvent.CurrentAnimationEvent = arg.Event;
            var action = aet.CurrentAction?.Value;
            aet.AnimEvent.Position = arg.Event.EventPosition;
            aet.AnimEvent.Rotation = aet.Target?.GetLookAtTarget(aet.AnimEvent.Position) ?? arg.Event.EventRotation;
            switch (arg.Event.EventType) {
                case AnimationEvent.Type.Spawn:
                    var spawn = arg.Event.EventDataObject as ProjectileConfig;
                    if (spawn != null) {
                        ProjectileFactory.SpawnProjectile(aet.Entity, spawn, aet.GetTargetPosition(), aet.AnimEvent.Position, aet.AnimEvent.Rotation);
                    }
                    else {
                        Debug.LogErrorFormat("No spawn for {0} {1}", aet.GetName(), arg.Event.EventDataObject != null ? arg.Event
                        .EventDataObject.name : null);
                    }
                    break;
                case AnimationEvent.Type.Camera:
                    ScriptingSystem.ExecuteCameraMessage(arg.Event.EventDataString.SplitIntoWords());
                    break;
                case AnimationEvent.Type.RaycastCollisionCheck:
                    if (action == null) {
                        return;
                    }
                    Vector3 originPos;
                    Vector3 target;
                    if (aet.IsPlayer()) {
                        originPos = PlayerInputSystem.GetLookTargetRay.origin;
                        target = PlayerInputSystem.GetMouseRaycastPosition(action.Config);
                    }
                    else {
                        originPos = aet.AnimEvent.Position;
                        target = aet.Target.GetPosition;
                    }
                    
                    var ray = new Ray(originPos, (target - originPos).normalized);
                    var raySize = action.Config.Source.Collision.GetRaySize();
                    var rayDistance = DistanceSystem.FromUnitGridDistance(action.Config.Range);
                    if (CollisionCheckSystem.Raycast(action.Entity, ray, rayDistance, LimitToEnemy) == null && raySize > 0.01f) {
                        CollisionCheckSystem.SphereCast(action.Entity, ray, rayDistance, raySize, LimitToEnemy);
                    }
                    break;
>>>>>>> FirstPersonAction
            }
            if (action != null) {
                World.Get<ActionSystem>().ProcessAnimationAction(aet, action, arg.Event);
            }
        }

       // private void FindEventPositionRotation(AnimationEventTemplate aet, AnimationEvent ae) {
            // if (aet.SpriteRenderer != null) {
            //     //aet.AnimEvent.Position = aet.SpriteRenderer.GetEventPosition(ae.EventPosition);
            // }
            // else {
            //     //aet.AnimEvent.Position = aet.SimpleSpriteRenderer.GetEventPosition(aet.SpriteAnimator.CurrentFrame, action?.Config.EquippedSlot ?? 0);
            //     aet.AnimEvent.Rotation = target?.GetLookAtTarget(aet.AnimEvent.Position) ?? aet.SimpleSpriteRenderer.;
            // }
            //if (action != null) {
                // if (action.Weapon?.Loaded != null) {
                //     aet.AnimEvent.Position = action.Weapon.Loaded.Spawn.position;
                //     aet.AnimEvent.Rotation = target?.GetLookAtTarget(aet.AnimEvent.Position) ?? action.Weapon.Loaded.Spawn.rotation;
                //     return;
                // }
                // if (action.SpawnPivot != null) {
                //     aet.AnimEvent.Position = action.SpawnPivot.position;
                //     aet.AnimEvent.Rotation = target?.GetLookAtTarget(aet.AnimEvent.Position) ?? action.SpawnPivot.rotation;
                //     return;
                // }
            //}
            // if (aet.SpriteAnimator?.CurrentFrame != null) {
            //     if (aet.SpriteRenderer != null) {
            //         aet.AnimEvent.Position = aet.SpriteRenderer.GetEventPosition(aet.SpriteAnimator.CurrentFrame);
            //         aet.AnimEvent.Rotation = target?.GetLookAtTarget(aet.AnimEvent.Position) ?? aet.SpriteRenderer.BaseTr.rotation;
            //     }
            //     else {
            //         aet.AnimEvent.Position = aet.SimpleSpriteRenderer.GetEventPosition(aet.SpriteAnimator.CurrentFrame, action?.Config.EquippedSlot ?? 0);
            //         aet.AnimEvent.Rotation = target?.GetLookAtTarget(aet.AnimEvent.Position) ?? aet.SimpleSpriteRenderer.Rotation;
            //     }
            //     return;
            // }
            // if (aet.SpawnPivot != null) {
            //     aet.AnimEvent.Position = aet.SpawnPivot.position;
            //     aet.AnimEvent.Rotation = target?.GetLookAtTarget(aet.AnimEvent.Position) ?? aet.SpawnPivot.rotation;
            // }
            // else {
            //     aet.AnimEvent.Position = aet.Tr.position;
            //     aet.AnimEvent.Rotation = target?.GetLookAtTarget(aet.AnimEvent.Position) ?? aet.Tr.rotation;
            // }
        //}
    }

    public class AnimationEventTemplate : BaseTemplate {

        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<CurrentAction> _currentAction = new CachedComponent<CurrentAction>();
        private CachedComponent<AnimationGraphComponent> _animGraph = new CachedComponent<AnimationGraphComponent>();
        private CachedComponent<SpawnPivotComponent> _spawnPivot = new CachedComponent<SpawnPivotComponent>();
        private CachedComponent<AnimationEventComponent> _animEvent = new CachedComponent<AnimationEventComponent>();
        private CachedComponent<SpriteAnimatorComponent> _spriteAnimator = new CachedComponent<SpriteAnimatorComponent>();
        private CachedComponent<SpriteRendererComponent> _spriteRenderer = new CachedComponent<SpriteRendererComponent>();
        private CachedComponent<SpriteSimpleRendererComponent> _simpleSpriteRenderer = new CachedComponent<SpriteSimpleRendererComponent>();
        private CachedComponent<CommandTarget> _target = new CachedComponent<CommandTarget>();
        public TransformComponent Tr { get => _tr.Value; }
        public SpawnPivotComponent SpawnPivot { get => _spawnPivot.Value; }
        public CurrentAction CurrentAction { get => _currentAction; }
        public AnimationGraphComponent AnimGraph { get => _animGraph; }
        public AnimationEventComponent AnimEvent { get => _animEvent; }
        public SpriteAnimatorComponent SpriteAnimator { get => _spriteAnimator; }
        public SpriteRendererComponent SpriteRenderer { get => _spriteRenderer; }
        public SpriteSimpleRendererComponent SimpleSpriteRenderer { get => _simpleSpriteRenderer; }
        public CommandTarget Target => _target.Value;


        public Vector3 GetTargetPosition() {
            if (this.IsPlayer()) {
                return PlayerInputSystem.GetMouseRaycastPosition(DistanceSystem.FromUnitGridDistance( CurrentAction.Value.Config.Range));
            }
            return Target.GetPosition;
        }
        
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _tr, _spawnPivot, _currentAction, _animGraph, _animEvent, _spriteAnimator, _spriteRenderer, _target, _simpleSpriteRenderer
        };
        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(TransformComponent),
                typeof(AnimationEventComponent),
                typeof(CurrentAction),
            };
        }
    }
    
    
}
