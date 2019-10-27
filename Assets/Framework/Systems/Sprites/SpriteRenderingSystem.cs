using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace PixelComrades {
    [AutoRegister, Priority(Priority.Lowest)]
    public sealed class SpriteRenderingSystem : SystemBase, IMainSystemUpdate {
        
        public static int ShaderPropertyUv = Shader.PropertyToID("_MainTex_UV");
        public static int ShaderPropertyColor = Shader.PropertyToID("_Color");
        public static int ShaderPropertyTexture = Shader.PropertyToID("_MainTex");
        public static int ShaderPropertyNormal = Shader.PropertyToID("_BumpMap");
        public static int ShaderPropertyEmissive = Shader.PropertyToID("_EmissionMap");
        public static string MaterialAddress = "SpriteNpcInstanced.mat";
        public static int RenderLayer = LayerMasks.NumberEnemy;
        
        private TemplateList<SpriteRendererTemplate> _list;
        private ManagedArray<SpriteRendererTemplate>.RefDelegate _del;
        private Queue<RenderBlock> _renderPool = new Queue<RenderBlock>();
        private Material _material;
        private Dictionary<Texture2D, RenderBlock> _blocks = new Dictionary<Texture2D, RenderBlock>();
        private List<Texture2D> _textureList = new List<Texture2D>();

        public SpriteRenderingSystem() {
            TemplateFilter<SpriteRendererTemplate>.Setup();
            _list = EntityController.GetTemplateList<SpriteRendererTemplate>();
            _del = RunUpdate;
            ItemPool.LoadAsset<Material>(MaterialAddress, (m)=> _material = m);
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            _textureList.Clear();
            _blocks.Clear();
            _list.Run(_del);
            for (int i = 0; i < _textureList.Count; i++) {
                var block = _blocks[_textureList[i]];
                block.MaterialPropertyBlock.SetVectorArray(ShaderPropertyUv, block.UvList);
                block.MaterialPropertyBlock.SetVectorArray(ShaderPropertyColor, block.Colors);
                Graphics.DrawMeshInstanced(
                    block.Quad,
                    0,
                    block.Material,
                    block.MatrixList,
                    block.MaterialPropertyBlock, ShadowCastingMode.TwoSided, true, RenderLayer);
                _renderPool.Enqueue(block);
            }
        }

        private void RunUpdate(ref SpriteRendererTemplate template) {
            bool backwards = template.Billboard.Backwards;
            if (template.Renderer.MeshRenderer != null) {
                backwards = template.Billboard.Backwards && !template.Renderer.FlipX;
            }
            template.Billboard.Billboard.Apply(template.Renderer.SpriteTr, backwards, ref template.Billboard.LastAngleHeight);
            var orientation = SpriteFacingControl.GetCameraSide(
                template.Billboard.Facing, template.Renderer.SpriteTr,
                template.Renderer.BaseTr, 5, out var inMargin);
            if (!inMargin || !(orientation.IsAdjacent(template.Billboard.Orientation))){
                template.Billboard.Orientation = orientation;
            }
            if (template.Renderer.IsDirty) {
                template.Renderer.UpdatedSprite();
                var sprite = template.Renderer.Sprite;
                template.Renderer.Uv = GetUv(sprite);
                if (template.Renderer.SavedCollider != null) {
                    template.Collider.Value.UpdateCollider(template.Renderer.SavedCollider);
                }
                else {
                    template.Collider.Value.UpdateSprite(sprite, template.Renderer.FlipX);
                }
                if (template.Renderer.MeshRenderer != null) {
                    UpdateMeshRenderer(template);
                    return;
                }
            }
            if (template.Renderer.MeshRenderer != null) {
                return;
            }
            var texture = template.Renderer.Sprite.texture;
            if (!_blocks.TryGetValue(texture, out var block)) {
                _textureList.Add(texture);
                block = CreateBlock(template.Renderer);
                _blocks.Add(texture, block);
            }
            var tr = template.Renderer.SpriteTr;
            var scale = tr.localScale;
            if (template.Renderer.FlipX) {
                scale.x = -scale.x;
            }
            var matrix = Matrix4x4.TRS(tr.position, tr.rotation, scale);
            block.MatrixList.Add(matrix);
            block.UvList.Add(template.Renderer.Uv);
            block.Colors.Add(template.SpriteColor?.CurrentColor ?? Color.white);
        }

        private void UpdateMeshRenderer(SpriteRendererTemplate template) {
            var renderer = template.Renderer;
            renderer.MatBlock.SetVector(ShaderPropertyColor, template.SpriteColor.CurrentColor);
            renderer.MatBlock.SetVector(ShaderPropertyUv, renderer.Uv);
            renderer.MatBlock.SetTexture(ShaderPropertyTexture, renderer.Sprite.texture);
            renderer.MatBlock.SetTexture(ShaderPropertyNormal, renderer.Normal);
            renderer.MatBlock.SetTexture(ShaderPropertyEmissive, renderer.Emissive);
            renderer.ApplyMaterialBlock();
            var pixelsPerUnit = renderer.Sprite.pixelsPerUnit;
            var size = new Vector2(
                renderer.Sprite.rect.width / pixelsPerUnit,
                renderer.Sprite.rect.height / pixelsPerUnit);
            Vector2 scaledPivot = size * new Vector2(0.5f, 0);
            renderer.MeshVertices[0] = new Vector3(size.x - scaledPivot.x, size.y - scaledPivot.y, 0);
            renderer.MeshVertices[1] = new Vector3(size.x - scaledPivot.x, -scaledPivot.y, 0);
            renderer.MeshVertices[2] = new Vector3(-scaledPivot.x, -scaledPivot.y, 0);
            renderer.MeshVertices[3] = new Vector3(-scaledPivot.x, size.y - scaledPivot.y, 0);
            renderer.UpdateMesh();
        }

        private RenderBlock CreateBlock(SpriteRendererComponent data) {
            var block = GetRenderBlock();
            block.Clear();
            block.Material.SetTexture(ShaderPropertyTexture, data.Sprite.texture);
            block.Material.SetTexture(ShaderPropertyNormal, data.Normal);
            block.Material.SetTexture(ShaderPropertyEmissive, data.Emissive);
            var pixelsPerUnit = data.Sprite.pixelsPerUnit;
            var size = new Vector2(
                data.Sprite.rect.width / pixelsPerUnit,
                data.Sprite.rect.height / pixelsPerUnit);
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
            public  MaterialPropertyBlock MaterialPropertyBlock;


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
        
        public SpriteRendererComponent Renderer { get => _renderer.Value; }
        public SpriteColliderComponent Collider { get => _collider.Value; }
        public SpriteBillboardComponent Billboard => _billboard.Value;
        public SpriteColorComponent SpriteColor => _spriteColor.Value;
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _renderer, _collider, _billboard, _spriteColor
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
