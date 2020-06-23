using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ParticleRectSwitcher : MonoBehaviour, IActionPrefab {

        [SerializeField] private Renderer _matSource = null;
        [SerializeField] private SimpleAnimation _animationUi = null;

        public void OnActionSpawn(ActionEvent state) {
            var focus = state.Target;
            if (focus == null) {
                return;
            }
            var rect = World.Get<CharacterRectSystem>().GetEntityRect(focus.Entity);
            //if (focus == null || !focus.HasComponent<PlayerComponent>()) {
            //    return;
            //}
            if (rect == null) {
                return;
            }
            if (_matSource == null) {
                _matSource = GetComponent<Renderer>();
            }
            ItemPool.Spawn(
                LazyDb.Main.ParticleUIHolder,
                entity => {
                    var altParticle = entity.GetComponent<UIAnimation>();
                    if (altParticle == null) {
                        Debug.LogErrorFormat(
                            "{0} tried to convert to UI animation for {1} targeting {2} at state {3} spawn {4}",
                            name,
                            state.State,
                            focus.Get<LabelComponent>(),
                            state,
                            entity.name);
                        ItemPool.Despawn(entity);
                        return;
                    }
                    altParticle.transform.SetParent(rect.RectTr);
                    altParticle.Play(_animationUi, _matSource != null ? _matSource.sharedMaterial : null);
                    ItemPool.Despawn(gameObject);
                });
        }
    }
}