using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class SimpleVolumetricParticle : MonoBehaviour, IProjectile, IOnCreate {

        [SerializeField] private ParticleSystem.MinMaxGradient _currentColors = new ParticleSystem.MinMaxGradient();
        [SerializeField] private ParticleSystem _lengthParticles = null;
        [SerializeField] private ParticleSystem _glowParticles = null;
        [SerializeField] private Light _particleLight = null;
        [SerializeField] private float _baseLength = 0.32f;
        [SerializeField] private NormalizedFloatRange _baseSize = new NormalizedFloatRange(0.12f, 0.15f);
        [SerializeField] private float _baseGlowSize = 0.45f;
        [SerializeField, Range(0, 5)] private float _lengthMulti = 1f;
        [SerializeField, Range(0, 5)] private float _sizeMulti = 1f;
        [SerializeField] private bool _changeMainColor = true;

        [SerializeField] private Rigidbody _rb = null;
        [SerializeField] private CapsuleCollider _capsuleCollider = null;

        public Transform Tr { get; private set; }
        public MaterialPropertyBlock[] MaterialBlocks { get { return null; } }
        public Renderer[] Renderers { get { return null; } }
        public Rigidbody Rigidbody { get { return _rb; } }
        public Collider Collider { get { return _capsuleCollider; } }

        public void OnCreate(PrefabEntity entity) {
            Tr = transform;
        }

        [Button]
        public void UpdateParticles() {
            var lengthShape = _lengthParticles.shape;
            lengthShape.scale = new Vector3(lengthShape.scale.x, lengthShape.scale.y, _baseLength * _lengthMulti);
            var colorShape = _glowParticles.shape;
            colorShape.scale = new Vector3(colorShape.scale.x, colorShape.scale.y, _baseLength * _lengthMulti);
            var colorLifetime = _glowParticles.colorOverLifetime;
            colorLifetime.color = _currentColors;
            var glowMain = _glowParticles.main;
            glowMain.startColor = _currentColors.gradient.colorKeys[0].color;
            _particleLight.color = _currentColors.gradient.colorKeys.LastElement().color;
            glowMain.startSize = _baseGlowSize * _sizeMulti;
            var lengthMain = _lengthParticles.main;
            lengthMain.startSize = new ParticleSystem.MinMaxCurve(_baseSize.Min * _sizeMulti, _baseSize.Max * _sizeMulti);
            lengthMain.startColor = _changeMainColor ? _currentColors.gradient.colorKeys[0].color : Color.white;
        }

        public void ApplyMaterialBlocks(MaterialPropertyBlock[] matBlocks) {}

        public void SetRendering(RenderingMode status) {
            if (status != RenderingMode.None) {
                _lengthParticles.Play();
                _glowParticles.Play();
            }
            else {
                _lengthParticles.Stop();
                _glowParticles.Stop();
            }
        }

        public void SetColor(Color main, Color offset) {
            var colorKeys = _currentColors.gradient.colorKeys;
            var alphaKeys = _currentColors.gradient.alphaKeys;
            colorKeys[0].color = main;
            colorKeys[_currentColors.gradient.colorKeys.LastIndex()].color = offset;
            _currentColors.gradient.SetKeys(colorKeys, alphaKeys);
            UpdateParticles();
        }

        public void SetSize(float size, float length) {
            _lengthMulti = length;
            _sizeMulti = size;
            _capsuleCollider.height = (_baseLength * length) * 2;
            _capsuleCollider.radius = _baseSize.Min * size;
            UpdateParticles();
        }
    }
}
