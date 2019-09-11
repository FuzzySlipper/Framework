using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SpawnPrefabOnDeath : ComponentBase, IReceive<DeathEvent> {

        private string _prefab;
        private IntRange _countRange;
        private float _radius;

        public SpawnPrefabOnDeath(string prefab, IntRange count, float radius) {
            _prefab = prefab;
            _countRange = count;
            _radius = radius;
        }

        public void Handle(DeathEvent arg) {
            var position = Entity.Tr.position;
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

    public class SpawnSimplePrefabOnDeath : ComponentBase, IReceive<DeathEvent> {

        private GameObject _prefab;
        private int _count;

        public SpawnSimplePrefabOnDeath(GameObject prefab, int count) {
            _prefab = prefab;
            _count = count;
        }

        public void Handle(DeathEvent arg) {
            for (int i = 0; i < _count; i++) {
                ItemPool.Spawn(_prefab, Entity.Tr.position, Entity.Tr.rotation, true);
            }
        }
    }

    [Priority(Priority.Lowest)]
    public class DisableTrOnDeath : ComponentBase, IReceive<DeathEvent> {

        public void Handle(DeathEvent arg) {
            Entity.Tr.gameObject.SetActive(false);
        }
    }
}
