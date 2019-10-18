using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;

namespace PixelComrades {
    public class ProjectileSystem : SystemBase, IEntityFactory {
        
        private static GameOptions.CachedFloat _defaultTimeout = new GameOptions.CachedFloat("ProjectileTimeout");
        private static GameOptions.CachedFloat _defaultSpeed = new GameOptions.CachedFloat("ProjectileSpeed");
        private static GameOptions.CachedFloat _defaultRotation = new GameOptions.CachedFloat("ProjectileRotation");
        private static GameOptions.CachedInt _defaultPool = new GameOptions.CachedInt("ProjectilePool");
        private static Dictionary<string, ProjectileConfig> _configs = new Dictionary<string, ProjectileConfig>();
        private Dictionary<string, ManagedArray<Entity>> _poolDict = new Dictionary<string, ManagedArray<Entity>>();

        public ProjectileSystem() {
            TemplateFilter<ProjectileTemplate>.Setup(ProjectileTemplate.GetTypes());
        }

        private static void Init() {
            GameData.AddInit(Init);
            foreach (var loadedDataEntry in GameData.GetSheet("ActionSpawn")) {
                var data = loadedDataEntry.Value;
                _configs.AddOrUpdate(data.ID, new ProjectileConfig(data));
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

        public Entity SpawnProjectile(Entity owner, string id, Vector3 target, Vector3 spawnPos, Quaternion spawnRot) {
            if (_configs.Count == 0) {
                Init();
            }
            if (!_configs.TryGetValue(id, out var config)) {
#if DEBUG
                DebugLog.Add("Couldn't find project config " + id, false);
#endif
                return null;
            }
            var entity = GetProjectile(config);
            var template = entity.GetTemplate<ProjectileTemplate>();
            template.MoveTarget.SetMoveTarget(target);
            if (config.ActionFx != null) {
                template.ActionFx.ChangeFx(config.ActionFx);
            }
            var prefab = ItemPool.Spawn(UnityDirs.ActionSpawn, config.Type, spawnPos, spawnRot);
            if (prefab == null) {
                return entity;
            }
            var spawn = prefab.GetComponent<IProjectile>();
            entity.Add(new TransformComponent(prefab.Transform));
            switch (config.Type) {
                default:
                case "Simple":
                    break;
                case "SpriteAnimation":
                    spawn.SetColor(config.MainColor, Color.white * config.GlowPower);
                    if (config.Animation != null) {
                        var spriteRenderer = prefab.Renderers[0] as SpriteRenderer;
                        entity.Add(new SpriteAnimationComponent(spriteRenderer, config.Animation, false, config.Billboard));
                    }
                    break;
                case "VolumeLaser":
                    spawn.SetColor(config.MainColor, config.OffsetColor);
                    break;
            }
            spawn.SetSize(config.Size, config.Length);
            switch (config.Movement) {
                case "Forward":
                case "Arc":
                    template.CollisionCheckForward.LastPos = null;
                    prefab.Transform.LookAt(target, prefab.Transform.up);
                    break;
                case "Force":
                    //var force = transform.forward * ForceRange.Lerp(Mathf.Clamp01(charging.ElapsedTime / MaxChargeTime));
                    break;
            }
            if (spawn.Rigidbody != null) {
                template.Rb.SetRb(spawn.Rigidbody);
            }
            entity.Tags.Add(EntityTags.Moving);
            template.Rendering.Set(spawn);
            UnityToEntityBridge.RegisterToEntity(prefab.Transform.gameObject, entity);
            entity.ParentId = owner.Id;
            entity.Post(new ProjectileSpawned(config, entity));
            return entity;
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
                stack = new ManagedArray<Entity>(data.PoolSize);
                _poolDict.Add(data.ID, stack);
            }
            var entity = GetDefaultEntity(data.ID);
            //var prefab = data.GetValue<string>(DatabaseFields.Model);
            entity.Add(new TypeId(data.ID));
            switch (data.Type) {
                default:
                case "Simple":
                    break;
                case "SpriteAnimation":
                    entity.Add(new CollisionCheckForward(data.CollisionDistance));
                    break;
                case "VolumeLaser":
                    entity.Add(new CollisionCheckForward(data.CollisionDistance));
                    break;
            }
            switch (data.Movement) {
                case "Forward":
                    entity.Add(new ForwardMover());
                    break;
                case "Arc":
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
            if (data.Components != null) {
                World.Get<DataFactory>().AddComponentList(entity, data.Data, data.Components);
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

            public static System.Type[] GetTypes() {
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

        public class ProjectileConfig {
            public DataEntry Data;
            public string ID;
            public string Type;
            public string Movement;
            public float Speed;
            public float Rotation;
            public float CollisionDistance;
            public float Size;
            public float Length;
            public float Timeout;
            public string Prefab;
            public ActionFx ActionFx;
            public SpriteAnimation Animation;
            public ImpactRadiusTypes Radius;
            public BillboardMode Billboard;
            public Color MainColor;
            public Color OffsetColor;
            public float GlowPower;
            public int PoolSize;
            public DataList Components;
            public int TrailAmount;
            public float TrailFrequency;
            public Color TrailColor;

            public ProjectileConfig(DataEntry data) {
                Data = data;
                ID = data.ID;
                Type = data.TryGetValue("Type", data.ID);
                Movement = data.TryGetValue("Movement", "Simple");
                Speed = data.TryGetValue("Speed", 0f);
                Rotation = data.TryGetValue("Rotation", 0f);
                CollisionDistance = data.TryGetValue("CollisionDistance", 0.25f);
                Size = data.TryGetValue("Size", 0f);
                Length = data.TryGetValue("Length", 0f);
                Timeout = data.TryGetValue(DatabaseFields.Timeout, _defaultTimeout.Value);
                GlowPower = data.TryGetValue("GlowPower", 0f);
                TrailAmount = data.TryGetValue("TrailAmount", 0);
                TrailFrequency = data.TryGetValue("TrailFrequency", 0f);
                TrailColor = data.TryGetValue("TrailColor", Color.black);
                Radius = ParseUtilities.TryParseEnum(data.GetValue<string>("Radius"), ImpactRadiusTypes.Single);
                Billboard = ParseUtilities.TryParseEnum(data.GetValue<string>("Billboard"), BillboardMode.FaceCamYDiff);
                MainColor = data.TryGetValue("MainColor", Color.white);
                OffsetColor = data.TryGetValue("OffsetColor", Color.white);
                PoolSize = data.TryGetValue("PoolSize", _defaultPool);
                var afx = data.GetValue<string>(DatabaseFields.ActionFx);
                if (!string.IsNullOrEmpty(afx)) {
                    ActionFx = ItemPool.LoadAsset<ActionFx>(UnityDirs.ActionFx, afx);
                }
                var model = data.GetValue<string>(DatabaseFields.Model);
                switch (Type) {
                    default:
                    case "Simple":
                        break;
                    case "SpherePhysics":
                        if (!string.IsNullOrEmpty(model)) {
                            Animation = ItemPool.LoadAsset<SpriteAnimation>(UnityDirs.ActionSpawn, model);
                        }
                        break;
                    case "VolumeLaser":
                        Prefab = model;
                        break;
                }
                Components = data.Get(DatabaseFields.Components) as DataList;
            }
        }
    }

    public struct ProjectileSpawned : IEntityMessage {
        public ProjectileSystem.ProjectileConfig Config;
        public Entity Entity;

        public ProjectileSpawned(ProjectileSystem.ProjectileConfig config, Entity entity) {
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
