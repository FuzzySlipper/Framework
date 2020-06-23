using UnityEngine;

namespace PixelComrades {
    public class TweenAnimatorCanvasAlpha : TweenAnimator {

        [SerializeField] private float[] _targets = new float[0];
        [SerializeField] private EasingTypes[] _easing = new EasingTypes[0];
        [SerializeField] private float[] _durations = new float[0];
        [SerializeField] private CanvasGroup _canvasGroup = null;
        [SerializeField] private bool _adjustInteractive = true;

        private int _index = -1;
        private TweenFloat _tweener = new TweenFloat();

        public override Tweener Tween { get { return _tweener; } }

        public override void StartTween() {
            _index++;
            if (_index >= _targets.Length) {
                _index = 0;
            }
            _tweener.Restart(_canvasGroup.alpha, _targets[_index], _durations[_index], _easing[_index], UnScaled);
        }

        public override void PlayFrame(float normalized) {
            _canvasGroup.alpha = _tweener.Get(normalized);
        }

        public override void UpdateTween() {
            _canvasGroup.alpha = _tweener.Get();
            if (!_adjustInteractive) {
                return;
            }
            if (_canvasGroup.alpha >= 0.99f) {
                _canvasGroup.interactable = _canvasGroup.blocksRaycasts = true;
            }
            else if (_canvasGroup.alpha <= 0.01f) {
                _canvasGroup.interactable = _canvasGroup.blocksRaycasts = false;
            }
        }
    }
}