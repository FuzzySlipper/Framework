using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIHealthBarStatic : MonoBehaviour, ISystemUpdate, IReceive<EntityDisposed> {

        [SerializeField] private Slider _statSlider = null;
        [SerializeField] private TextMeshProUGUI _statText = null;
        [SerializeField] private TweenFloat _statTween = new TweenFloat();
        [SerializeField] private int _targetStat = 0;
        [SerializeField] private float _maxTweenLength = 1.5f;
        private CharacterTemplate _actor;

        public bool Unscaled { get { return true; } }

        public void OnSystemUpdate(float delta) {
            if (_actor != null && _statTween.Active) {
                _statSlider.value = _statTween.Get();
            }
        }

        public void SetNewTarget(CharacterTemplate actor) {
            if (_actor != null) {
                RemoveActor();
            }
            _actor = actor;
            if (_actor != null) {
                _actor.Entity.AddObserver(this);
                SetupActorStat();
            }
        }

        public void RemoveActor() {
            if (_actor != null) {
                if (!_actor.Disposed) {
                    var vital = _actor.GetVital(_targetStat);
                    if (vital != null) {
                        vital.OnStatChanged -= CheckStat;
                    }
                }
                _actor.Entity.RemoveObserver(this);
            }
            _statSlider.value = 0;
            _actor = null;
            if (_statText != null) {
                _statText.text = "";
            }
        }

        private void CheckStat(BaseStat stat) {
            if (_statText != null) {
                _statText.text = stat.ToString();
            }
            if (_actor.Entity.Tags.Contain(EntityTags.IsDead)) {
                _statTween.Restart(_statSlider.value, 0, 0.35f);
                return;
            }
            if (_statTween.Active) {
                return;
            }
            if (_statTween.Length <= 0) {
                _statSlider.value = _actor.GetVital(_targetStat).CurrentPercent;
                return;
            }
            var percent = _actor.GetVital(_targetStat).CurrentPercent;
            var statDifference = Mathf.Abs(_statSlider.value - percent);
            if (statDifference > 0.05f) {
                _statTween.Restart
                    (_statSlider.value, percent, Mathf.Lerp(0.35f, _maxTweenLength, statDifference));
            }
        }

        private void SetupActorStat() {
            if (_actor == null) {
                return;
            }
            if (_statText != null) {
                _statText.text = _actor.GetVital(_targetStat).ToString();
            }
            _actor.GetVital(_targetStat).OnStatChanged += CheckStat;
            _statSlider.value = _actor.GetVital(_targetStat).CurrentPercent;
        }


        public void Handle(EntityDisposed arg) {
            RemoveActor();
        }
    }
}