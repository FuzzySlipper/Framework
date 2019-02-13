using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace PixelComrades {
    public class ShaderAnimator : TargetAnimator, IPoolEvents {

        [SerializeField] private float _length = 1;
        [SerializeField] private Renderer[] _renderers = new Renderer[0];
        [SerializeField] private string _shaderFeature = "_DissolveIntensity";
        [SerializeField] private bool _unscaled = true;
        [SerializeField] private float[] _shaderTargets = new float[0];
        [SerializeField] private string _shaderKeyword = "_DISSOLVE";
        [SerializeField] private ShadowCastingMode[] _shadowCastingModes = new ShadowCastingMode[0];

        private MaterialPropertyBlock[] _matBlocks;
        private Task _task;
        private int _index = -1;
        private float[] _originValues;

        public override float Length { get { return _length; } }
        public override bool IsPlaying { get { return _task != null; } }
        private float Target { get { return _shaderTargets[_index]; } }

        public override void Play() {
            if (_matBlocks == null) {
                SetMatBlocks();
            }
            _index++;
            if (_index >= _shaderTargets.Length) {
                _index = 0;
            }
            if (_task != null) {
                TimeManager.Cancel(_task);
            }
            _task = TimeManager.StartTask(PlayAnimation(), _unscaled, Finish);
        }

        public void OnPoolSpawned() {
            if (!string.IsNullOrEmpty(_shaderKeyword)) {
                for (int i = 0; i < _renderers.Length; i++) {
                    _renderers[i].material.EnableKeyword(_shaderKeyword);
                }
            }
        }

        public void OnPoolDespawned() {
            if (_matBlocks != null) {
                for (int i = 0; i < _matBlocks.Length; i++) {
                    _matBlocks[i].SetFloat(_shaderFeature, _originValues[i]);
                    _renderers[i].SetPropertyBlock(_matBlocks[i]);
                    _renderers[i].shadowCastingMode = ShadowCastingMode.On;
                }
            }
            if (!string.IsNullOrEmpty(_shaderKeyword)) {
                for (int i = 0; i < _renderers.Length; i++) {
                    _renderers[i].material.DisableKeyword(_shaderKeyword);
                }
            }
            _matBlocks = null;
            _index = -1;
        }

        private void SetMatBlocks() {
            _matBlocks = new MaterialPropertyBlock[_renderers.Length];
            _originValues = new float[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++) {
                _matBlocks[i] = new MaterialPropertyBlock();
                _renderers[i].GetPropertyBlock(_matBlocks[i]);
                _originValues[i] = _matBlocks[i].GetFloat(_shaderFeature);
            }
        }

        private void Finish() {
            _task = null;
        }

        public override void PlayFrame(float normalized) {
            if (_matBlocks == null) {
                SetMatBlocks();
            }
            for (int i = 0; i < _matBlocks.Length; i++) {
                _matBlocks[i].SetFloat(_shaderFeature, Mathf.Lerp(_originValues[i], Target, normalized));
                _renderers[i].SetPropertyBlock(_matBlocks[i]);
            }
        }

        private IEnumerator PlayAnimation() {
            var startTime = TimeManager.Time;
            var _startValues = new float[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++) {
                _startValues[i] = _matBlocks[i].GetFloat(_shaderFeature);
                _renderers[i].shadowCastingMode = _shadowCastingModes[_index];
            }
            while (TimeManager.Time < startTime + _length) {
                var percent = (TimeManager.Time - startTime) / _length;
                for (int i = 0; i < _matBlocks.Length; i++) {
                    _matBlocks[i].SetFloat(_shaderFeature, Mathf.Lerp(_startValues[i], Target, percent));
                    _renderers[i].SetPropertyBlock(_matBlocks[i]);
                }
                yield return null;
            }
            for (int i = 0; i < _matBlocks.Length; i++) {
                _matBlocks[i].SetFloat(_shaderFeature, Mathf.Lerp(_startValues[i], Target, 1));
                _renderers[i].SetPropertyBlock(_matBlocks[i]);
            }
        }
    }
}
