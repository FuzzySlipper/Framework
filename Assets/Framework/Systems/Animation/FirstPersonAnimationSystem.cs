using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class FirstPersonAnimationSystem : SystemBase, IReceive<ReadyActionsChanged>, IReceive<AnimationEventTriggered> {
        
        private TemplateList<FirstPersonAnimationTemplate> _animTemplates;
        
        public FirstPersonAnimationSystem(){
            TemplateFilter<FirstPersonAnimationTemplate>.Setup();
            _animTemplates = EntityController.GetTemplateList<FirstPersonAnimationTemplate>();
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new [] {
                typeof(WeaponModelComponent),
                typeof(PoseAnimatorComponent)
            }));
        }

        public void Handle(ReadyActionsChanged arg) {
            if (arg.Action == null) {
                return;
            }
            var weaponModelComponent = arg.Action.Weapon;
            if (weaponModelComponent == null) {
                if (arg.Container == null) {
                    arg.Action.Entity.Remove<SpawnPivotComponent>();
                }
                else {
                    var actionPivots = arg.Container.GetEntity().Get<ActionPivotsComponent>();
                    if (actionPivots != null) {
                        arg.Action.Entity.Add(new SpawnPivotComponent(arg.Action.Config.Primary ? actionPivots.PrimaryPivot : actionPivots
                        .SecondaryPivot));
                    }
                }
                return;
            }
            if (arg.Container == null) {
                ItemPool.Despawn(weaponModelComponent.Loaded.Tr.gameObject);
                arg.Action.Entity.Remove<TransformComponent>();
                arg.Action.Entity.Remove<SpawnPivotComponent>();
                weaponModelComponent.Set(null);
            }
            else {
                if (weaponModelComponent.Loaded != null) {
                    return;
                }
                var weaponModel = ItemPool.Spawn(UnityDirs.Weapons, weaponModelComponent.Prefab, Vector3.zero, Quaternion.identity, false,
                 false);
                if (weaponModel == null) {
                    return;
                }
                var projectileSpawn = arg.Container.GetEntity().Get<ActionPivotsComponent>();
                if (projectileSpawn != null) {
                    weaponModel.Transform.SetParentResetPos(arg.Action.Config.Primary? projectileSpawn.PrimaryPivot : projectileSpawn
                    .SecondaryPivot);
                }
                weaponModelComponent.Set(weaponModel.GetComponent<IWeaponModel>());
                arg.Action.Entity.Add(new TransformComponent(weaponModel.transform));
                arg.Action.Entity.Add(new SpawnPivotComponent(weaponModel.Transform));
            }
        }

        public void Handle(AnimationEventTriggered arg) {
            switch (arg.Event.EventType) {
                case AnimationEvent.Type.Camera:
                    World.Get<CameraSystem>().PlaySpringAnimation(arg.Event.EventDataString);
                    break;
            } 
        }
    }

    public class FirstPersonAnimationTemplate : BaseTemplate {

        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<ColliderComponent> _collider = new CachedComponent<ColliderComponent>();
        private CachedComponent<WeaponBobComponent> _weaponBob = new CachedComponent<WeaponBobComponent>();
        private CachedComponent<CurrentAction> _currentAction = new CachedComponent<CurrentAction>();
        private CachedComponent<AnimationGraphComponent> _animGraph = new CachedComponent<AnimationGraphComponent>();
        
        public TransformComponent Tr { get => _tr.Value; }
        public Collider Collider { get => _collider.Value.Collider; }
        public CurrentAction CurrentAction { get => _currentAction; }
        public WeaponBobComponent WeaponBob { get => _weaponBob; }
        public AnimationGraphComponent AnimGraph { get => _animGraph; }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _tr, _collider, _weaponBob, _currentAction, _animGraph
        };

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(PoseAnimatorComponent),
                typeof(TransformComponent),
            };
        }
    }
}
