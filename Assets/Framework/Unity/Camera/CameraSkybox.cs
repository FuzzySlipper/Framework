using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Rendering.PostProcessing;

namespace PixelComrades {
    public class CameraSkybox : MonoBehaviour {

        [SerializeField] private RenderTexture _rt = null;
        [SerializeField] private LayerMask _bgLayerMask = new LayerMask();
        [SerializeField] private LayerMask _normalLayerMask = new LayerMask();
        [SerializeField] private LayerMask _resetMask = new LayerMask();
        [SerializeField] private Camera _cam = null;
        [SerializeField] private MonoBehaviour[] _cameraScripts = new MonoBehaviour[0];
        [SerializeField] private bool _blurBackground = true;
        [SerializeField] private MonoBehaviour _blurScript;

        public void ResetMask() {
            _cam.cullingMask = _resetMask;
        }

        [Button("UpdateBackground")]
        public void UpdateBackground() {
            for (int i = 0; i < _cameraScripts.Length; i++) {
                _cameraScripts[i].enabled = false;
            }
            if (_blurBackground) {
                _blurScript.enabled = true;
            }
            _cam.cullingMask = _bgLayerMask;
            _cam.targetTexture = _rt;
            _cam.Render();
            _cam.targetTexture = null;
            _cam.cullingMask = _normalLayerMask;
            if (_blurBackground) {
                _blurScript.enabled = false;
            }
            for (int i = 0; i < _cameraScripts.Length; i++) {
                _cameraScripts[i].enabled = true;
            }
        }
    }
}
