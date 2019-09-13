using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class SpawnPrefabOnDeath : IComponent, IReceive<DeathEvent> {

        private string _prefab;
        private IntRange _countRange;
        private float _radius;

        public SpawnPrefabOnDeath(string prefab, IntRange count, float radius) {
            _prefab = prefab;
            _countRange = count;
            _radius = radius;
        }

        public SpawnPrefabOnDeath(SerializationInfo info, StreamingContext context) {
            _prefab = info.GetValue(nameof(_prefab), _prefab);
            _countRange = info.GetValue(nameof(_countRange), _countRange);
            _radius = info.GetValue(nameof(_radius), _radius);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_prefab), _prefab);
            info.AddValue(nameof(_countRange), _countRange);
            info.AddValue(nameof(_radius), _radius);
        }

        public void Handle(DeathEvent arg) {
            var position = arg.Target.Tr.position;
            var count = _countRange.Get();
            if (count <= 0) {
                return;
            }
            for (int i = 0; i < count; i++) {
                var spawnPos = position + Random.insideUnitSphere * (_radius  * 0.5f);
                spawnPos.y = position.y;
                var spawn = ItemPool.Spawn(UnityDirs.Items, _prefab, Vector3.Lerp(spawnPos, spawnPos + Vector3.up, Random.value), Quaternion.identity, true);
                if (spawn == null) {
                    continue;
                }
                var rb = spawn.GetComponent<FakePhysicsObject>();
                if (rb == null) {
                    continue;
                }
                WhileLoopLimiter.ResetInstance();
                while (WhileLoopLimiter.InstanceAdvance()) {
                    var throwPos = spawnPos + (Random.insideUnitSphere * _radius);
                    throwPos.y = position.y;
                    if (!Physics.Linecast(spawn.transform.position, throwPos, LayerMasks.Environment)) {
                        if (Physics.Raycast(throwPos, Vector3.down, out var hit, 5f, LayerMasks.Floor)) {
                            throwPos = hit.point;
                        }
                        rb.Throw(throwPos);
                        break;
                    }
                }
            }
        }
    }

    [System.Serializable]
	public sealed class SpawnSimplePrefabOnDeath : IComponent, IReceive<DeathEvent> {

        private PrefabEntity _prefab;
        private int _count;

        public SpawnSimplePrefabOnDeath(PrefabEntity prefab, int count) {
            _prefab = prefab;
            _count = count;
        }

        public SpawnSimplePrefabOnDeath(SerializationInfo info, StreamingContext context) {
            _prefab = ItemPool.GetReferencePrefab( info.GetValue(nameof(_prefab), _prefab.PrefabId));
            _count = info.GetValue(nameof(_count), _count);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_prefab), _prefab.PrefabId);
            info.AddValue(nameof(_count), _count);
        }
        
        public void Handle(DeathEvent arg) {
            for (int i = 0; i < _count; i++) {
                ItemPool.Spawn(_prefab, arg.Target.Tr.position, arg.Target.Tr.rotation, true);
            }
        }
    }

    [Priority(Priority.Lowest)]
    [System.Serializable]
	public sealed class DisableTrOnDeath : IComponent, IReceive<DeathEvent> {

        public void Handle(DeathEvent arg) {
            arg.Target.Tr.gameObject.SetActive(false);
        }

        public DisableTrOnDeath() {}

        public DisableTrOnDeath(SerializationInfo info, StreamingContext context) {
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
        }
    }
}
