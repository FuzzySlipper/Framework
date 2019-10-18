using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class RenderingWrapper : MonoBehaviour, IOnCreate, IRenderingComponent {

        private PrefabEntity _entity;

        public void OnCreate(PrefabEntity entity) {
            _entity = entity;
            _renderers = entity.Renderers;
            _blocks = entity.GatherMatBlocks();
        }

        private MaterialPropertyBlock[] _blocks;
        public virtual MaterialPropertyBlock[] MaterialBlocks {
            get {
                return _blocks;
            }
        }
        private Renderer[] _renderers;
        public virtual Renderer[] Renderers {
            get {
                return _renderers;
            }
        }

        public virtual Transform Tr { get { return transform; } }

        public void ApplyMaterialBlocks(MaterialPropertyBlock[] matBlocks) {
            for (int i = 0; i < _renderers.Length; i++) {
                _renderers[i].SetPropertyBlock(matBlocks[i]);
            }
        }

        public void SetFloat(int id, float value) {
            for (int i = 0; i < _blocks.Length; i++) {
                _blocks[i].SetFloat(id, value);
            }
        }

        public void ApplyMaterialBlock() {
            for (int i = 0; i < _renderers.Length; i++) {
                _renderers[i].SetPropertyBlock(_blocks[i]);
            }
        }

        public void SetRendering(RenderingMode status) {
            _entity.SetVisible(status != RenderingMode.None);
        }
    }
}
