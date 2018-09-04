using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ParticleRectSwitcher : MonoBehaviour, IActionPrefab {

        [SerializeField] private Renderer _matSource = null;
        [SerializeField] private SimpleAnimation _animationUi = null;

        public void OnActionSpawn(ActionStateEvent state) {
            var focus = state.GetFocus();
            if (focus == null || !focus.HasComponent<PlayerComponent>()) {
                return;
            }
            if (_matSource == null) {
                _matSource = GetComponent<Renderer>();
            }
            var spawn = ItemPool.Spawn(StringConst.ParticleUI, false, false);
            var altParticle = spawn.GetComponent<UIAnimation>();
            if (altParticle == null) {
                Debug.LogErrorFormat("{0} tried to conver to UI animation for {1} targetting {2} at state {3} spawn {4}", name, state.State, focus.Get<LabelComponent>(), state, spawn.name);
                ItemPool.Despawn(spawn);
                return;
            }
            altParticle.transform.SetParent(World.Get<CharacterRectSystem>().GetEntityRect(focus).RectTr);
            altParticle.Play(_animationUi, _matSource != null ? _matSource.sharedMaterial : null);
            ItemPool.Despawn(gameObject);
        }

    }
}
