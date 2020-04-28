using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public interface IProjectile : IRenderingComponent {
        void SetConfig(ProjectileConfig config, Entity entity);
        Rigidbody Rigidbody { get; }
    }

    public class SpriteProjectile : MonoBehaviour, IOnCreate, IProjectile{

        [SerializeField] private SpriteRenderer _spriteRenderer = null;
        [SerializeField] private SphereCollider _sphereCollider = null;
        [SerializeField] private Rigidbody _rigidbody = null;
        [SerializeField] private BillboardMode _billboard = BillboardMode.FaceCamYDiff;
        public SpriteRenderer SpriteRenderer { get => _spriteRenderer; }
        public Collider Collider { get => _sphereCollider; }
        public Rigidbody Rigidbody { get => _rigidbody; }
        public Transform Tr { get; private set; }
        public MaterialPropertyBlock[] MaterialBlocks { get; private set; }
        public Renderer[] Renderers { get; private set; }

        public void ApplyMaterialBlocks(MaterialPropertyBlock[] matBlocks) {
            if (matBlocks == null || matBlocks.Length < 1) {
                return;
            }
            _spriteRenderer.SetPropertyBlock(matBlocks[0]);
        }

        public void SetFloat(int id, float value) {
            for (int i = 0; i < MaterialBlocks.Length; i++) {
                MaterialBlocks[i].SetFloat(id, value);
            }
        }

        public void ApplyMaterialBlock() {
            for (int i = 0; i < Renderers.Length; i++) {
                Renderers[i].SetPropertyBlock(MaterialBlocks[i]);
            }
        }

        public void SetRendering(RenderingMode status) {
            _spriteRenderer.SetMode(status);
        }

        public void OnCreate(PrefabEntity entity) {
            Tr = transform;
            MaterialBlocks = new MaterialPropertyBlock[1];
            MaterialBlocks[0] = new MaterialPropertyBlock();
            _spriteRenderer.GetPropertyBlock(MaterialBlocks[0]);
            Renderers = new Renderer[1];
            Renderers[0] = _spriteRenderer;
        }

        public void SetConfig(ProjectileConfig config, Entity entity) {
            MaterialBlocks[0].SetColor("_TintColor", Color.white * config.GlowPower);
            _spriteRenderer.SetPropertyBlock(MaterialBlocks[0]);
            SpriteRenderer.color = config.MainColor;
            if (config.Animation != null) {
                entity.Add(new SpriteAnimationComponent(_spriteRenderer, config.Animation.LoadedAsset, false, _billboard));
            }
            if (_sphereCollider == null) {
                return;
            }
            _sphereCollider.radius = config.Size;
        }

    }
}
