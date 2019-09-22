using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine.Rendering;

namespace PixelComrades {
    [System.Serializable]
    public sealed class SpriteRendererComponent : IComponent, IRenderingComponent, IDisposable {
        
        private CachedUnityComponent<SpriteRenderer> _renderer;
        private CachedTransform _baseTr;
        public SpriteRenderer Value { get => _renderer; }
        public Transform BaseTr { get => _baseTr; }
        public Transform SpriteTr { get; private set; }
        public MaterialPropertyBlock[] MaterialBlocks { get; private set; }
        public void ApplyMaterialBlocks(MaterialPropertyBlock[] matBlocks) {
            if (matBlocks == null || matBlocks.Length == 0) {
                return;
            }
            Value.SetPropertyBlock(matBlocks[0]);
        }

        public void SetRendering(RenderingMode status) {
            Value.SetMode(status);
        }

        public SpriteRendererComponent(SpriteRenderer renderer, Transform baseTr) {
            _renderer = new CachedUnityComponent<SpriteRenderer>(renderer);
            _baseTr = new CachedTransform(baseTr);
            Setup();
        }

        private void Setup() {
            MaterialBlocks = new MaterialPropertyBlock[1] { new MaterialPropertyBlock()};
            Value.GetPropertyBlock(MaterialBlocks[0]);
            SpriteTr = Value.transform;
        }
        
        public SpriteRendererComponent(SerializationInfo info, StreamingContext context) {
            _renderer = info.GetValue(nameof(_renderer), _renderer);
            _baseTr = info.GetValue(nameof(_baseTr), _baseTr);
            Setup();
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_renderer), _renderer);
            info.AddValue(nameof(_baseTr), _baseTr);
        }

        public void Dispose() {
            _renderer?.Dispose();
            _renderer = null;
            _baseTr?.Dispose();
            _baseTr = null;
        }
    }
}
