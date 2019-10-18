using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public interface IProjectile : IRenderingComponent {
        void SetColor(Color main, Color offset);
        void SetSize(float size, float length);
        Rigidbody Rigidbody { get; }
        Collider Collider { get; }
    }

    public class SpriteProjectile : MonoBehaviour, IOnCreate, IProjectile{

        [SerializeField] private SpriteRenderer _spriteRenderer = null;
        [SerializeField] private SphereCollider _sphereCollider = null;
        [SerializeField] private Rigidbody _rigidbody = null;

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

        public void SetColor(Color main, Color offset) {
            MaterialBlocks[0].SetColor("_TintColor", offset);
            _spriteRenderer.SetPropertyBlock(MaterialBlocks[0]);
            SpriteRenderer.color = main;
        }

        public void SetSize(float size, float length) {
            _sphereCollider.radius = size;
        }
    }
}
