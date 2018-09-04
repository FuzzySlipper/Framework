using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class MaterializeOnSpawn : MonoBehaviour, IPoolEvents, IOnCreate {

        private PrefabEntity _entity;

        public void OnCreate(PrefabEntity entity) {
            _entity = entity;
        }

        public void OnPoolSpawned() {
            for (int i = 0; i < _entity.Renderers.Length; i++) {
                _entity.Renderers[i].Renderer.material.SetFloat("_DissolveAmount", 0);
            }
            TimeManager.Start(FadeInItem());
        }

        public void OnPoolDespawned() {
        }

        private IEnumerator FadeInItem() {
            var tweener = new TweenFloat(0, 1, 0.5f, EasingTypes.SinusoidalInOut, true);
            tweener.Init();
            while (tweener.Active) {
                for (int i = 0; i < _entity.Renderers.Length; i++) {
                    _entity.Renderers[i].Renderer.material.SetFloat("_DissolveAmount", tweener.Get());
                }
                yield return null;
            }
        }
    }
}