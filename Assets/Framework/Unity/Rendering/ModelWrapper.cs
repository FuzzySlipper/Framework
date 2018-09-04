using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ModelWrapper : MonoBehaviour, IOnCreate, IModelComponent {

        private PrefabEntity _entity;

        public void OnCreate(PrefabEntity entity) {
            _entity = entity;
        }

        private MaterialPropertyBlock[] _blocks;
        public MaterialPropertyBlock[] GetMatBlocks {
            get {
                if (_blocks != null) {
                    return _blocks;
                }
                _blocks = new MaterialPropertyBlock[_renderers.Length];
                for (int i = 0; i < _blocks.Length; i++) {
                    _blocks[i] = new MaterialPropertyBlock();
                    _renderers[i].GetPropertyBlock(_blocks[i]);
                }
                return _blocks;
            }
        }
        private Renderer[] _renderers;
        public Renderer[] GetRenderers {
            get {
                if (_renderers == null) {
                    List<Renderer> renders = new List<Renderer>();
                    for (int i = 0; i < _entity.Renderers.Length; i++) {
                        if (_entity.Renderers[i].Renderer == null || _entity.Renderers[i].Renderer.transform.CompareTag(StringConst.TagConvertedTexture) ||
                            _entity.Renderers[i].Renderer is SpriteRenderer) {
                            continue;
                        }
                        renders.Add(_entity.Renderers[i].Renderer);
                    }
                    _renderers = renders.ToArray();
                }
                return _renderers;
            }
        }

        public void ApplyMaterialBlocks(MaterialPropertyBlock[] matBlocks) {
            for (int i = 0; i < _renderers.Length; i++) {
                _renderers[i].SetPropertyBlock(matBlocks[i]);
            }
        }

        public void SetVisible(bool status) {
            _entity.SetVisible(status);
        }
    }
}
