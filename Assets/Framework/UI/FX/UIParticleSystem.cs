using UnityEngine;
using UnityEngine.UI;

namespace PixelComrades {
    [ExecuteInEditMode] [RequireComponent(typeof(CanvasRenderer))]
    public class UIParticleSystem : MaskableGraphic {

        //[Tooltip("Having this enabled run the system in LateUpdate rather than in Update making it faster but less precise (more clunky)"), SerializeField] private bool _fixedTime = true;
        [SerializeField] private ParticleSystem _pSystem = null;
        [SerializeField] private bool _unscaled = false;

        private UIVertex[] _quad = new UIVertex[4];
        private Transform _transform;
        private Material _currentMaterial;
        private Texture _currentTexture;
        private Vector4 _imageUv = Vector4.zero;
        private ParticleSystem.MainModule _mainModule;
        private ParticleSystem.Particle[] _particles;
        private ParticleSystemRenderer _pRenderer;
        private ParticleSystem.TextureSheetAnimationModule _textureSheetAnimation;
        private int _textureSheetAnimationFrames;
        private Vector2 _textureSheetAnimationFrameSize;
        private bool _playing = false;

        public override Texture mainTexture { get { return _currentTexture; } }
        public bool Playing => _playing;
        public float Length { get { return _pSystem != null ? _pSystem.main.duration : 1f; } }
        private float DeltaTime {
            get {
                if (!Application.isPlaying) {
                    return Time.unscaledDeltaTime;
                }
                return _unscaled ? TimeManager.DeltaUnscaled : TimeManager.DeltaTime;
            }
        }

        protected override void Awake() {
            base.Awake();
            _playing = Initialize();
        }

        protected override void OnEnable() {
            base.OnEnable();
            _playing = Initialize();
        }

        //private void LateUpdate() {
        //    if (_pSystem == null || !_playing) {
        //        return;
        //    }
        //    if (!Application.isPlaying) {
        //        SetAllDirty();
        //    }
        //    else if (_fixedTime) {
        //        _pSystem.Simulate(DeltaTime, false, false, true);
        //        SetAllDirty();
        //    }
        //}

        private void Update() {
            if (_pSystem == null) {
                return;
            }
            if (Game.GameActive && Game.Paused && !_unscaled) {
                return;
            }
            //if (!_fixedTime && Application.isPlaying) {
                _pSystem.Simulate(DeltaTime, false, false, true);
                SetAllDirty();
            //}
        }

        protected bool Initialize() {
            if (_transform == null) {
                _transform = transform;
            }
            if (_pSystem == null) {
                _pSystem = GetComponent<ParticleSystem>();
                if (_pSystem == null) {
                    return false;
                }
            }
            _pSystem.Simulate(0, true, true);
            _pRenderer = _pSystem.GetComponent<ParticleSystemRenderer>();
            if (_pRenderer != null) {
                _pRenderer.enabled = false;
            }
            if (material == null) {
                var foundShader = Shader.Find("UI/Particles/Additive");
                material = new Material(foundShader);
            }
            _mainModule = _pSystem.main;
            if (_pSystem.main.maxParticles > 14000) {
                _mainModule.maxParticles = 14000;
            }
            _currentMaterial = material;
            if (_pRenderer != null && _pRenderer.sharedMaterial.mainTexture != null) {
                _currentTexture = _pRenderer.sharedMaterial.mainTexture;
            }
            else if (_currentMaterial && _currentMaterial.HasProperty("_MainTex")) {
                _currentTexture = _currentMaterial.mainTexture;
            }
            if (_currentTexture == null) {
                _currentTexture = Texture2D.whiteTexture;
            }
            material = _currentMaterial;
            // automatically set scaling
            _mainModule.scalingMode = ParticleSystemScalingMode.Hierarchy;
            _particles = new ParticleSystem.Particle[_pSystem.main.maxParticles];
            _imageUv = new Vector4(0, 0, 1, 1);
            // prepare texture sheet animation
            _textureSheetAnimation = _pSystem.textureSheetAnimation;
            _textureSheetAnimationFrames = 0;
            _textureSheetAnimationFrameSize = Vector2.zero;
            if (_textureSheetAnimation.enabled) {
                _textureSheetAnimationFrames = _textureSheetAnimation.numTilesX * _textureSheetAnimation.numTilesY;
                _textureSheetAnimationFrameSize = new Vector2(1f / _textureSheetAnimation.numTilesX, 1f / _textureSheetAnimation.numTilesY);
            }

            return true;
        }


        protected override void OnPopulateMesh(VertexHelper vh) {
            if (_pSystem == null || _particles == null) {
                return;
            }
            // prepare vertices
            vh.Clear();

            if (!gameObject.activeInHierarchy) {
                return;
            }

            var temp = Vector2.zero;
            var corner1 = Vector2.zero;
            var corner2 = Vector2.zero;
            // iterate through current particles
            var count = _pSystem.GetParticles(_particles);

            for (var i = 0; i < count; ++i) {
                var particle = _particles[i];

                // get particle properties
                Vector2 position = _mainModule.simulationSpace == ParticleSystemSimulationSpace.Local ? particle.position : _transform.InverseTransformPoint(particle.position);

                var rotation = -particle.rotation * Mathf.Deg2Rad;
                var rotation90 = rotation + Mathf.PI / 2;
                var pColor = particle.GetCurrentColor(_pSystem);
                var size = particle.GetCurrentSize(_pSystem) * 0.5f;

                // apply scale
                if (_mainModule.scalingMode == ParticleSystemScalingMode.Shape) {
                    position /= canvas.scaleFactor;
                }


                // apply texture sheet animation
                var particleUV = _imageUv;
                if (_textureSheetAnimation.enabled) {
                    var frameProgress = 1 - particle.remainingLifetime / particle.startLifetime;

                    if (_textureSheetAnimation.frameOverTime.curveMin != null) {
                        frameProgress = _textureSheetAnimation.frameOverTime.curveMin.Evaluate(1 - particle.remainingLifetime / particle.startLifetime);
                    }
                    else if (_textureSheetAnimation.frameOverTime.curve != null) {
                        frameProgress = _textureSheetAnimation.frameOverTime.curve.Evaluate(1 - particle.remainingLifetime / particle.startLifetime);
                    }
                    else if (_textureSheetAnimation.frameOverTime.constant > 0) {
                        frameProgress = _textureSheetAnimation.frameOverTime.constant - particle.remainingLifetime / particle.startLifetime;
                    }

                    frameProgress = Mathf.Repeat(frameProgress * _textureSheetAnimation.cycleCount, 1);
                    var frame = 0;

                    switch (_textureSheetAnimation.animation) {
                        case ParticleSystemAnimationType.WholeSheet:
                            frame = Mathf.FloorToInt(frameProgress * _textureSheetAnimationFrames);
                            break;

                        case ParticleSystemAnimationType.SingleRow:
                            frame = Mathf.FloorToInt(frameProgress * _textureSheetAnimation.numTilesX);

                            var row = _textureSheetAnimation.rowIndex;
                            //                    if (textureSheetAnimation.useRandomRow) { // FIXME - is this handled internally by rowIndex?
                            //                        row = Random.Range(0, textureSheetAnimation.numTilesY, using: particle.randomSeed);
                            //                    }
                            frame += row * _textureSheetAnimation.numTilesX;
                            break;
                    }

                    frame %= _textureSheetAnimationFrames;

                    particleUV.x = frame % _textureSheetAnimation.numTilesX * _textureSheetAnimationFrameSize.x;
                    particleUV.y = Mathf.FloorToInt(frame / _textureSheetAnimation.numTilesX) * _textureSheetAnimationFrameSize.y;
                    particleUV.z = particleUV.x + _textureSheetAnimationFrameSize.x;
                    particleUV.w = particleUV.y + _textureSheetAnimationFrameSize.y;
                }

                temp.x = particleUV.x;
                temp.y = particleUV.y;

                _quad[0] = UIVertex.simpleVert;
                _quad[0].color = pColor;
                _quad[0].uv0 = temp;

                temp.x = particleUV.x;
                temp.y = particleUV.w;
                _quad[1] = UIVertex.simpleVert;
                _quad[1].color = pColor;
                _quad[1].uv0 = temp;

                temp.x = particleUV.z;
                temp.y = particleUV.w;
                _quad[2] = UIVertex.simpleVert;
                _quad[2].color = pColor;
                _quad[2].uv0 = temp;

                temp.x = particleUV.z;
                temp.y = particleUV.y;
                _quad[3] = UIVertex.simpleVert;
                _quad[3].color = pColor;
                _quad[3].uv0 = temp;

                if (rotation == 0) {
                    // no rotation
                    corner1.x = position.x - size;
                    corner1.y = position.y - size;
                    corner2.x = position.x + size;
                    corner2.y = position.y + size;

                    temp.x = corner1.x;
                    temp.y = corner1.y;
                    _quad[0].position = temp;
                    temp.x = corner1.x;
                    temp.y = corner2.y;
                    _quad[1].position = temp;
                    temp.x = corner2.x;
                    temp.y = corner2.y;
                    _quad[2].position = temp;
                    temp.x = corner2.x;
                    temp.y = corner1.y;
                    _quad[3].position = temp;
                }
                else {
                    // apply rotation
                    var right = new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation)) * size;
                    var up = new Vector2(Mathf.Cos(rotation90), Mathf.Sin(rotation90)) * size;

                    _quad[0].position = position - right - up;
                    _quad[1].position = position - right + up;
                    _quad[2].position = position + right + up;
                    _quad[3].position = position + right - up;
                }

                vh.AddUIVertexQuad(_quad);
            }
        }
    }
}