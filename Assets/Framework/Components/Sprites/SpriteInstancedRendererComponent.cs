using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {

    public class SpriteData {
        private Sprite _sprite;
        public Texture2D Normal;
        public Texture2D Emissive;
        public bool Flip = false;
        public readonly MaterialPropertyBlock MatBlock;
        public Vector4 Uv { get; private set; }
        public bool IsDirty { get; private set; }
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

        public void SetUv(Vector4 uv) {
            Uv = uv;
            IsDirty = false;
        }
    }

    [System.Serializable]
    public sealed class SpriteSimpleRendererComponent : IComponent {
        private CachedTransform _spriteTr;
        
        public SpriteData[] Sprites = new SpriteData[0];
        public Mesh Quad;
        public Material Mat;
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

        public Vector3 GetEventPosition(AnimationFrame frame, int index) {
            if (index >= Sprites.Length || index < 0) {
                return _spriteTr.Tr.position;
            }
            var sprite = Sprites[index].Sprite;
            if (sprite == null) {
                return _spriteTr.Tr.position;
            }
            var size = new Vector2(
                sprite.rect.width / sprite.pixelsPerUnit,
                sprite.rect.height / sprite.pixelsPerUnit);
            return _spriteTr.Tr.TransformPoint(
                Mathf.Lerp(-(size.x * 0.5f), (size.x * 0.5f), frame.EventPosition.x), size.y * frame.EventPosition.y, 0);
        }

        public SpriteSimpleRendererComponent(Transform spriteTr) {
            _spriteTr = new CachedTransform(spriteTr);
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
 
    [System.Serializable]
    public sealed class SpriteInstancedRendererComponent : IComponent {
        private CachedTransform _spriteTr;
        private Vector3 _position = Vector3.zero;
        
        public SpriteData[] Sprites = new SpriteData[0];
        public Quaternion Rotation {
            get { return _spriteTr != null ? _spriteTr.Tr.rotation : Quaternion.identity; }
        }
        public Vector3 Position {
            get {
                if (_spriteTr != null && _spriteTr.Tr != null) {
                    return _spriteTr.Tr.TransformPoint(_position);
                }
                return _position;
            }
        }
        public Vector3 Scale { get { return _spriteTr.Tr.localScale; } }

        public Vector3 GetEventPosition(AnimationFrame frame, Sprite sprite) {
            var size = new Vector2(sprite.rect.width / sprite.pixelsPerUnit,
                sprite.rect.height / sprite.pixelsPerUnit);
            return _spriteTr.Tr.TransformPoint(
                Mathf.Lerp(-(size.x * 0.5f), (size.x * 0.5f), frame.EventPosition.x), size.y * frame.EventPosition.y, 0);
        }

        public SpriteInstancedRendererComponent(Transform spriteTr) {
            _spriteTr = new CachedTransform(spriteTr);
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
