using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine.Rendering;

namespace PixelComrades {
    public interface ISpriteRendererComponent {
        void SetSprite(Sprite sprite, Texture2D normal, Texture2D emissive, SavedSpriteCollider spriteCollider, int instanceIdx,
            bool flip);

        void Flip(bool isFlipped);
        Vector3 GetEventPosition(Vector2 framePos, int instancedIndex);
        Quaternion GetRotation();
    }

    public interface ISpriteRendererComponent {
        void SetSprite(Sprite sprite, Texture2D normal, Texture2D emissive, SavedSpriteCollider spriteCollider, int instanceIdx,
            bool flip);

        void Flip(bool isFlipped);
        Vector3 GetEventPosition(Vector2 framePos, int instancedIndex);
        Quaternion GetRotation();
    }
    
    [System.Serializable]
    public sealed class SpriteRendererComponent : IComponent, IRenderingComponent, ISpriteRendererComponent, IDisposable {
        private CachedTransform _baseTr;
        private CachedTransform _spriteTr;
        private CachedUnityComponent<MeshFilter> _filter;
        private CachedUnityComponent<Renderer> _renderer;
        private CachedUnityComponent<SpriteRenderer> _spriteRenderer;

        public MaterialPropertyBlock MatBlock;
        public Vector4 Uv;
        public List<Vector3> MeshVertices;
        public SavedSpriteCollider SavedCollider;
        public bool IsDirty { get; private set; }
        public Sprite Sprite { get; private set; }
        public Texture2D Normal { get; private set; }
        public Texture2D Emissive { get; private set; }
        public bool FlipX { get; private set; }
        public Transform BaseTr { get => _baseTr; }
        public Transform SpriteTr { get => _spriteTr; }
        public bool IsMeshRenderer { get; private set; }
        
        public void ApplyMaterialBlock() {
            if (_renderer.Value != null) {
                _renderer.Value.SetPropertyBlock(MatBlock);
            }
        }

        public void SetRendering(RenderingMode status) {
            if (_renderer != null) {
                _renderer.Value.SetMode(status);
            }
        }

        public void SetFloat(int id, float value) {
            if (MatBlock != null) {
                MatBlock.SetFloat(id, value);
            }
        }

        public void SetVector(int id, Vector3 value) {
            if (MatBlock != null) {
                MatBlock.SetVector(id, value);
            }
        }

        public void UpdateColor(Color color) {
            MatBlock.SetVector(SpriteRenderingSystem.ShaderPropertyColor, color);
            ApplyMaterialBlock();
        }

        public void SetSprite(Sprite sprite, Texture2D normal, Texture2D emissive, SavedSpriteCollider spriteCollider) {
            IsDirty = true;
            Sprite = sprite;
            Normal = normal;
            Emissive = emissive;
            SavedCollider = spriteCollider;
            SetRendering(RenderingMode.Normal);
        }

        public void SetSprite(Sprite sprite, Texture2D normal, Texture2D emissive, SavedSpriteCollider spriteCollider, int instanceIdx, 
        bool flip) {
            SetSprite(sprite,normal, emissive, spriteCollider);
        }

        public Quaternion GetRotation() {
            return BaseTr.rotation;
        }

        public void UpdatedSprite() {
            IsDirty = false;
            if (!IsMeshRenderer) {
                _spriteRenderer.Value.sprite = Sprite;
            }
            MatBlock.SetTexture(SpriteRenderingSystem.ShaderPropertyTexture, Sprite.texture);
            MatBlock.SetTexture(SpriteRenderingSystem.ShaderPropertyNormal, Normal);
            MatBlock.SetTexture(SpriteRenderingSystem.ShaderPropertyEmissive, Emissive);

            //Value.sprite = _sprite;
        }

        public void Flip(bool flipped) {
            FlipX = flipped;
        }

        public Vector3 GetEventPosition(AnimationFrame frame) {
            return GetEventPosition(frame.EventPosition, 0);
        }

        public Vector3 GetEventPosition(Vector2 frame, int instancedIndex) {
            var size = new Vector2(
                Sprite.rect.width / Sprite.pixelsPerUnit,
                Sprite.rect.height / Sprite.pixelsPerUnit);
            return SpriteTr.TransformPoint(
                Mathf.Lerp(-(size.x * 0.5f), (size.x * 0.5f), frame.x), size.y * frame.y, 0);
        }

        public SpriteRendererComponent(MeshRenderer renderer, MeshFilter filter, Transform baseTr) {
            Setup(renderer,filter, baseTr);
        }

        public SpriteRendererComponent(SpriteHolder spriteHolder) {
            if (spriteHolder.SpriteRenderer == null) {
                Setup(spriteHolder.MeshRenderer, spriteHolder.MeshFilter, spriteHolder.SpriteBaseTr);
            }
            else {
                Setup(spriteHolder.SpriteRenderer, spriteHolder.SpriteBaseTr);
            }
        }

        private void Setup(MeshRenderer renderer, MeshFilter filter, Transform baseTr) {
            _renderer = new CachedUnityComponent<Renderer>(renderer);
            filter.sharedMesh = ProceduralMeshUtility.GenerateQuad(Vector2.one, new Vector2(0.5f, 0));
            _filter = new CachedUnityComponent<MeshFilter>(filter);
            MeshVertices = new List<Vector3>();
            filter.sharedMesh.GetVertices(MeshVertices);
            MatBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(MatBlock);
            _spriteTr = new CachedTransform(renderer.transform);
            _baseTr = new CachedTransform(baseTr);
            IsMeshRenderer = true;
            IsDirty = false;
        }

        private void Setup(SpriteRenderer renderer, Transform baseTr) {
            _renderer = new CachedUnityComponent<Renderer>(renderer);
            _spriteRenderer = new CachedUnityComponent<SpriteRenderer>(renderer);
            MatBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(MatBlock);
            _spriteTr = new CachedTransform(renderer.transform);
            _baseTr = new CachedTransform(baseTr);
            IsMeshRenderer = false;
            IsDirty = false;
        }

        public SpriteRendererComponent(SerializationInfo info, StreamingContext context) {
            _spriteTr = info.GetValue(nameof(_spriteTr), _spriteTr);
            _baseTr = info.GetValue(nameof(_baseTr), _baseTr);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_spriteTr), _spriteTr);
            info.AddValue(nameof(_baseTr), _baseTr);
        }

        public void UpdateMesh() {
            if (_filter == null) {
                return;
            }
            _filter.Value.sharedMesh.SetVertices(MeshVertices);
        }

        public void Dispose() {
            _spriteTr?.Dispose();
            _spriteTr = null;
            _baseTr?.Dispose();
            _baseTr = null;
            _spriteRenderer?.Dispose();
            _spriteRenderer = null;
            _filter?.Dispose();
            _filter = null;
        }
    }
}
