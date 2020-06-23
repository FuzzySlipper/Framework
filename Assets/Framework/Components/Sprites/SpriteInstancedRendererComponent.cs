using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {

    public class SpriteData {
        private Sprite _sprite;
        public Texture2D Normal;
        public Texture2D Emissive;
        public readonly MaterialPropertyBlock MatBlock;
        public bool IsDirty { get; protected set; }
        public Sprite Sprite {
            get { return _sprite; }
            set {
                IsDirty = true;
                _sprite = value;
            }
        }

        public SpriteData() {
            MatBlock = new MaterialPropertyBlock();
        }
    }

    public class RendererSprite : SpriteData {
        public readonly SpriteRenderer Renderer;

        public RendererSprite(SpriteRenderer renderer) {
            Renderer = renderer;
        }
    }

    [System.Serializable]
    public sealed class SpriteSimpleRendererComponent : IComponent, ISpriteRendererComponent {
        private CachedTransform _spriteTr;
        
        public RendererSprite[] Sprites = new RendererSprite[0];
        public Quaternion Rotation { get { return _spriteTr.Tr.rotation; } }
        public Vector3 Position { get { return _spriteTr.Tr.position; }}
        public Vector3 Scale { get { return _spriteTr.Tr.localScale; } }

        public Vector3 GetEventPosition(AnimationFrame frame, Sprite sprite) {
            var size = new Vector2(
                sprite.rect.width / sprite.pixelsPerUnit,
                sprite.rect.height / sprite.pixelsPerUnit);
            return _spriteTr.Tr.TransformPoint(
                Mathf.Lerp(-(size.x * 0.5f), (size.x * 0.5f), frame.EventPosition.x), size.y * frame.EventPosition.y, 0);
        }

        public Vector3 GetEventPosition(Vector2 framePos, int instancedIndex) {
            if (instancedIndex >= Sprites.Length || instancedIndex < 0) {
                return _spriteTr.Tr.position;
            }
            var sprite = Sprites[instancedIndex].Sprite;
            if (sprite == null) {
                return _spriteTr.Tr.position;
            }
            var size = new Vector2(
                sprite.rect.width / sprite.pixelsPerUnit,
                sprite.rect.height / sprite.pixelsPerUnit);
            return _spriteTr.Tr.TransformPoint(
                Mathf.Lerp(-(size.x * 0.5f), (size.x * 0.5f), framePos.x), size.y * framePos.y, 0.2f);
<<<<<<< HEAD
        }

        public void SetSprite(
            Sprite sprite, Texture2D normal, Texture2D emissive, SavedSpriteCollider spriteCollider, int instanceIdx,
            bool flip) {
            var data = Sprites[instanceIdx];
            data.Sprite = sprite;
            data.Emissive = emissive;
            data.Normal = normal;
            data.Flip = flip;
        }

=======
        }

        public void SetSprite(Sprite sprite, Texture2D normal, Texture2D emissive, SavedSpriteCollider spriteCollider, int instanceIdx,
            bool flip) {
            var data = Sprites[instanceIdx];
            data.Sprite = sprite;
            data.Emissive = emissive;
            data.Normal = normal;
            data.Renderer.sprite = sprite;
            data.Renderer.flipX = flip;
        }

>>>>>>> FirstPersonAction
        public Quaternion GetRotation() {
            return Rotation;
        }

        public void Flip(bool isFlipped) { }

<<<<<<< HEAD
        public SpriteSimpleRendererComponent(Transform spriteTr, int cnt) {
            _spriteTr = new CachedTransform(spriteTr);
            Sprites = new SpriteData[cnt];
            for (int i = 0; i < Sprites.Length; i++) {
                Sprites[i] = new SpriteData();
=======
        public SpriteSimpleRendererComponent(Transform spriteTr, SpriteRenderer[] renderers) {
            _spriteTr = new CachedTransform(spriteTr);
            Sprites = new RendererSprite[renderers.Length];
            for (int i = 0; i < Sprites.Length; i++) {
                Sprites[i] = new RendererSprite(renderers[i]);
>>>>>>> FirstPersonAction
            }
        }

        public SpriteSimpleRendererComponent(SerializationInfo info, StreamingContext context) {
            _spriteTr = info.GetValue(nameof(_spriteTr), _spriteTr);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_spriteTr), _spriteTr);
        }

        public void Dispose() {
            _spriteTr?.Dispose();
            _spriteTr = null;
        }
    }

    public class InstancedSprite : SpriteData {
        
        public Vector4 Uv { get; private set; }
        public bool IsFlipped = false;

        public void SetUv(Vector4 uv) {
            Uv = uv;
            IsDirty = false;
        }
      
    }
    
    [System.Serializable]
    public sealed class SpriteInstancedRendererComponent : IComponent, ISpriteRendererComponent {
        private CachedTransform _spriteTr;
        public Mesh Quad;
        public Material Mat;
        public InstancedSprite[] Sprites = new InstancedSprite[0];
        public Quaternion Rotation { get { return _spriteTr.Tr.rotation; } }
        public Vector3 Position { get { return _spriteTr.Tr.position; }}
        public Vector3 Scale { get { return _spriteTr.Tr.localScale; } }

        public Vector3 GetEventPosition(AnimationFrame frame, Sprite sprite) {
            var size = new Vector2(
                sprite.rect.width / sprite.pixelsPerUnit,
                sprite.rect.height / sprite.pixelsPerUnit);
            return _spriteTr.Tr.TransformPoint(
                Mathf.Lerp(-(size.x * 0.5f), (size.x * 0.5f), frame.EventPosition.x), size.y * frame.EventPosition.y, 0);
        }

        public Vector3 GetEventPosition(Vector2 framePos, int instancedIndex) {
            if (instancedIndex >= Sprites.Length || instancedIndex < 0) {
                return _spriteTr.Tr.position;
            }
            var sprite = Sprites[instancedIndex].Sprite;
            if (sprite == null) {
                return _spriteTr.Tr.position;
            }
            var size = new Vector2(
                sprite.rect.width / sprite.pixelsPerUnit,
                sprite.rect.height / sprite.pixelsPerUnit);
            return _spriteTr.Tr.TransformPoint(
                Mathf.Lerp(-(size.x * 0.5f), (size.x * 0.5f), framePos.x), size.y * framePos.y, 0.2f);
        }

        
        public void SetSprite(Sprite sprite, Texture2D normal, Texture2D emissive, SavedSpriteCollider spriteCollider, int instanceIdx,
            bool flip) {
            var data = Sprites[instanceIdx];
            data.Sprite = sprite;
            data.Emissive = emissive;
            data.Normal = normal;
            data.IsFlipped = flip;
        }

        public Quaternion GetRotation() {
            return Rotation;
        }

        public void Flip(bool isFlipped) { }

        public SpriteInstancedRendererComponent(Transform spriteTr, int cnt) {
            _spriteTr = new CachedTransform(spriteTr);
            Sprites = new InstancedSprite[cnt];
            for (int i = 0; i < Sprites.Length; i++) {
                Sprites[i] = new InstancedSprite();
            }
        }

        public SpriteInstancedRendererComponent(SerializationInfo info, StreamingContext context) {
            _spriteTr = info.GetValue(nameof(_spriteTr), _spriteTr);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_spriteTr), _spriteTr);
        }

        public void Dispose() {
            _spriteTr?.Dispose();
            _spriteTr = null;
        }
    }
 
}
