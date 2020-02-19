using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine.Rendering;

namespace PixelComrades {

    public struct MaterialValue {
        public int Key { get; }
        public float Float { get; }
        public Vector3? V3 { get; }

        public MaterialValue(int key, float f, Vector3? v3) {
            Key = key;
            Float = f;
            V3 = v3;
        }
    }
    
    [System.Serializable]
    public sealed class SpriteRendererComponent : IComponent, IRenderingComponent, IDisposable {
        private CachedTransform _baseTr;
        private CachedTransform _spriteTr;
        private CachedUnityComponent<MeshFilter> _filter;
        private CachedUnityComponent<MeshRenderer> _meshRenderer;
        
        public MaterialPropertyBlock MatBlock;
        public Vector4 Uv;
        public Queue<MaterialValue> MaterialValues = new Queue<MaterialValue>();
        public List<Vector3> MeshVertices;
        public SavedSpriteCollider SavedCollider;
        public MeshRenderer MeshRenderer { get => _meshRenderer?.Value; }
        public bool IsDirty { get; private set; }
        public Sprite Sprite { get; private set; }
        public Texture2D Normal { get; private set; }
        public Texture2D Emissive { get; private set; }
        public bool FlipX { get; private set; }
        public Transform BaseTr { get => _baseTr; }
        public Transform SpriteTr { get => _spriteTr; }
        
        public void ApplyMaterialBlock() {
            if (MeshRenderer != null) {
                MeshRenderer.SetPropertyBlock(MatBlock);
            }
        }

        public void SetRendering(RenderingMode status) {
            if (MeshRenderer != null) {
                MeshRenderer.SetMode(status);
            }
        }

        public void SetFloat(int id, float value) {
            if (MeshRenderer != null) {
                MatBlock.SetFloat(id, value);
            }
            else {
                MaterialValues.Enqueue(new MaterialValue(id, value, null));
            }
        }

        public void SetVector(int id, Vector3 value) {
            if (MeshRenderer != null) {
                MatBlock.SetVector(id, value);
            }
            else {
                MaterialValues.Enqueue(new MaterialValue(id, -1, value));
            }
        }

        public void SetSprite(Sprite sprite, Texture2D normal, Texture2D emissive, SavedSpriteCollider spriteCollider) {
            IsDirty = true;
            Sprite = sprite;
            Normal = normal;
            Emissive = emissive;
            SavedCollider = spriteCollider;
            SetRendering(RenderingMode.Normal);
        }

        public void UpdatedSprite() {
            IsDirty = false;
            //Value.sprite = _sprite;
        }

        public void Flip(bool flipped) {
            FlipX = flipped;
        }

        public Vector3 GetEventPosition(AnimationFrame frame) {
            var size = new Vector2(Sprite.rect.width / Sprite.pixelsPerUnit,
                Sprite.rect.height / Sprite.pixelsPerUnit);
            return SpriteTr.TransformPoint(
                Mathf.Lerp(-(size.x * 0.5f), (size.x * 0.5f), frame.EventPosition.x), size.y * frame.EventPosition.y, 0);
        }

        public SpriteRendererComponent(MeshRenderer renderer, MeshFilter filter, Transform baseTr) {
            _meshRenderer = new CachedUnityComponent<MeshRenderer>(renderer);
            filter.sharedMesh = ProceduralMeshUtility.GenerateQuad(Vector2.one, new Vector2(0.5f, 0));
            _filter = new CachedUnityComponent<MeshFilter>(filter);
            MeshVertices = new List<Vector3>();
            filter.sharedMesh.GetVertices(MeshVertices);
            MatBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(MatBlock);
            _spriteTr = new CachedTransform(renderer.transform);
            _baseTr = new CachedTransform(baseTr);
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
            _filter.Value.sharedMesh.SetVertices(MeshVertices);
        }

        public void Dispose() {
            _spriteTr?.Dispose();
            _spriteTr = null;
            _baseTr?.Dispose();
            _baseTr = null;
        }
    }
}
