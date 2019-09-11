using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;

namespace PixelComrades {
    public class ProjectileSystem : SystemBase, IEntityPool {
        
        private static GameOptions.CachedFloat _defaultTimeout = new GameOptions.CachedFloat("ProjectileTimeout");
        private static GameOptions.CachedFloat _defaultSpeed = new GameOptions.CachedFloat("ProjectileSpeed");
        private static GameOptions.CachedFloat _defaultRotation = new GameOptions.CachedFloat("ProjectileRotation");
        private static GameOptions.CachedInt _defaultPool = new GameOptions.CachedInt("ProjectilePool");
        private static Dictionary<string, ProjectileTemplate> _templates = new Dictionary<string, ProjectileTemplate>();
        private Dictionary<string, ManagedArray<Entity>> _poolDict = new Dictionary<string, ManagedArray<Entity>>();

        private static void Init() {
            GameData.AddInit(Init);
            foreach (var loadedDataEntry in GameData.GetSheet("ActionSpawn")) {
                var data = loadedDataEntry.Value;
                _templates.AddOrUpdate(data.ID, new ProjectileTemplate(data));
            }
        }

        public ManagedArray<Entity> GetPool(string typeID) {
            return _poolDict.TryGetValue(typeID, out var stack) ? stack : null;
        }

        public void Store(Entity entity) {
            UnityToEntityBridge.Unregister(entity);
            entity.Get<ModelComponent>().Model = null;
            entity.Remove(typeof(SpriteAnimationComponent));
            entity.Tags.Clear();
            entity.Post(new ProjectileDespawned(entity));
            if (_poolDict.TryGetValue(entity.Get<TypeId>().Id, out var stack)) {
                if (!stack.IsFull) {
                    entity.Pooled = true;
                    entity.ClearParent();
                    entity.Stats.ClearMods();
                    stack.Add(entity);
                    return;
                }
            }
            entity.Destroy();
        }

        public Entity SpawnProjectile(Entity owner, string id, ActionEvent msg, List<IActionImpact> impacts) {
            var animData = owner.Find<AnimatorData>();
            var spawnPos = animData?.Animator?.GetEventPosition ?? (owner.Tr != null ? owner.Tr.position : Vector3.zero);
            var spawnRot = animData?.Animator?.GetEventRotation ?? (owner.Tr != null ? owner.Tr.rotation : Quaternion.identity);
            return SpawnProjectile(owner, id, msg.Target, spawnPos, spawnRot, impacts);
        }

        public Entity SpawnProjectile(Entity owner, string id, Vector3 target, Vector3 spawnPos, Quaternion spawnRot, List<IActionImpact> impacts) {
            if (_templates.Count == 0) {
                Init();
            }
            if (!_templates.TryGetValue(id, out var template)) {
#if DEBUG
                DebugLog.Add("Couldn't find project template " + id, false);
#endif
                return null;
            }
            var entity = GetProjectile(template);
            entity.Get<DespawnTimer>().StartTimer();
            entity.Get<MoveTarget>().SetMoveTarget(target);
            entity.Get<ActionImpacts>().Impacts = impacts;
            if (template.ActionFx != null) {
                entity.Get<ActionFxComponent>().ChangeFx(template.ActionFx);
            }
            var prefab = ItemPool.Spawn(UnityDirs.ActionSpawn, template.Type, spawnPos, spawnRot);
            if (prefab == null) {
                return entity;
            }
            var spawn = prefab.GetComponent<IProjectile>();
            entity.Tr = spawn.Tr;
            switch (template.Type) {
                default:
                case "Simple":
                    break;
                case "SpriteAnimation":
                    spawn.SetColor(template.MainColor, Color.white * template.GlowPower);
                    if (template.Animation != null) {
                        var spriteRenderer = spawn.Renderers[0] as SpriteRenderer;
                        entity.Add(new SpriteAnimationComponent(spriteRenderer, template.Animation, false, template.Billboard));
                    }
                    break;
                case "VolumeLaser":
                    spawn.SetColor(template.MainColor, template.OffsetColor);
                    break;
            }
            spawn.SetSize(template.Size, template.Length);
            switch (template.Movement) {
                case "Forward":
                case "Arc":
                    entity.Tr.LookAt(target, entity.Tr.up);
                    break;
                case "Force":
                    //var force = transform.forward * ForceRange.Lerp(Mathf.Clamp01(charging.ElapsedTime / MaxChargeTime));
                    break;
            }
            if (spawn.Rigidbody != null) {
                entity.Get<RigidbodyComponent>().SetRb(spawn.Rigidbody);
            }
            entity.Tags.Add(EntityTags.Moving);
            entity.Get<ModelComponent>().Model = spawn;
            UnityToEntityBridge.RegisterToEntity(spawn.Tr.gameObject, entity);
            entity.ParentId = owner.Id;
            entity.Post(new ProjectileSpawned(template, entity));
            return entity;
        }

        private Entity GetProjectile(ProjectileTemplate data) {
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
            entity.Get<DespawnTimer>().Time = data.Timeout;
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
            entity.PoolOwner = this;
            //if it has a label component it'll get picked up by center target
            //entity.Add(new LabelComponent(name));
            entity.Add(new ModelComponent(null));
            entity.Add(new DespawnTimer(_defaultTimeout.Value, false, false));
            entity.Add(new DespawnOnCollision());
            entity.Add(new MoveSpeed(_defaultSpeed.Value));
            entity.Add(new RotationSpeed(_defaultRotation.Value));
            entity.Add(new RigidbodyComponent(null));
            entity.Add(new MoveTarget());
            entity.Add(new ActionImpacts(null));
            return entity;
        }

        public class ProjectileTemplate {
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

            public ProjectileTemplate(DataEntry data) {
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
        public ProjectileSystem.ProjectileTemplate Template;
        public Entity Entity;

        public ProjectileSpawned(ProjectileSystem.ProjectileTemplate template, Entity entity) {
            Template = template;
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
