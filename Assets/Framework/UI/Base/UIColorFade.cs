using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIColorFade : MonoSingleton<UIColorFade> {

        [SerializeField] private float _painHoldTime = 0.25f;
        [SerializeField] private float _maxAlpha = 0.75f;
        [SerializeField] private TweenFloat _painEnd = new TweenFloat();
        [SerializeField] private TweenFloat _normalFade = new TweenFloat();
        [SerializeField] private RawImage _image = null;
        [SerializeField] private Color _painColor = Color.red;
        [SerializeField] private CanvasGroup _canvasGroup = null;
        [SerializeField] private RenderTexture _renderTexture = null;
        [SerializeField] private Material _rtMaterial = null;

        private Task _fadeTask;

        void Awake() {
            MessageKit.addObserver(Messages.PlayerDamaged, PlayerHurt);
        }
        
        private void PlayerHurt() {
            if (!GameOptions.UsePainFlash) {
                return;
            }
            if (_fadeTask != null) {
                TimeManager.Cancel(_fadeTask);
            }
            _fadeTask = TimeManager.StartUnscaled(PainAnimation(), Clear);
        }

        public void HurtFlash() {
            if (_fadeTask != null) {
                TimeManager.Cancel(_fadeTask);
            }
            _fadeTask = TimeManager.StartUnscaled(PainAnimation(), Clear);
        }

        public void Fade(float totalTime) {
            if (_fadeTask != null) {
                TimeManager.Cancel(_fadeTask);
            }
            _fadeTask = TimeManager.StartUnscaled(FadeAnimation(totalTime), Clear);
        }

        public void FadeTo(float time, Color color, bool endStatus) {
            if (_fadeTask != null) {
                TimeManager.Cancel(_fadeTask);
            }
            _fadeTask = TimeManager.StartUnscaled(FadeOneStep(time, color, endStatus), Clear);
        }

        public void TransitionPlayerCamera(float totalTime, float extraPause) {
            _image.color = Color.white;
            _image.material = _rtMaterial;
            _image.texture = _renderTexture;
            Player.Cam.targetTexture = _renderTexture;
            Player.Cam.Render();
            Player.Cam.targetTexture = null;
            _canvasGroup.alpha = 1;
            _image.enabled = true;
            _rtMaterial.SetFloat("_Cutoff", 0);
            TimeManager.StartUnscaled(TransitionRenderTexture(totalTime, extraPause));
        }

        private void Clear() {
            _fadeTask = null;
        }

        [UnityEngine.ContextMenu("Test Animation")]
        private void TestAnimation() {
            TimeManager.StartUnscaled(PainAnimation());
        }

        private IEnumerator TransitionRenderTexture(float totalTime, float extraPause) {
            yield return extraPause;
            _normalFade.Restart(0, 1, totalTime);
            while (_normalFade.Active) {
                _rtMaterial.SetFloat("_Cutoff", _normalFade.Get());
                yield return null;
            }
            _image.enabled = false;
            _canvasGroup.alpha = 0;
        }

        private IEnumerator FadeAnimation(float totalTime) {
            _image.color = Color.black;
            _image.material = null;
            _image.texture = null;
            _image.enabled = true;
            _canvasGroup.alpha = 0;
            _normalFade.Restart(0,1,totalTime * 0.5f);
            while (_normalFade.Active) {
                _canvasGroup.alpha = _normalFade.Get();
                yield return null;
            }
            _normalFade.Restart(1,0,totalTime * 0.5f);
            while (_normalFade.Active) {
                _canvasGroup.alpha = _normalFade.Get();
                yield return null;
            }
            _image.enabled = false;
        }

        private IEnumerator FadeOneStep(float totalTime, Color color, bool endStatus) {
            _image.color = color;
            _image.material = null;
            _image.texture = null;
            _image.enabled = true;
            _normalFade.Restart(endStatus ? 0 : 1, endStatus ? 1 : 0, totalTime);
            while (_normalFade.Active) {
                _canvasGroup.alpha = _normalFade.Get();
                yield return null;
            }
            _image.enabled = endStatus;
        }

        private IEnumerator PainAnimation() {
            _image.color = _painColor;
            _image.material = null;
            _image.texture = null;
            _image.enabled = true;
            _canvasGroup.alpha = _maxAlpha;
            yield return _painHoldTime;
            _painEnd.Restart(_maxAlpha,0);
            while (_painEnd.Active) {
                _canvasGroup.alpha = _painEnd.Get();
                yield return null;
            }
            _image.enabled = false;
        }
    }
}