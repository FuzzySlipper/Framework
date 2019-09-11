using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ModelWrapper : MonoBehaviour, IOnCreate, IModelComponent {

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

        public void SetVisible(bool status) {
            _entity.SetVisible(status);
        }
    }
}
