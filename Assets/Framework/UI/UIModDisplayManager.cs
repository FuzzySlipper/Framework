using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class UIModDisplayManager : MonoBehaviour, IReceive<ModifiersChanged> {

        [SerializeField] private UIModIcon _prefab = null;
        [SerializeField] private int _modLimit = 5;

        private List<UIModIcon> _active = new List<UIModIcon>();
        private CharacterNode _actor;

        public void SetupActor(CharacterNode actor) {
            if (_actor != null) {
                _actor.Entity.RemoveObserver(this);
                MessageKit.removeObserver(Messages.ModifiersUpdated, UpdateMods);
            }
            _actor = actor;
            if (_actor == null) {
                return;
            }
            _actor.Entity.AddObserver(this);
            MessageKit.addObserver(Messages.ModifiersUpdated, UpdateMods);
        }

        private void ClearList() {
            for (int i = 0; i < _active.Count; i++) {
                ItemPool.Despawn(_active[i].gameObject);
            }
            _active.Clear();
        }

        private void CheckMods() {
            ClearList();
            for (int i = _actor.Modifiers.c.Count - 1; i >= 0; i--) {
                if (_active.Count >= _modLimit) {
                    break;
                }
                var mod = _actor.Modifiers.c[i];
                if (mod.Icon == null || ContainsMod(mod)) {
                    continue;
                }
                var modWatch = ItemPool.SpawnUIPrefab<UIModIcon>(_prefab.gameObject, transform);
                _active.Add(modWatch);
                modWatch.Assign(mod, _actor);
            }
        }

        private bool ContainsMod(IEntityModifier mod) {
            for (int i = 0; i < _active.Count; i++) {
                if (_active[i].Mod == mod) {
                    return true;
                }
            }
            return false;
        }

        private void UpdateMods() {
            for (int i = 0; i < _active.Count; i++) {
                _active[i].UpdateCoolDown();
            }
        }

        public void Handle(ModifiersChanged arg) {
            CheckMods();
        }
    }
}
