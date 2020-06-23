using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace PixelComrades {
    [AutoRegister, Priority(Priority.Lowest)]
    public sealed class SpriteRenderingSystem : SystemBase, IMainSystemUpdate, IMainLateUpdate {
        
        public static readonly int ShaderPropertyUv = Shader.PropertyToID("_MainTex_UV");
        public static readonly int ShaderPropertyColor = Shader.PropertyToID("_Color");
        public static readonly int ShaderPropertyTexture = Shader.PropertyToID("_MainTex");
        public static readonly int ShaderPropertyNormal = Shader.PropertyToID("_NormalMap");
        public static readonly int ShaderPropertyEmissive = Shader.PropertyToID("_Emissive");
        public static readonly int ShaderPropertyEmissivePower = Shader.PropertyToID("_EmissionPower");
        // public static int RenderLayer = LayerMasks.NumberWeapon;
        
        private TemplateList<SpriteRendererTemplate> _rendererList;
        private ManagedArray<SpriteRendererTemplate>.RefDelegate _rendererDel;
        private ComponentArray<SpriteInstancedRendererComponent> _simpleRenderers;
        private ComponentArray<SpriteInstancedRendererComponent>.RefDelegate _simpleRendererDel;
        private Queue<RenderBlock> _renderPool = new Queue<RenderBlock>();
        private Material _material;
        // private Dictionary<Texture2D, RenderBlock> _blocks = new Dictionary<Texture2D, RenderBlock>();
        // private List<Texture2D> _textureList = new List<Texture2D>();

        public SpriteRenderingSystem() {
            _rendererList = EntityController.GetTemplateList<SpriteRendererTemplate>();
            _rendererDel = RunUpdate;

            _simpleRenderers = EntityController.GetComponentArray<SpriteInstancedRendererComponent>();
            _simpleRendererDel = RunUpdate;
            _material = LazyDb.Main.InstancedMat;
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            // _textureList.Clear();
            // _blocks.Clear();
            
            _rendererList.Run(_rendererDel);
            
            // for (int i = 0; i < _textureList.Count; i++) {
            //     var block = _blocks[_textureList[i]];
            //     block.MaterialPropertyBlock.SetVectorArray(ShaderPropertyUv, block.UvList);
            //     block.MaterialPropertyBlock.SetVectorArray(ShaderPropertyColor, block.Colors);
            //     Graphics.DrawMeshInstanced(
            //         block.Quad,
            //         0,
            //         block.Material,
            //         block.MatrixList,
            //         block.MaterialPropertyBlock, ShadowCastingMode.TwoSided, true, RenderLayer);
            //     _renderPool.Enqueue(block);
            // }
        }

        public void OnSystemLateUpdate(float dt, float unscaledDt) {
            _simpleRenderers.Run(_simpleRendererDel);
        }

        private void RunUpdate(ref SpriteRendererTemplate template) {
            if (template.Billboard != null) {
                bool backwards = template.Billboard.Backwards && !template.Renderer.FlipX;
                UpdateBillboard(template.Billboard, template.Renderer.BaseTr, template.Renderer.SpriteTr, backwards);
            }
            if (!template.Renderer.IsDirty) {
                return;
            }
            var sprite = template.Renderer.Sprite;
            var size = new Vector2(sprite.rect.width / sprite.pixelsPerUnit,
                sprite.rect.height / sprite.pixelsPerUnit);
            if (template.Renderer.SavedCollider != null) {
                template.Collider.Value.UpdateCollider(template.Renderer.SavedCollider);
                if (template.CriticalHitCollider != null) {
                    template.CriticalHitCollider.Assign(template.Renderer.SavedCollider.CriticalRect, size);
                }
            }
            else {
                template.Collider.Value.UpdateSprite(sprite, template.Renderer.FlipX);
            }
            template.Renderer.MatBlock.SetVector(ShaderPropertyColor, template.SpriteColor.CurrentColor);
            if (template.Renderer.IsMeshRenderer) {
                template.Renderer.Uv = GetUv(sprite);
                UpdateMeshRenderer(template, size);
            }
            template.Renderer.UpdatedSprite();
            template.Renderer.ApplyMaterialBlock();
        }

        private void UpdateBillboard(SpriteBillboardComponent billboard, Transform baseTr, Transform spriteTr, bool backwards) {
            billboard.Billboard.Apply(spriteTr, backwards, ref billboard.LastAngleHeight);
            var orientation = SpriteFacingControl.GetCameraSide(billboard.Facing, spriteTr,baseTr, 5, out var inMargin);
            if (!inMargin || !(orientation.IsAdjacent(billboard.Orientation))) {
                billboard.Orientation = orientation;
            }
        }

        private void RunUpdate(ref SpriteInstancedRendererComponent renderer) {
            for (int i = 0; i < renderer.Sprites.Length; i++) {
                var data = renderer.Sprites[i];
                if (data.Sprite == null) {
                    continue;
                }
                if (data.IsDirty) {
                    data.SetUv(GetUv(data.Sprite));
                    data.MatBlock.SetColor(ShaderPropertyColor, Color.white);
                    data.MatBlock.SetTexture(ShaderPropertyTexture, data.Sprite.texture);
                    data.MatBlock.SetTexture(ShaderPropertyNormal, data.Normal);
                    if (data.Emissive != null) {
                        data.MatBlock.SetTexture(ShaderPropertyEmissive, data.Emissive);
                    }
                    data.MatBlock.SetFloat(ShaderPropertyEmissivePower, data.Emissive != null ? 1 : 0);
                }
                var rotation = renderer.Rotation;
                var scale = renderer.Scale;
                if (data.IsFlipped) {
                    scale.x = -scale.x;
                    //rotation = Quaternion.Inverse(rotation);
                }
                var matrix = Matrix4x4.TRS(renderer.Position, rotation, scale);
                data.MatBlock.SetVector(ShaderPropertyUv, data.Uv);
                Graphics.DrawMesh(renderer.Quad, matrix, renderer.Mat, 0, null, 0, data.MatBlock, ShadowCastingMode.On);
            }
        }

        private void UpdateMeshRenderer(SpriteRendererTemplate template, Vector2 size) {
            var renderer = template.Renderer;
            renderer.MatBlock.SetVector(ShaderPropertyUv, renderer.Uv);
            Vector2 scaledPivot = size * new Vector2(0.5f, 0);
            renderer.MeshVertices[0] = new Vector3(size.x - scaledPivot.x, size.y - scaledPivot.y, 0);
            renderer.MeshVertices[1] = new Vector3(size.x - scaledPivot.x, -scaledPivot.y, 0);
            renderer.MeshVertices[2] = new Vector3(-scaledPivot.x, -scaledPivot.y, 0);
            renderer.MeshVertices[3] = new Vector3(-scaledPivot.x, size.y - scaledPivot.y, 0);
            renderer.UpdateMesh();
        }

        private RenderBlock CreateBlock(SpriteRendererComponent data) {
            return CreateBlock(data.Sprite, data.Normal, data.Emissive);
        }

        private RenderBlock CreateBlock(Sprite sprite, Texture2D normal, Texture2D emissive) {
            var block = GetRenderBlock();
            block.Clear();
            block.Material.SetTexture(ShaderPropertyTexture, sprite.texture);
            block.Material.SetTexture(ShaderPropertyNormal, normal);
            block.Material.SetTexture(ShaderPropertyEmissive, emissive);
            var pixelsPerUnit = sprite.pixelsPerUnit;
            var size = new Vector2(
                sprite.rect.width / pixelsPerUnit,
                sprite.rect.height / pixelsPerUnit);
            block.Quad = ProceduralMeshUtility.GenerateQuad(size, new Vector2(0.5f, 0));
            return block;
        }

        private RenderBlock GetRenderBlock() {
            if (_renderPool.Count > 0) {
                return _renderPool.Dequeue();
            }
            var block = new RenderBlock();
            block.Material = new Material(_material);
            block.MaterialPropertyBlock = new MaterialPropertyBlock();
            return block;
        }

        public static Vector4 GetUv(Sprite sprite) {
            var spriteRect = sprite.rect;
            float uvWidth = spriteRect.width / sprite.texture.width;
            float uvHeight = spriteRect.height / sprite.texture.height;
            var uvOffsetX = sprite.rect.x / sprite.texture.width;
            var uvOffsetY = sprite.rect.y / sprite.texture.height;
            return new Vector4(uvWidth, uvHeight, uvOffsetX, uvOffsetY);
        }

        private class RenderBlock {
            public List<Matrix4x4> MatrixList = new List<Matrix4x4>();
            public List<Vector4> UvList = new List<Vector4>();
            public List<Vector4> Colors = new List<Vector4>();
            public Material Material;
            public Mesh Quad;
            public MaterialPropertyBlock MaterialPropertyBlock;


            public void Clear() {
                MatrixList.Clear();
                UvList.Clear();
                Colors.Clear();
                MaterialPropertyBlock.Clear();
            }
        }
    }

    public class SpriteRendererTemplate : BaseTemplate {

        private CachedComponent<SpriteRendererComponent> _renderer = new CachedComponent<SpriteRendererComponent>();
        private CachedComponent<SpriteColliderComponent> _collider = new CachedComponent<SpriteColliderComponent>();
        private CachedComponent<SpriteBillboardComponent> _billboard = new CachedComponent<SpriteBillboardComponent>();
        private CachedComponent<SpriteColorComponent> _spriteColor = new CachedComponent<SpriteColorComponent>();
        private CachedComponent<CriticalHitCollider> _criticalHit = new CachedComponent<CriticalHitCollider>();
        
        public SpriteRendererComponent Renderer { get => _renderer.Value; }
        public SpriteColliderComponent Collider { get => _collider.Value; }
        public SpriteBillboardComponent Billboard => _billboard.Value;
        public SpriteColorComponent SpriteColor => _spriteColor.Value;
        public CriticalHitCollider CriticalHitCollider => _criticalHit.Value;
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _renderer, _collider, _billboard, _spriteColor, _criticalHit
        };

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(SpriteRendererComponent),
                typeof(SpriteColliderComponent),
                typeof(SpriteBillboardComponent),
            };
        }
    }
}
