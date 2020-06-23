using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIStatWatcher : MonoBehaviour, IPoolEvents, IPointerEnterHandler, IPointerExitHandler {

        [SerializeField] private TextMeshProUGUI _text = null;
        [SerializeField] private Button _button = null;
        [SerializeField] private Image _hoverGraphic = null;
        [SerializeField] private Color _hoverColor = Color.white;
        [SerializeField] private Animator _animator = null;

        private Color _originalColor;
        private System.Func<string> _hoverDescr;
        private BaseStat _stat;

        public TextMeshProUGUI Text { get { return _text; } }

        void Awake() {
            if (_hoverGraphic != null) {
                _originalColor = _hoverGraphic.color;
            }
        }

        public void OnPoolSpawned() {
        }

        public void OnPoolDespawned() {
            if (_stat != null) {
                _stat.OnStatChanged -= RefreshText;
            }
            _button.gameObject.SetActive(false);
            _stat = null;
            _hoverDescr = null;
        }

        public void AssignStat(BaseStat stat) {
            _stat = stat;
            if (_stat == null) {
                return;
            }
            _text.text = _stat.ToString();
            _stat.OnStatChanged += RefreshText;
        }

        public void SetHoverDescr(System.Func<string> del) {
            _hoverDescr = del;
        }

        public void LevelUpStat() {
        }

        private void RefreshText(BaseStat stat) {
            _text.text = stat.ToString();
        }

        public void OnPointerEnter(PointerEventData eventData) {
            HoverStatus(true);
            if (_animator != null) {
                _animator.ResetTrigger(StringConst.ButtonNormalAnimName);
                _animator.SetTrigger(StringConst.ButtonSelectedAnimName);
            }
            if (_hoverDescr != null) {
                UITooltip.main.ShowToolTip(_hoverGraphic.rectTransform, _stat.Label, _hoverDescr());
            }
            
        }

        public void OnPointerExit(PointerEventData eventData) {
            HoverStatus(false);
            if (_animator != null) {
                _animator.ResetTrigger(StringConst.ButtonSelectedAnimName);
                _animator.SetTrigger(StringConst.ButtonNormalAnimName);
            }
            UITooltip.main.HideTooltip();
        }

        public virtual void HoverStatus(bool active) {
            if (_hoverGraphic == null) {
                return;
            }
            if (!active) {
                _hoverGraphic.color = _originalColor;
                return;
            }
            _hoverGraphic.color = _hoverColor;
        }
    }
}