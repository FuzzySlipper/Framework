using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;

namespace PixelComrades {
    [ExecuteInEditMode]
    public sealed class TestSpriteAnimation : MonoBehaviour {
        private static int _shaderPropertyUv = Shader.PropertyToID("_MainTex_UV");
        private static int _shaderPropertyColor = Shader.PropertyToID("_Color");
        private static int _shaderPropertyTexture = Shader.PropertyToID("_MainTex");
        private static int _shaderPropertyNormal = Shader.PropertyToID("_BumpMap");
        private static int _shaderPropertyEmissive = Shader.PropertyToID("_EmissionMap");
        private static int _shaderPropertyEmissivePower = Shader.PropertyToID("_EmissionPower");
        private static int _dissolveMaskPosition = Shader.PropertyToID("_DissolveMaskPosition");
        private static int _dissolveMaskRadius = Shader.PropertyToID("_DissolveMaskRadius");
        private static string _shaderEmissiveKeyword = "_EMISSION";
        
        [SerializeField] private SpriteAnimation _animation = null;
        [SerializeField] private Sprite _sprite = null;
        [SerializeField] private MeshFilter _meshFilter = null;
        [SerializeField] private MeshRenderer _meshRenderer = null;
        [SerializeField] private SpriteCollider _spriteCollider = null;
        [SerializeField] private int _updateFrame = 0;
        [SerializeField] private bool _drawNow = true;
        [SerializeField] private Material _mat = null;
        
        private List<Vector3> _vertices = new List<Vector3>();
        private UnscaledTimer _drawTimer = new UnscaledTimer();
        private Mesh _mesh = null;
        private int _frame = 0;

        [Button]
        public void UpdateFrame() {
            UpdateSprite(_updateFrame);
        }

        void OnEnable() {
            if (!_drawNow) {
                return;
            }
            Camera.onPreCull -= DrawWithCamera;
            Camera.onPreCull += DrawWithCamera;
            if (_spriteCollider != null) {
                _spriteCollider.InitMeshCollider();
            }
        }

        void OnDisable() {
            if (!_drawNow) {
                return;
            }
            Camera.onPreCull -= DrawWithCamera;
        }

        private void DrawWithCamera(Camera cam) {
            if (!cam) {
                return;
            }
            if (_animation == null || _sprite == null) {
                return;
            }
            if (_mesh == null) {
                _mesh = ProceduralMeshUtility.GenerateQuad(new Vector2(20.48f, 10.24f), new Vector2(0.5f, 0));
            }
            if (_sprite != null) {
                DrawSprite();
                return;
            }
            if (!_drawTimer.IsActive) {
                _frame++;
                if (_frame >= _animation.Frames.Length) {
                    _frame = 0;
                }
                _drawTimer.StartNewTime(_animation.FrameTime * _animation.GetFrame(_frame).Length);
            }
            var sprite = _animation.GetSprite(_frame);
            var matBlock = new MaterialPropertyBlock();
            matBlock.SetTexture(_shaderPropertyTexture, sprite.texture);
            matBlock.SetTexture(_shaderPropertyNormal, _animation.NormalMap);
            matBlock.SetFloat(_shaderPropertyEmissivePower, _animation.EmissiveMap != null ? 1 : 0);
            if (_animation.EmissiveMap != null) {
                matBlock.SetTexture(_shaderPropertyEmissive, _animation.EmissiveMap);
            }
            matBlock.SetColor(_shaderPropertyColor, Color.white);
            matBlock.SetVector(_shaderPropertyUv, SpriteRenderingSystem.GetUv(sprite));
            Graphics.DrawMesh(_mesh, GetMatrix(), _mat, 0, null, 0, matBlock, ShadowCastingMode.On);
        }

        private Matrix4x4 GetMatrix() {
            return Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        }

        private void DrawSprite() {
            var matBlock = new MaterialPropertyBlock();
            matBlock.SetTexture(_shaderPropertyTexture, _sprite.texture);
            matBlock.SetColor(_shaderPropertyColor, Color.white);
            matBlock.SetVector(_shaderPropertyUv, SpriteRenderingSystem.GetUv(_sprite));
            Graphics.DrawMesh(_mesh, GetMatrix(), _mat, 0, null, 0, matBlock, ShadowCastingMode.On);
        }
        
#if UNITY_EDITOR
        private bool _looping;

        [Button]
        public void TestAnimation() {
            _looping = true;
            TimeManager.StartUnscaled(TestAnimationRunner());
        }

        [Button]
        public void StopLoop() {
            _looping = false;
        }

        private IEnumerator TestAnimationRunner() {
            _updateFrame = 0;
            UpdateSprite(_updateFrame);
            UnscaledTimer timer = new UnscaledTimer();
            timer.StartNewTime(_animation.FrameTime * _animation.GetFrame(0).Length);
            while (_looping) {
                if (!_looping) {
                    break;
                }
                if (timer.IsActive) {
                    yield return null;
                }
                _updateFrame++;
                var frame = _animation.GetFrame(_updateFrame);
                if (frame == null) {
                    _updateFrame = 0;
                    frame = _animation.GetFrame(_updateFrame);
                }
                timer.StartNewTime(_animation.FrameTime * frame.Length);
                UpdateSprite(_updateFrame);
                yield return null;
            }
        }
#endif

        [Button]
        public void SetTestTexture() {
            var texture = new Texture2D(2048, 1024);
            var colors = texture.GetPixels();
            for (int i = 0; i < colors.Length; i++) {
                colors[i] = Color.white;
            }
            texture.SetPixels(colors);
            var blockMaterial = new MaterialPropertyBlock();
            _meshRenderer.GetPropertyBlock(blockMaterial);
            blockMaterial.SetTexture(_shaderPropertyTexture, texture);
            blockMaterial.SetVector(_shaderPropertyColor, Color.white);
            _meshRenderer.SetPropertyBlock(blockMaterial);
        }

        private void UpdateSprite(int frameIdx) {
            var blockMaterial = new MaterialPropertyBlock();
            var sprite = _animation.GetSprite(frameIdx);
            var width = sprite.rect.width;
            var height = sprite.rect.height;
            //var gridY = Mathf.FloorToInt(spriteData.Frame / spriteData.FrameGridX);
            //var gridX = spriteData.Frame % spriteData.FrameGridX;
            //var pixelCoordsX = gridX * width;
            //var pixelCoordsY = sprite.texture.height - ((gridY+1) * height);
            var pixelCoordsX = sprite.rect.x;
            var pixelCoordsY = sprite.rect.y;
            float uvWidth = width / sprite.texture.width;
            float uvHeight = height / sprite.texture.height;
            var uvOffsetX = pixelCoordsX / sprite.texture.width;
            var uvOffsetY = pixelCoordsY / sprite.texture.height;
            blockMaterial.SetVector(_shaderPropertyUv, new Vector4(uvWidth, uvHeight, uvOffsetX, uvOffsetY));
            blockMaterial.SetVector(_shaderPropertyColor, Color.white);
            blockMaterial.SetTexture(_shaderPropertyTexture, sprite.texture);
            blockMaterial.SetTexture(_shaderPropertyNormal, _animation.NormalMap);
            if (_animation.EmissiveMap != null) {
                _meshRenderer.sharedMaterial.EnableKeyword(_shaderEmissiveKeyword);
                blockMaterial.SetTexture(_shaderPropertyEmissive, _animation.EmissiveMap);
            }
            else {
                _meshRenderer.sharedMaterial.DisableKeyword(_shaderEmissiveKeyword);
            }
            _meshRenderer.SetPropertyBlock(blockMaterial);
            var frame = _animation.GetFrame(frameIdx);
            var pixelsPerUnit = sprite.pixelsPerUnit;
            var size = new Vector2(sprite.rect.width / pixelsPerUnit,sprite.rect.height / pixelsPerUnit);
            if (frame.HasEvent) {
                var center = _meshFilter.transform.TransformPoint(
                    Mathf.Lerp(
                        -(size.x * 0.5f), (size.x * 0.5f), frame.EventPosition.x), size.y * frame.EventPosition.y, 0);
                Debug.DrawRay(center, _meshFilter.transform.forward * 5, Color.red, 5f);
            }
            if (_meshFilter.sharedMesh == null) {
                _meshFilter.sharedMesh = ProceduralMeshUtility.GenerateQuad(size, new Vector2(0.5f, 0));
            }
            if (_vertices.Count == 0) {
                _meshFilter.sharedMesh.GetVertices(_vertices);
            }
            Resize(size);
            if (_spriteCollider != null) {
                var savedCollider = _animation.GetSpriteCollider(frameIdx);
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

        private void Resize(Vector2 size) {
            Vector2 scaledPivot = size * new Vector2(0.5f, 0);
            _vertices[0] = new Vector3(size.x - scaledPivot.x, size.y - scaledPivot.y, 0);
            _vertices[1] = new Vector3(size.x - scaledPivot.x, -scaledPivot.y, 0);
            _vertices[2] = new Vector3(-scaledPivot.x, -scaledPivot.y, 0);
            _vertices[3] = new Vector3(-scaledPivot.x, size.y - scaledPivot.y, 0);
            _meshFilter.sharedMesh.SetVertices(_vertices);
        }
    }
}
