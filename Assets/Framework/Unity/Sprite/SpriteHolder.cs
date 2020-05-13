using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SpriteHolder : MonoBehaviour, IPoolEvents {

        [SerializeField] private BillboardMode _billboard = BillboardMode.CamFwd;
        [SerializeField] private bool _unscaled = true;
        //[SerializeField] private SpriteRenderer _renderer = null;
        [SerializeField] private Transform _spriteBaseTr = null;
        [SerializeField] private Transform _spriteTr = null;
        [SerializeField] private SpriteFacing _facing = SpriteFacing.EightwayFlipped;
        [SerializeField] private bool _backwards = true;
        [SerializeField] private List<Collider> _ignoreColliders = new List<Collider>();
        [SerializeField] private MeshRenderer _meshRenderer = null;
        [SerializeField] private MeshFilter _meshFilter = null;
        [SerializeField] private bool _isVisible = false;
        
        public BillboardMode Billboard { get => _billboard; }
        public bool Unscaled { get => _unscaled; }
        //public SpriteRenderer Renderer { get => _renderer; }
        public MeshRenderer MeshRenderer { get => _meshRenderer; }
        public MeshFilter MeshFilter { get => _meshFilter; }
        public Transform SpriteBaseTr { get => _spriteBaseTr; }
        public Transform SpriteTr { get => _spriteTr; }
        public SpriteFacing Facing { get => _facing; }
        public bool Backwards { get => _backwards; }
        public List<Collider> IgnoreColliders { get => _ignoreColliders; }
        public bool IsVisible { get => _isVisible; }

        private void OnBecameVisible() {
            _isVisible = true;
        }

        private void OnBecameInvisible() {
            _isVisible = false;
        }

        public void OnPoolSpawned() {
            _isVisible = false;
        }

        public void OnPoolDespawned() {
            _isVisible = false;
        }
    }
}
