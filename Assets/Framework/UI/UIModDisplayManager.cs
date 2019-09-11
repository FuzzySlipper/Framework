using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class UIModDisplayManager : MonoBehaviour, IReceive<ModifiersChanged> {

        [SerializeField] private UIModIcon _prefab = null;
        [SerializeField] private int _modLimit = 5;

        private List<UIModIcon> _active = new List<UIModIcon>();
        private CharacterNode _actor;
        private List<ModEntry> _mods = new List<ModEntry>();

        public void SetupActor(CharacterNode actor) {
            if (_actor != null) {
                _actor.Entity.RemoveObserver(this);
            }
            _actor = actor;
            if (_actor == null) {
                return;
            }
            _actor.Entity.AddObserver(this);
        }

        private void ClearList() {
            for (int i = 0; i < _active.Count; i++) {
                ItemPool.Despawn(_active[i].gameObject);
            }
            _active.Clear();
        }

        private void CheckMods() {
            ClearList();
            _mods.Clear();
            World.Get<ModifierSystem>().FillModList(_mods, _actor.Entity.Id);
            for (int i = _mods.Count - 1; i >= 0; i--) {
                if (_active.Count >= _modLimit) {
                    break;
                }
                var mod = _mods[i];
                if (mod.Icon == null) {
                    continue;
                }
                var modWatch = ItemPool.SpawnUIPrefab<UIModIcon>(_prefab.gameObject, transform);
                _active.Add(modWatch);
                modWatch.Assign(mod);
            }
        }

        public void Handle(ModifiersChanged arg) {
            CheckMods();
        }
    }
}
