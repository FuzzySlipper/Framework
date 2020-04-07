using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;

namespace PixelComrades {
    public class ProjectileFactory : ScriptableSingleton<ProjectileFactory>, IEntityFactory {
        private static GameOptions.CachedInt _defaultPool = new GameOptions.CachedInt("ProjectilePoolSize");

        private class ProjectileLoader : LoadOperationEvent {
            private Entity _entity;
            private Vector3 _target;
            private Vector3 _spawnPos;
            private Quaternion _spawnRot;
            private ActionFx _actionFx;
            private ProjectileConfig _config;

            public void Set(ProjectileConfig config, Entity entity, Vector3 target, Vector3 spawnPos, Quaternion spawnRot, ActionFx actionFx) {
                SourcePrefab = config.Prefab;
                _config = config;
                _entity = entity;
                _target = target;
                _spawnPos = spawnPos;
                _spawnRot = spawnRot;
                _actionFx = actionFx;
            }

            public override void OnComplete() {
                NewPrefab.transform.SetPositionAndRotation(_spawnPos, _spawnRot);
                var spawn = NewPrefab.GetComponent<IProjectile>();
                _entity.Add(new TransformComponent(NewPrefab.Transform));
                var template = _entity.GetTemplate<ProjectileTemplate>();
                template.MoveTarget.SetMoveTarget(_target);
                if (_config.ActionFx != null) {
                    template.ActionFx.ChangeFx(_config.ActionFx);
                }
                if (_actionFx != null) {
                    if (template.ActionFx != null) {
                        template.ActionFx.ChangeFx(_actionFx);
                    }
                    else {
                        _entity.Add(new ActionFxComponent(_actionFx));
                    }
                }
                switch (_config.Movement) {
                    case ProjectileMovement.Arc:
                    case ProjectileMovement.Forward:
                        template.CollisionCheckForward.LastPos = null;
                        NewPrefab.Transform.LookAt(_target, NewPrefab.Transform.up);
                        break;
                    case ProjectileMovement.Force:
                        //var force = transform.forward * ForceRange.Lerp(Mathf.Clamp01(charging.ElapsedTime / MaxChargeTime));
                        break;
                }
                spawn.SetConfig(_config, _entity);
                if (spawn.Rigidbody != null) {
                    template.Rb.SetRb(spawn.Rigidbody);
                }
                _entity.Tags.Add(EntityTags.Moving);
                template.Rendering.Set(spawn);
                UnityToEntityBridge.RegisterToEntity(NewPrefab.Transform.gameObject, _entity);
                _entity.Post(new ProjectileSpawned(_config, _entity));
                Clear();
            }

            private void Clear() {
                SourcePrefab = null;
                NewPrefab = null;
                _entity = null;
                _actionFx = null;
                _loadPool.Store(this);
            }
        }
        
        private static GameOptions.CachedFloat _defaultTimeout = new GameOptions.CachedFloat("ProjectileTimeout");
        private static GameOptions.CachedFloat _defaultSpeed = new GameOptions.CachedFloat("ProjectileSpeed");
        private static GameOptions.CachedFloat _defaultRotation = new GameOptions.CachedFloat("ProjectileRotation");
        private static Dictionary<string, ProjectileConfig> _configs = new Dictionary<string, ProjectileConfig>();
        private static GenericPool<ProjectileLoader> _loadPool = new GenericPool<ProjectileLoader>(2);
        private Dictionary<string, ManagedArray<Entity>> _poolDict = new Dictionary<string, ManagedArray<Entity>>();
        
        [SerializeField] private ProjectileConfig[] _allItems = new ProjectileConfig[0];

        private static void Init() {
            GameData.AddInit(Init);
            _configs.Clear();
            for (int i = 0; i < Main._allItems.Length; i++) {
                var data = Main._allItems[i];
                _configs.Add(data.ID, data);
            }
        }

        public ManagedArray<Entity> GetPool(string typeID) {
            return _poolDict.TryGetValue(typeID, out var stack) ? stack : null;
        }

        public bool TryStore(Entity entity) {
            if (_poolDict.TryGetValue(entity.Get<TypeId>().Id, out var stack)) {
                if (!stack.IsFull) {
                    UnityToEntityBridge.Unregister(entity);
                    entity.Get<RenderingComponent>().Clear();
                    entity.Remove(typeof(SpriteAnimationComponent));
                    entity.Tags.Clear();
                    entity.Post(new ProjectileDespawned(entity));
                    entity.Pooled = true;
                    entity.ClearParent();
                    stack.Add(entity);
                    return true;
                }
            }
            //entity.Destroy();
            return false;
        }
        

        public static void SpawnProjectile(Entity owner, string id, Vector3 target, Vector3 spawnPos, Quaternion spawnRot, ActionFx 
        actionFx = null) {
            if (_configs.Count == 0) {
                Init();
            }
            if (!_configs.TryGetValue(id, out var config)) {
#if DEBUG
                DebugLog.Add("Couldn't find project config " + id, false);
#endif
                return;
            }
            SpawnProjectile(owner, config, target, spawnPos, spawnRot, actionFx);
        }

        public static void SpawnProjectile(Entity owner, ProjectileConfig config, Vector3 target, Vector3 spawnPos, Quaternion spawnRot, ActionFx actionFx = null) {
            var entity = Main.GetProjectile(config);
            entity.ParentId = owner.Id;
            var projectileEvent = _loadPool.New();
            if (config.Type == ProjectileType.SpriteAnimation && !config.Animation.RuntimeKeyIsValid()) {
                config.Animation.LoadAssetAsync<SpriteAnimation>();
            }
            projectileEvent.Set(config, entity, target, spawnPos, spawnRot, actionFx);
            ItemPool.Spawn(projectileEvent);
        }

        private Entity GetProjectile(ProjectileConfig data) {
            if (_poolDict.TryGetValue(data.ID, out var stack)) {
                if (stack.UsedCount > 0) {
                    var pooled = stack.Pop();
                    if (pooled != null) {
                        pooled.Pooled = false;
                        return pooled;
                    }
                }
            }
            else {
                stack = new ManagedArray<Entity>(_defaultPool);
                _poolDict.Add(data.ID, stack);
            }
            var entity = GetDefaultEntity(data.ID);
            //var prefab = data.GetValue<string>(DatabaseFields.Model);
            entity.Add(new TypeId(data.ID));
            switch (data.Type) {
                default:
                case ProjectileType.Simple:
                    break;
                case ProjectileType.SpriteAnimation:
                    entity.Add(new CollisionCheckForward(data.CollisionDistance));
                    break;
                case ProjectileType.VolumeLaser:
                    entity.Add(new CollisionCheckForward(data.CollisionDistance));
                    break;
            }
            switch (data.Movement) {
                case ProjectileMovement.Forward:
                    entity.Add(new ForwardMover());
                    break;
                case ProjectileMovement. Arc:
                    entity.Add(new ArcMover());
                    break;
            }
            entity.Get<DespawnTimer>().Length = data.Timeout;
            entity.Get<MoveSpeed>().Speed = data.Speed;
            entity.Get<RotationSpeed>().Speed = data.Rotation;
            if (data.ActionFx != null) {
                entity.Add(new ActionFxComponent(data.ActionFx));
                if (data.ActionFx.TryGetColor(out var actionColor)) {
                    entity.Add(new HitParticlesComponent(actionColor));
                }  
            }
            if (data.TrailAmount > 0) {
                entity.Add(new ParticleTrailComponent(data.TrailAmount, data.TrailFrequency, data.TrailColor, ParticleGravityStatus.Default));
            }
            return entity;
        }

        private Entity GetDefaultEntity(string name) {
            var entity = Entity.New(name);
            entity.Factory = this;
            //if it has a label component it'll get picked up by center target
            //entity.Add(new LabelComponent(name));
            entity.Add(new RenderingComponent(null));
            entity.Add(new DespawnTimer(_defaultTimeout.Value, false));
            entity.Add(new DespawnOnCollision());
            entity.Add(new MoveSpeed(_defaultSpeed.Value));
            entity.Add(new RotationSpeed(_defaultRotation.Value));
            entity.Add(new RigidbodyComponent(null));
            entity.Add(new MoveTarget());
            entity.Add(new ProjectileComponent(name));
            return entity;
        }

        public sealed class ProjectileTemplate : BaseTemplate {
            private CachedComponent<ProjectileComponent> _projectile = new CachedComponent<ProjectileComponent>();
            private CachedComponent<MoveTarget> _moveTarget = new CachedComponent<MoveTarget>();
            private CachedComponent<RigidbodyComponent> _rb = new CachedComponent<RigidbodyComponent>();
            private CachedComponent<MoveSpeed> _moveSpeed = new CachedComponent<MoveSpeed>();
            private CachedComponent<RotationSpeed> _rotationSpeed = new CachedComponent<RotationSpeed>();
            private CachedComponent<RenderingComponent> _rendering = new CachedComponent<RenderingComponent>();
            private CachedComponent<ActionFxComponent> _actionFx = new CachedComponent<ActionFxComponent>();
            private CachedComponent<CollisionCheckForward> _checkForward = new CachedComponent<CollisionCheckForward>();

            public ProjectileComponent ProjectileComponent { get => _projectile; }
            public MoveTarget MoveTarget { get => _moveTarget; }
            public RigidbodyComponent Rb { get => _rb; }
            public MoveSpeed MoveSpeed { get => _moveSpeed; }
            public RotationSpeed RotationSpeed { get => _rotationSpeed; }
            public RenderingComponent Rendering { get => _rendering; }
            public ActionFxComponent ActionFx { get => _actionFx; }
            public CollisionCheckForward CollisionCheckForward { get => _checkForward; }
            
            public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
                _projectile, _moveTarget, _rotationSpeed, _rb, _moveSpeed, _rendering, _actionFx, _checkForward
            };

            public override System.Type[] GetTypes() {
                return new System.Type[] {
                    typeof(ProjectileComponent),
                    typeof(MoveTarget),
                    typeof(RigidbodyComponent),
                    typeof(RotationSpeed),
                    typeof(MoveSpeed),
                    typeof(RenderingComponent),
                };
            }
        }
    }

    public enum ProjectileType {
        Simple,
        SpriteAnimation,
        VolumeLaser
    }

    public struct ProjectileSpawned : IEntityMessage {
        public ProjectileConfig Config;
        public Entity Entity;

        public ProjectileSpawned(ProjectileConfig config, Entity entity) {
            Config = config;
            Entity = entity;
        }
    }

    public struct ProjectileDespawned : IEntityMessage {
        public Entity Entity;

        public ProjectileDespawned(Entity entity) {
            Entity = entity;
        }
    }
}
