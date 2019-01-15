using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ProjectileSystem : SystemBase, IEntityPool {
        
        private static GameOptions.CachedFloat _defaultTimeout = new GameOptions.CachedFloat("ProjectileTimeout");
        private static GameOptions.CachedFloat _defaultSpeed = new GameOptions.CachedFloat("ProjectileSpeed");
        private static GameOptions.CachedFloat _defaultRotation = new GameOptions.CachedFloat("ProjectileRotation");
        private static GameOptions.CachedInt _defaultPool = new GameOptions.CachedInt("ProjectilePool");

        private Dictionary<string, ManagedArray<Entity>> _poolDict = new Dictionary<string, ManagedArray<Entity>>();

        public ManagedArray<Entity> GetPool(string typeID) {
            return _poolDict.TryGetValue(typeID, out var stack) ? stack : null;
        }

        public void Store(Entity entity) {
            MonoBehaviourToEntity.Unregister(entity);
            entity.Remove<RigidbodyComponent>();
            entity.Get<ModelComponent>().Model = null;
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

        public Entity SpawnProjectile(Entity owner) {
            var spawnComponent = owner.Get<ActionSpawnComponent>();
            var entity = GetProjectile(spawnComponent.Data);
            entity.Get<DespawnTimer>().StartTimer();
            if (string.IsNullOrEmpty(spawnComponent.Prefab)) {
                return entity;
            }
            owner.FindSpawn(out var spawnPos, out var spawnRot);
            var spawn = ItemPool.Spawn(UnityDirs.Projectiles, spawnComponent.Prefab, spawnPos, spawnRot);
            if (spawn == null) {
                return entity;
            }
            entity.Tr = spawn.Transform;
            entity.Add(new RigidbodyComponent(spawn.GetComponent<Rigidbody>()));
            entity.Get<ModelComponent>().Model = spawn.GetComponent<IModelComponent>();
            MonoBehaviourToEntity.RegisterToEntity(spawn.gameObject, entity);
            entity.ParentId = owner.Id;
            return entity;
        }

        private Entity GetProjectile(DataEntry data) {
            if (_poolDict.TryGetValue(data.ID, out var stack)) {
                if (stack.Count > 0) {
                    var pooled = stack.Pop();
                    if (pooled != null) {
                        pooled.Pooled = false;
                        return pooled;
                    }
                }
            }
            else {
                stack = new ManagedArray<Entity>(data.TryGetValue("PoolSize", _defaultPool.Value));
                _poolDict.Add(data.ID, stack);
            }
            var entity = GetDefaultEntity(data.TryGetValue(DatabaseFields.Name, "Spawn"));
            entity.Add(new TypeId(data.ID));
            entity.Get<DespawnTimer>().Time = data.TryGetValue(DatabaseFields.Timeout, _defaultTimeout.Value);
            if (data.TryGetValue(DatabaseFields.Speed, out float speed)) {
                entity.Get<MoveSpeed>().Speed = speed;
            }
            if (data.TryGetValue(DatabaseFields.Rotation, out float rotation)) {
                entity.Get<RotationSpeed>().Speed = rotation;
            }
            World.Get<DataFactory>().AddComponentList(entity, data, data.Get(DatabaseFields.Components) as DataList);
            return entity;
        }

        private Entity GetDefaultEntity(string name) {
            var entity = Entity.New(name);
            entity.Add(new LabelComponent(name));
            entity.Add(new ModelComponent(null));
            entity.Add(new DespawnTimer(_defaultTimeout.Value, false, this, false));
            entity.Add(new DespawnOnCollision(this));
            entity.Add(new MoveSpeed(_defaultSpeed.Value));
            entity.Add(new RotationSpeed(_defaultRotation.Value));
            return entity;
        }
    }
}
