using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class UILogoIntro : MonoSingleton<UILogoIntro> {

        [SerializeField] private CanvasGroup _canvasGroup = null;
        [SerializeField] private float _loadPause = 1f;
        [SerializeField] private float _fadeTime = 1.5f;
        void Awake() {
            _canvasGroup.alpha = 1;
        }

        public void FadeIn() {
            TimeManager.PauseFor(_loadPause, true, () => { _canvasGroup.FadeTo(0, _fadeTime, EasingTypes.SinusoidalInOut, true); });
        }
    }
}
