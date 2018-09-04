using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class UITweenOnEvent : MonoBehaviour {

        [SerializeField] private int _enableEvent = 2;
        [SerializeField] private int _disableEvent = 1;
        [SerializeField] private CanvasGroup _canvasGroup = null;
        [SerializeField] private TweenFloat _inTween = new TweenFloat();
        [SerializeField] private TweenFloat _outTween = new TweenFloat();

        void Awake() {
            MessageKit.addObserver(_enableEvent, EnableCanvas);
            MessageKit.addObserver(_disableEvent, DisableCanvas);
        }

        private void EnableCanvas() {
            if (_inTween.Length <= 0) {
                _canvasGroup.alpha = 1;
            }
            else {
                TimeManager.StartUnscaled(TweenAnim((tween) => _canvasGroup.alpha = tween.Get(), _inTween));
            }
        }

        private IEnumerator TweenAnim(System.Action<TweenFloat> action, TweenFloat tweener) {
            while (tweener.Active) {
                action(tweener);
                yield return null;
            }
        }

        private void DisableCanvas() {
            if (_outTween.Length <= 0) {
                _canvasGroup.alpha = 0;
            }
            else {
                TimeManager.StartUnscaled(TweenAnim((tween) => _canvasGroup.alpha = tween.Get(), _outTween));
            }
        }
    }
}