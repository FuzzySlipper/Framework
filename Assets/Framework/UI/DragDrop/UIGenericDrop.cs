using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelComrades {
    public abstract class UIGenericDrop : MonoBehaviour, IPointerClickHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler {

        [SerializeField] private Image _hoverGraphic = null;
        [SerializeField] private Color _hoverColor = Color.white;
        [SerializeField] private Animator _animator = null;

        private Color _defaultColor;

        protected abstract void OnDropEvent();

        protected virtual void Awake() {
            if (_hoverGraphic != null) {
                _defaultColor = _hoverGraphic.color;
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            HoverStatus(true);
            if (_animator != null) {
                _animator.ResetTrigger(StringConst.ButtonNormalAnimName);
                _animator.SetTrigger(StringConst.ButtonSelectedAnimName);
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

        public virtual void OnDrop(PointerEventData eventData) {
            OnDropEvent();
        }

        public virtual void OnPointerClick(PointerEventData eventData) {
            OnDropEvent();
        }

        public virtual void HoverStatus(bool active) {
            if (_hoverGraphic == null) {
                return;
            }
            if (!active) {
                _hoverGraphic.color = _defaultColor;
                return;
            }
            _hoverGraphic.color = _hoverColor;
        }
    }
}
