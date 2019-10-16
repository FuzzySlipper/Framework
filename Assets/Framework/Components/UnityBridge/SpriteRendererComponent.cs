using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine.Rendering;

namespace PixelComrades {
    [System.Serializable]
    public sealed class SpriteRendererComponent : IComponent, IRenderingComponent, IDisposable {
        private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");
        private static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
        
        private CachedUnityComponent<SpriteRenderer> _renderer;
        private CachedTransform _baseTr;
        private Sprite _sprite;
        public bool IsDirty { get; private set; }
        private SpriteRenderer Value { get => _renderer; }
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

        public void SetSprite(Sprite sprite) {
            IsDirty = true;
            _sprite = sprite;
        }

        public void UpdateSprite() {
            IsDirty = false;
            Value.sprite = _sprite;
        }

        public void Flip(bool flipped) {
            Value.flipX = flipped;
        }

        public void SetTextures(Texture2D normalMap, Texture2D emissive) {
            MaterialBlocks[0].SetTexture(BumpMap, normalMap);
            MaterialBlocks[0].SetTexture(EmissionMap, emissive);
            if (emissive != null) {
                Value.material.EnableKeyword("_EMISSION");
            }
            else {
                Value.material.DisableKeyword("_EMISSION");
            }
            Value.SetPropertyBlock(MaterialBlocks[0]);
        }

        public Vector3 GetEventPosition(AnimationFrame frame) {
            var sprite = Value.sprite;
            var pixelsPerUnit = sprite.pixelsPerUnit;
            var width = (sprite.rect.width / 2) / pixelsPerUnit;
            var height = (sprite.rect.height / 2) / pixelsPerUnit;
            //return renderer.transform.position + new Vector3(frame.EventPosition.x * width, height + (frame.EventPosition.y * height), 0);
            return SpriteTr.TransformPoint(
                new Vector3(frame.EventPosition.x * width, height + (frame.EventPosition.y * height), 0));
            //return renderer.bounds.center + new Vector3(frame.EventPosition.x * -renderer.bounds.extents.x, frame.EventPosition.y * -renderer.bounds.extents.y);
        }

        public SpriteRendererComponent(SpriteRenderer renderer, Transform baseTr) {
            _renderer = new CachedUnityComponent<SpriteRenderer>(renderer);
            _baseTr = new CachedTransform(baseTr);
            IsDirty = false;
            Setup();
        }

        private void Setup() {
            MaterialBlocks = new[] { new MaterialPropertyBlock()};
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
