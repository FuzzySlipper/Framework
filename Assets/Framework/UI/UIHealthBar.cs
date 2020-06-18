using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIHealthBar : MonoBehaviour {

        private static Dictionary<CharacterTemplate, UIHealthBar> _activeBars = new Dictionary<CharacterTemplate, UIHealthBar>();

        private enum State {
            Waiting,
            Active,
            Fading
        }

        [SerializeField] private Slider _healthSlider = null;
        [SerializeField] private float _maxTweenLength = 1f;
        [SerializeField] private TweenFloat _healthTween = new TweenFloat();
        [SerializeField] private TweenFloat _fadeTween = new TweenFloat();
        [SerializeField] private float _timeOut = 15f;
        [SerializeField] private int _targetVital = 0;

        private CanvasGroup _canvasGroup;
        private float _endTime;
        private State _state = State.Waiting;


        void Awake() {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private IEnumerator UpdateBar(CharacterTemplate unit, float offset) {
            _healthSlider.value = unit.GetVital(_targetVital).CurrentPercent;
            //_shieldSlider.value = unit.VitalStats[Vitals.Shield].CurrentPercent;
            _endTime = TimeManager.Time + _timeOut;
            _state = State.Waiting;
            transform.position = unit.Entity.GetPosition();
            _canvasGroup.alpha = 1;
            var v3Offset = new Vector3(0, offset, 0);
            while (true) {
                transform.position = unit.Entity.GetPosition() + v3Offset;
                CameraSystem.Cam.FaceCamera(transform, false);
                if (_state != State.Fading) {
                    CheckSliders(unit);
                }
                if (_state == State.Waiting && TimeManager.Time > _endTime) {
                    _fadeTween.Restart(1, 0);
                    _state = State.Fading;
                }
                if (_state == State.Fading) {
                    if (_fadeTween.Active) {
                        _canvasGroup.alpha = _fadeTween.Get();
                    }
                    else {
                        break;
                    }
                }
                yield return null;
            }
            _activeBars.Remove(unit);
            ItemPool.Despawn(gameObject);
        }

        private void CheckSliders(CharacterTemplate unit) {
            _state = State.Waiting;
            if (!_healthTween.Active) {
                var healthPercent = unit.GetVital(_targetVital).CurrentPercent;
                var statDifference = Math.Abs(_healthSlider.value - healthPercent);
                if (statDifference > 0.01f) {
                    _healthTween.Restart
                        (_healthSlider.value, healthPercent, Mathf.Lerp(0.15f, _maxTweenLength, statDifference));
                }
            }
            else {
                _state = State.Active;
                _healthSlider.value = _healthTween.Get();
            }
            //if (!_shieldTween.Active) {
            //    //var shieldPercent = unit.VitalStats[Vitals.Shield].CurrentPercent;
            //    var statDifference = Math.Abs(_shieldSlider.value - shieldPercent);
            //    if ( statDifference > 0.01f) {
            //        _shieldTween.Restart
            //            (_shieldSlider.value, shieldPercent, Mathf.Lerp(0.15f, _maxTweenLength, statDifference));
            //    }
            //}
            //else {
            //    _shieldSlider.value = _shieldTween.Get();
            //    _state = State.Active;
            //}
        }






    }
}