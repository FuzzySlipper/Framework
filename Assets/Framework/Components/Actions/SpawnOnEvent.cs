using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SpawnOnEvent : IComponent, IReceive<ActionStateEvent> {
        public int Owner { get; set; }

        public ActionStateEvents StartEvent { get; }
        public ActionStateEvents EndEvent { get; }
        public GameObject Prefab { get; }
        public bool Parent { get; }

        private PrefabEntity _activeGameObject;

        public SpawnOnEvent(ActionStateEvents startEvent, ActionStateEvents endEvent, bool parent, GameObject prefab) {
            StartEvent = startEvent;
            EndEvent = endEvent;
            Prefab = prefab;
            Parent = parent;
        }

        public void Handle(ActionStateEvent arg) {
            if (arg.State == EndEvent && _activeGameObject != null) {
                ItemPool.Despawn(_activeGameObject);
            }
            else if (arg.State == StartEvent && Prefab != null) {
                var animTr = this.Get<AnimTr>().Tr;
                if (Parent && animTr != null) {
                    _activeGameObject = ItemPool.Spawn(Prefab);
                    if (_activeGameObject != null) {
                        _activeGameObject.transform.SetParentResetPos(animTr);
                    }
                }
                else {
                    this.GetEntity().FindSpawn(out var spawnPos, out var spawnRot);
                    _activeGameObject = ItemPool.Spawn(Prefab, spawnPos, spawnRot);
                }
                if (EndEvent == ActionStateEvents.None) {
                    _activeGameObject = null;
                }
            }
        }
    }
}
