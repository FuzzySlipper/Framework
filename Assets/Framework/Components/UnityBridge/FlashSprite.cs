using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FlashSprite : IComponent {
        public int Owner { get; set; }

        [SerializeField] private string _shaderColor = "_Color";
        [SerializeField] private string _shaderEmissive = "_EmissionColor";
        [SerializeField] private Renderer _spriteRender = null;
        [SerializeField] private float _dmgMaxScale = 1.05f;
        [SerializeField] private TweenFloat _scaleDmgTween = null;

        private bool _animatingColor = false;

        public void Animate() {
            if (!_animatingColor && _spriteRender != null) {
                TimeManager.Start(DamageTween());
            }
        }

        private IEnumerator DamageTween() {
            _animatingColor = true;
            var modelComponent = this.GetEntity().Get<ModelComponent>();
            var blocks = modelComponent.GetMatBlocks;
            if (blocks == null) {
                yield break;
            }
            var renderers = modelComponent.GetRenderers;
            if (renderers == null) {
                yield break;
            }
            Color[] colors = new Color[blocks.Length];
            Color[] emissive = new Color[blocks.Length];
            for (int i = 0; i < blocks.Length; i++) {
                colors[i] = renderers[i].sharedMaterial.GetColor(_shaderColor);
                emissive[i] = renderers[i].sharedMaterial.GetColor(_shaderEmissive);
                blocks[i].SetColor(_shaderColor, Color.red);
                blocks[i].SetColor(_shaderEmissive, Color.red);
            }
            modelComponent.ApplyMaterialBlocks(blocks);
            _scaleDmgTween.Restart(1, _dmgMaxScale);
            var scale = _spriteRender.transform.localScale;
            while (_scaleDmgTween.Active) {
                _spriteRender.transform.localScale = new Vector3(scale.x * _scaleDmgTween.Get(),
                    scale.y, scale.z);
                yield return null;
            }
            _scaleDmgTween.Restart(_dmgMaxScale,1);
            while (_scaleDmgTween.Active) {
                _spriteRender.transform.localScale = new Vector3(scale.x * _scaleDmgTween.Get(),
                    scale.y, scale.z);
                for (int i = 0; i < blocks.Length; i++) {
                    blocks[i].SetColor(_shaderColor, Color.Lerp(Color.red, colors[i], _scaleDmgTween.Get()));
                    blocks[i].SetColor(_shaderEmissive, Color.Lerp(Color.red, emissive[i], _scaleDmgTween.Get()));
                }
                modelComponent.ApplyMaterialBlocks(blocks);
                yield return null;
            }
            _animatingColor = false;
        }
    }
}
