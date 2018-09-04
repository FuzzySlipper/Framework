using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIModIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPoolEvents {

        [SerializeField] private Image _hoverGraphic = null;
        [SerializeField] private Color _hoverColor = Color.white;
        [SerializeField] private Image _iconImage = null;
        [SerializeField] private Sprite _defaultIcon = null;
        [SerializeField] private Image _cooldownImage = null;

        private Color _defaultColor;
        private IEntityModifier _watchedMod;
        private CharacterNode _target;

        public IEntityModifier Mod { get { return _watchedMod; } }

        protected virtual void Awake() {
            if (_hoverGraphic != null) {
                _defaultColor = _hoverGraphic.color;
            }
        }

        public void OnPoolSpawned() {}

        public void OnPoolDespawned() {
            _watchedMod = null;
        }

        public void Assign(IEntityModifier mod, CharacterNode target) {
            _target = target;
            _watchedMod = mod;
            if (_watchedMod != null) {
                _iconImage.sprite = mod.Icon != null ? mod.Icon : _defaultIcon;
                UpdateCoolDown();
            }
        }

        public void UpdateCoolDown() {
            if (_watchedMod.TurnsLeft() == 0 || _watchedMod.TurnLength == 0) {
                _cooldownImage.fillAmount = 0;
                return;
            }
            if (_watchedMod.TurnStart == TurnBased.TurnNumber) {
                _cooldownImage.fillAmount = 1;
                return;
            }
            _cooldownImage.fillAmount = 1 - ((TurnBased.TurnNumber - _watchedMod.TurnStart) / _watchedMod.TurnLength);
        }

        public void HoverStatus(bool active) {
            if (_hoverGraphic == null) {
                return;
            }
            if (!active) {
                _hoverGraphic.color = _defaultColor;
                return;
            }
            _hoverGraphic.color = _hoverColor;
        }

        public virtual void OnPointerClick(PointerEventData eventData) {
            if (_watchedMod == null) {
                return;
            }
            if (eventData.button == PointerEventData.InputButton.Right) {
                if (_watchedMod.Owner.HasComponent<PlayerComponent>()) {
                    _watchedMod.Owner.Get<ModifiersContainer>(m => m.RemoveMod(_watchedMod.Id));
                }
            }
        }

        protected virtual void DisplayHoverData() {
            UITooltip.main.ShowToolTip(_hoverGraphic, _watchedMod.Icon, _watchedMod.Label, _watchedMod.Description);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            HoverStatus(true);
            if (_watchedMod == null) {
                return;
            }
            DisplayHoverData();
        }

        public void OnPointerExit(PointerEventData eventData) {
            HoverStatus(false);
            UITooltip.main.HideTooltip();
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected() {
            if (_watchedMod != null) {
                UnityEditor.Handles.Label(transform.position, _watchedMod.Description);
            }
        }
#endif
    }
}
