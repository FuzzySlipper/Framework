using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;

namespace PixelComrades {
    [ExecuteInEditMode]
    public sealed class TestInstancedSpriteRendering : MonoBehaviour {
        private static int _shaderPropertyUv = Shader.PropertyToID("_MainTex_UV");
        private static int _shaderPropertyColor = Shader.PropertyToID("_Color");
        private static int _shaderPropertyTexture = Shader.PropertyToID("_MainTex");
        private static int _shaderPropertyNormal = Shader.PropertyToID("_BumpMap");
        private static int _shaderPropertyEmissive = Shader.PropertyToID("_EmissionMap");
        private static int _shaderPropertyEmissivePower = Shader.PropertyToID("_EmissionPower");
        private static int _dissolveMaskPosition = Shader.PropertyToID("_DissolveMaskPosition");
        private static int _dissolveMaskRadius = Shader.PropertyToID("_DissolveMaskRadius");
        private static string _shaderEmissiveKeyword = "_EMISSION";
        
        [SerializeField] private Material _material = null;
        [SerializeField] private SpriteData[] _spriteData = new SpriteData[0];
        [SerializeField] private float _distance = 10;
        [SerializeField] private bool _disable = true;
        [SerializeField] private MeshRenderer _testRenderer = null;
        [SerializeField] private MeshFilter _testFilter = null;
        [SerializeField] private SpriteCollider _spriteCollider = null;

        private float _lastTime;
        
        [System.Serializable]
        public class SpriteData {
            //public int FrameGridX;
            public SpriteAnimation Animation;
            public Sprite Sprite = null;
            public bool Flip = false;
            public float Scale = 1;

            public Vector3 DissolvePosition = Vector3.zero;
            public float DissolveRadius = 0;
            
            public int Frame;
            public float FrameTimer;
            public Vector4 Uv;
            public Matrix4x4 Matrix;
        }

        [Button]
        public void ResetData() {
            _renderPool.Clear();
            _blocks.Clear();
            _textureList.Clear();
        }

        void OnEnable() {
            Camera.onPreCull -= DrawWithCamera;
            Camera.onPreCull += DrawWithCamera;
            if (_spriteCollider != null) {
                _spriteCollider.InitMeshCollider();
            }
        }

        void OnDisable() {
            Camera.onPreCull -= DrawWithCamera;
        }

        void Update() {
            if (_disable || _material == null) {
                return;
            }
            var deltaTime = TimeManager.DeltaUnscaled;
            var rotation = Quaternion.Inverse(transform.rotation);
            for (int i = 0; i < _spriteData.Length; i++) {
                ref var spriteData = ref _spriteData[i];
                spriteData.FrameTimer -= deltaTime;
                if (spriteData.FrameTimer <= 0 || spriteData.Sprite == null) {
                    spriteData.Frame++;
                    var frame = spriteData.Animation.GetFrame(spriteData.Frame);
                    if (frame == null) {
                        spriteData.Frame = 0;
                        frame = spriteData.Animation.GetFrame(spriteData.Frame);
                    }
                    spriteData.FrameTimer = spriteData.Animation.FrameTime * frame.Length;
                    spriteData.Sprite = spriteData.Animation.GetSprite(spriteData.Frame);
                    var width = spriteData.Sprite.rect.width;
                    var height = spriteData.Sprite.rect.height;
                    //var gridY = Mathf.FloorToInt(spriteData.Frame / spriteData.FrameGridX);
                    //var gridX = spriteData.Frame % spriteData.FrameGridX;
                    //var pixelCoordsX = gridX * width;
                    //var pixelCoordsY = spriteData.Sprite.texture.height - ((gridY+1) * height);
                    var pixelCoordsX = spriteData.Sprite.rect.x;
                    var pixelCoordsY = spriteData.Sprite.rect.y;
                    float uvWidth = width / spriteData.Sprite.texture.width;
                    float uvHeight = height / spriteData.Sprite.texture.height;
                    var uvOffsetX = pixelCoordsX / spriteData.Sprite.texture.width;
                    var uvOffsetY = pixelCoordsY / spriteData.Sprite.texture.height;
                    spriteData.Uv = new Vector4(uvWidth, uvHeight, uvOffsetX, uvOffsetY);
                }
                var scale = spriteData.Flip ? new Vector3(-1, 1, 1) : Vector3.one;
                //var rotation = spriteData.Flip ? transform.rotation : Quaternion.Inverse(transform.rotation);
                spriteData.Matrix = Matrix4x4.TRS(transform.position + (_distance * i * Vector3.right), rotation,scale * spriteData.Scale);
                
            }
            if (_testRenderer != null && _testFilter != null) {
                var blockMaterial = new MaterialPropertyBlock();
                var data = _spriteData[0];
                blockMaterial.SetVector(_shaderPropertyUv, data.Uv);
                blockMaterial.SetVector(_shaderPropertyColor, Random.Range(0, 25) < 1 ? Color.red : Color.white);
                blockMaterial.SetVector(_dissolveMaskPosition, data.DissolvePosition);
                blockMaterial.SetFloat(_dissolveMaskRadius, data.DissolveRadius);
                blockMaterial.SetTexture(_shaderPropertyTexture, data.Sprite.texture);
                blockMaterial.SetTexture(_shaderPropertyNormal, data.Animation.NormalMap);
                if (data.Animation.EmissiveMap != null) {
                    _testRenderer.sharedMaterial.EnableKeyword(_shaderEmissiveKeyword);
                    blockMaterial.SetTexture(_shaderPropertyEmissive, data.Animation.EmissiveMap);
                }
                else {
                    _testRenderer.material.DisableKeyword(_shaderEmissiveKeyword);
                }
                _testRenderer.SetPropertyBlock(blockMaterial);
                _testRenderer.transform.rotation = data.Flip ? Quaternion.identity : Quaternion.Inverse(transform.rotation);
                var frame = data.Animation.GetFrame(data.Frame);
                var pixelsPerUnit = data.Sprite.pixelsPerUnit;
                var size = new Vector2(data.Sprite.rect.width / pixelsPerUnit,
                    data.Sprite.rect.height / pixelsPerUnit);
                if (frame.HasEvent) {
                    var center = _spriteCollider.transform.TransformPoint(
                        Mathf.Lerp(
                            -(size.x * 0.5f), (size.x * 0.5f), frame.EventPosition.x), size.y * frame.EventPosition.y, 0);
                    Debug.DrawRay(center, _testRenderer.transform.forward * 5, Color.red, 5f);
                }
                if (_testFilter.sharedMesh == null) {
                    _testFilter.sharedMesh = ProceduralMeshUtility.GenerateQuad(size, new Vector2(0.5f, 0));
                }
                if (_vertices.Count == 0) {
                    _testFilter.sharedMesh.GetVertices(_vertices);
                }
                Resize(size);
                if (_spriteCollider != null) {
                    var savedCollider = data.Animation.GetSpriteCollider(data.Frame);
                    _spriteCollider.UpdateCollider(savedCollider);
                    if (savedCollider != null) {
                        var center = _spriteCollider.transform.TransformPoint(Mathf.Lerp(-(size.x *0.5f), (size.x * 0.5f), savedCollider
                        .CriticalRect.x),size.y * savedCollider.CriticalRect.y,0);
                        var colliderSize = new Vector3(savedCollider.CriticalRect.size.x * size.x,
                            savedCollider.CriticalRect.size.y * size.y, 0.5f);
                        DebugExtension.DebugBounds(new Bounds(center,colliderSize), Color.red);
                    }
                }
            }
        }

        private void Resize(Vector2 size) {
            Vector2 scaledPivot = size * new Vector2(0.5f,0);
            _vertices[0] = new Vector3(size.x - scaledPivot.x, size.y - scaledPivot.y, 0);
            _vertices[1] = new Vector3(size.x - scaledPivot.x, -scaledPivot.y, 0);
            _vertices[2] = new Vector3(-scaledPivot.x, -scaledPivot.y, 0);
            _vertices[3] = new Vector3(-scaledPivot.x, size.y - scaledPivot.y, 0);
            _testFilter.sharedMesh.SetVertices(_vertices);
        }
        
        private List<Vector3> _vertices = new List<Vector3>();

        private void DrawWithCamera(Camera cam) {
            if (_disable || !cam) {
                return;
            }
            Render(cam, cam.transform.localToWorldMatrix * Matrix4x4.TRS(Vector3.forward * 10, Quaternion.identity, Vector3.one));
        }
        
        private Queue<RenderBlock> _renderPool = new Queue<RenderBlock>();

        private RenderBlock GetRenderBlock() {
            if (_renderPool.Count > 0) {
                return _renderPool.Dequeue();
            }
            var block = new RenderBlock();
            block.Material = new Material(_material);
            block.MaterialPropertyBlock = new MaterialPropertyBlock();
            return block;
        }

        private class RenderBlock {
            public List<Matrix4x4> MatrixList = new List<Matrix4x4>();
            public List<Vector4> UvList = new List<Vector4>();
            public List<Vector4> Colors = new List<Vector4>();
            public List<Vector4> DissolvePositions = new List<Vector4>();
            public List<float> DissolveRadii = new List<float>();
            
            public Material Material;
            public Mesh Quad;
            public MaterialPropertyBlock MaterialPropertyBlock;

            public void Clear() {
                MatrixList.Clear();
                UvList.Clear();
                Colors.Clear();
                DissolvePositions.Clear();
                DissolveRadii.Clear();
                MaterialPropertyBlock.Clear();
            }
        }
        
        private Dictionary<Texture2D, RenderBlock> _blocks = new Dictionary<Texture2D,RenderBlock>();
        private List<Texture2D> _textureList = new List<Texture2D>();

        private RenderBlock CreateBlock(SpriteData data) {
            var block = GetRenderBlock();
            block.Clear();
            block.Material.SetTexture(_shaderPropertyTexture, data.Sprite.texture);
            block.Material.SetTexture(_shaderPropertyNormal, data.Animation.NormalMap);
            block.Material.SetTexture(_shaderPropertyEmissive, data.Animation.EmissiveMap);
            block.Material.SetFloat(_shaderPropertyEmissivePower, data.Animation.EmissiveMap != null ? 1 : 0);
            var pixelsPerUnit = data.Sprite.pixelsPerUnit;
            var size = new Vector2(
                data.Sprite.rect.width / (float) pixelsPerUnit,
                data.Sprite.rect.height / (float) pixelsPerUnit);
            block.Quad = ProceduralMeshUtility.GenerateQuad(size, new Vector2(0.5f, 0));
            return block;
        }
        private void Render(Camera cam, Matrix4x4 matrix) {
            if (_material == null) {
                return;
            }
            _textureList.Clear();
            _blocks.Clear();
            int sliceCount = 1023;
            for (int i = 0; i < _spriteData.Length; i += sliceCount) {
                int sliceSize = Mathf.Min(_spriteData.Length - i, sliceCount);
                for (int s = 0; s < sliceSize; s++) {
                    SpriteData data = _spriteData[i + s];
                    if (data == null || data.Sprite == null) {
                        continue;
                    }
                    var texture = data.Sprite.texture;
                    if (!_blocks.TryGetValue(texture, out var block)) {
                        _textureList.Add(texture);
                        block = CreateBlock(data);
                        _blocks.Add(texture, block);
                    }
                    block.MatrixList.Add(data.Matrix);
                    block.UvList.Add(data.Uv);
                    block.Colors.Add(Random.Range(0,25) < 1 ? Color.red : Color.white);
                    block.DissolvePositions.Add(data.DissolvePosition);
                    block.DissolveRadii.Add(data.DissolveRadius);
                }
            }

            for (int i = 0; i < _textureList.Count; i++) {
                var block = _blocks[_textureList[i]];
                
                block.MaterialPropertyBlock.SetVectorArray(_shaderPropertyUv, block.UvList);
                block.MaterialPropertyBlock.SetVectorArray(_shaderPropertyColor, block.Colors);
                block.MaterialPropertyBlock.SetFloatArray(_dissolveMaskRadius, block.DissolveRadii);
                block.MaterialPropertyBlock.SetVectorArray(_dissolveMaskPosition, block.DissolvePositions);
                
                Graphics.DrawMeshInstanced(
                    block.Quad,
                    0,
                    block.Material,
                    block.MatrixList,
                    block.MaterialPropertyBlock, ShadowCastingMode.TwoSided, true, gameObject.layer, cam
                );
                _renderPool.Enqueue(block);
            }
            //UnityEditor.EditorUtility.SetDirty(this);
        }
    }
}
