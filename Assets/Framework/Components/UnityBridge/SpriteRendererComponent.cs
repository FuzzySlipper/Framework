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
        private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");
        private static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
        
        //private CachedUnityComponent<SpriteRenderer> _renderer;
        private CachedTransform _baseTr;
        private CachedTransform _spriteTr;
        public Queue<MaterialValue> MaterialValues = new Queue<MaterialValue>();
        public Vector4 Uv;
        public bool IsDirty { get; private set; }
        public Sprite Sprite { get; private set; }
        public Texture2D Normal { get; private set; }
        public Texture2D Emissive { get; private set; }
        public bool FlipX { get; private set; }
        //private SpriteRenderer Value { get => _renderer; }
        public Transform BaseTr { get => _baseTr; }
        public Transform SpriteTr { get => _spriteTr; }
        
        private CircularBuffer<string> _spriteChanges = new CircularBuffer<string>(20, true);

        [Command("listSpriteChanges")]
        public static void ListSpriteChanges(int entity) {
            var log = EntityController.Get(entity).Get<SpriteRendererComponent>()?._spriteChanges;
            if (log == null) {
                Console.Log(entity + " does not have component");
                return;
            }
            foreach (var msg in log.InOrder()) {
                Console.Log(string.Format("{0}: {1}",log.GetTime(msg), msg));
            }
        }
        public void ApplyMaterialBlock() {
            //Value.SetPropertyBlock(matBlocks[0]);
        }

        public void SetRendering(RenderingMode status) {
            //Value.SetMode(status);
        }

        public void SetFloat(int id, float value) {
            //MaterialBlocks[0].SetFloat(id, value);
            MaterialValues.Enqueue(new MaterialValue(id, value, null));
        }

        public void SetVector(int id, Vector3 value) {
            //MaterialBlocks[0].SetFloat(id, value);
            MaterialValues.Enqueue(new MaterialValue(id, -1, value));
        }

        public void SetSprite(Sprite sprite, Texture2D normal, Texture2D emissive) {
            _spriteChanges.Add(sprite.name);
            IsDirty = true;
            Sprite = sprite;
            Normal = normal;
            Emissive = emissive;
        }

        public void UpdatedSprite() {
            IsDirty = false;
            //Value.sprite = _sprite;
        }

        public void Flip(bool flipped) {
            FlipX = flipped;
        }

//        public void SetTextures(Texture2D normalMap, Texture2D emissive) {
//            MaterialBlocks[0].SetTexture(BumpMap, normalMap);
//            MaterialBlocks[0].SetTexture(EmissionMap, emissive);
//            if (emissive != null) {
//                Value.material.EnableKeyword("_EMISSION");
//            }
//            else {
//                Value.material.DisableKeyword("_EMISSION");
//            }
//            Value.SetPropertyBlock(MaterialBlocks[0]);
//        }

        public Vector3 GetEventPosition(AnimationFrame frame) {
            var sprite = Sprite;
            var pixelsPerUnit = sprite.pixelsPerUnit;
            var width = (sprite.rect.width / 2) / pixelsPerUnit;
            var height = (sprite.rect.height / 2) / pixelsPerUnit;
            //return renderer.transform.position + new Vector3(frame.EventPosition.x * width, height + (frame.EventPosition.y * height), 0);
            return SpriteTr.TransformPoint(
                new Vector3(frame.EventPosition.x * width, height + (frame.EventPosition.y * height), 0));
            //return renderer.bounds.center + new Vector3(frame.EventPosition.x * -renderer.bounds.extents.x, frame.EventPosition.y * -renderer.bounds.extents.y);
        }

        public SpriteRendererComponent(Transform spriteTr, Transform baseTr) {
            //_renderer = new CachedUnityComponent<SpriteRenderer>(renderer);
            _spriteTr = new CachedTransform(spriteTr);
            _baseTr = new CachedTransform(baseTr);
            IsDirty = false;
            Setup();
        }

        private void Setup() {
            //MaterialBlocks = new[] { new MaterialPropertyBlock()};
            //Value.GetPropertyBlock(MaterialBlocks[0]);
            //SpriteTr = Value.transform;
        }
        
        public SpriteRendererComponent(SerializationInfo info, StreamingContext context) {
            _spriteTr = info.GetValue(nameof(_spriteTr), _spriteTr);
            _baseTr = info.GetValue(nameof(_baseTr), _baseTr);
            Setup();
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_spriteTr), _spriteTr);
            info.AddValue(nameof(_baseTr), _baseTr);
        }

        public void Dispose() {
            _spriteTr?.Dispose();
            _spriteTr = null;
            _baseTr?.Dispose();
            _baseTr = null;
        }
    }
}
