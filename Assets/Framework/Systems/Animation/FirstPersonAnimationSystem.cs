using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class FirstPersonAnimationSystem : SystemBase, IMainSystemUpdate, IReceive<ReadyActionsChanged>, IReceive<AnimationEventTriggered> {
        
        private GameOptions.CachedBool _useWeaponBob = new GameOptions.CachedBool("UseWeaponBob");
        
        private TemplateList<FirstPersonAnimationTemplate> _animTemplates;
        private ManagedArray<FirstPersonAnimationTemplate>.RefDelegate _animDel;
        
        public FirstPersonAnimationSystem(){
            TemplateFilter<FirstPersonAnimationTemplate>.Setup(FirstPersonAnimationTemplate.GetTypes());
            _animTemplates = EntityController.GetTemplateList<FirstPersonAnimationTemplate>();
            _animDel = HandleAnimNodes;
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new [] {
                typeof(WeaponModelComponent),
                typeof(PlayerComponent)
            }));
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            _animTemplates.Run(_animDel);
        }
        
        private void HandleAnimNodes(ref FirstPersonAnimationTemplate template) {
            var dt = TimeManager.DeltaTime;
            if (template.WeaponBob != null && _useWeaponBob && template.AnimGraph.Value.CurrentTag != GraphNodeTags.Action) {
                template.WeaponBob.BobTime += dt;
                var velocity = Player.FirstPersonController.VelocityPercent;
                var y = template.WeaponBob.VerticalSwayAmount * Mathf.Sin((template.WeaponBob.SwaySpeed * 2) * template.WeaponBob.BobTime) * velocity;
                var x = template.WeaponBob.HorizontalSwayAmount * Mathf.Sin(template.WeaponBob.SwaySpeed * template.WeaponBob.BobTime) * velocity;
                template.WeaponBob.ArmsPivot.localPosition = template.WeaponBob.ResetPoint + new Vector3(x, y, 0);
            }
        }

        public void Handle(ReadyActionsChanged arg) {
            if (arg.Action == null) {
                return;
            }
            var entity = arg.Action.Entity;
            var weaponModelComponent = entity.Get<WeaponModelComponent>();
            if (weaponModelComponent == null) {
                if (arg.Container == null) {
                    entity.Remove<SpawnPivotComponent>();
                }
                else {
                    var actionPivots = arg.Container.GetEntity().Get<ActionPivotsComponent>();
                    if (actionPivots != null) {
                        entity.Add(new SpawnPivotComponent(arg.Action.Primary ? actionPivots.PrimaryPivot : actionPivots.SecondaryPivot));
                    }
                }
                return;
            }
            if (arg.Container == null) {
                ItemPool.Despawn(weaponModelComponent.Loaded.Tr.gameObject);
                entity.Remove<TransformComponent>();
                entity.Remove<SpawnPivotComponent>();
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
                    weaponModel.Transform.SetParentResetPos(arg.Action.Primary? projectileSpawn.PrimaryPivot : projectileSpawn.SecondaryPivot);
                }
                weaponModelComponent.Set(weaponModel.GetComponent<IWeaponModel>());
                entity.Add(new TransformComponent(weaponModel.transform));
                entity.Add(new SpawnPivotComponent(weaponModel.Transform));
            }
        }

        public void Handle(AnimationEventTriggered arg) {
            switch (arg.Event) {
                case AnimationEvents.Default:
                    break;
                case AnimationEvents.FxOn:
                    var weaponModelOn = arg.Entity.Get<CurrentAction>()?.Value?.Entity.Get<WeaponModelComponent>() ?? 
                                      arg.Entity.Get<WeaponModelComponent>();
                    if (weaponModelOn?.Loaded != null) {
                        weaponModelOn.Loaded.SetFx(true);
                    }
                    break;
                case AnimationEvents.FxOff:
                    var weaponModelOff = arg.Entity.Get<CurrentAction>()?.Value?.Entity.Get<WeaponModelComponent>() ??
                                      arg.Entity.Get<WeaponModelComponent>();
                    if (weaponModelOff?.Loaded != null) {
                        weaponModelOff.Loaded.SetFx(false);
                    }
                    break;
                default:
                    if (arg.Entity.Tags.Contain(EntityTags.Player)) {
                        FirstPersonCamera.PlaySpringAnimation(arg.Event);
                    }
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

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(TransformComponent),
            };
        }
    }
}
