using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIXpSlider : MonoBehaviour {

        [SerializeField] private TextMeshProUGUI _text = null;
        [SerializeField] private Slider _slider = null;

        private ExperienceStat _stat;

        void OnDisable() {
            Clear();
        }

        public void Clear() {
            if (_stat != null) {
                _stat.TotalXp.OnResourceChanged -= RefreshText;
            }

            _stat = null;
        }

        public void AssignStat(ExperienceStat stat) {
            _stat = stat;
            _stat.TotalXp.OnResourceChanged += RefreshText;
            RefreshText();
        }

        private void RefreshText() {
            _text.text = string.Format("XP: {0}/{1}", _stat.TotalXp.Value, _stat.XpNeeded);
            _slider.value = _stat.Percent;
        }
    }
}