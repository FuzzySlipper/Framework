using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIModIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPoolEvents, ISystemUpdate {

        [SerializeField] private Image _hoverGraphic = null;
        [SerializeField] private Color _hoverColor = Color.white;
        [SerializeField] private Image _iconImage = null;
        [SerializeField] private Sprite _defaultIcon = null;
        [SerializeField] private Image _cooldownImage = null;

        private Color _defaultColor;
        private ModEntry _watchedMod;
        private TimedModEntry _timedMod;
        private ScaledTimer _timer = new ScaledTimer(0.2f);
        public bool Unscaled { get { return false; } }

        public void OnSystemUpdate(float dt) {
            if (_watchedMod != null && !_timer.IsActive) {
                _timer.Activate();
                UpdateCoolDown();
            }

        }
        protected virtual void Awake() {
            if (_hoverGraphic != null) {
                _defaultColor = _hoverGraphic.color;
            }
        }

        public void OnPoolSpawned() {}

        public void OnPoolDespawned() {
            _watchedMod = null;
            _timedMod = null;
        }

        public void Assign(ModEntry mod) {
            _watchedMod = mod;
            if (_watchedMod != null) {
                _timedMod = _watchedMod as TimedModEntry;
                _iconImage.overrideSprite = mod.Icon != null ? mod.Icon : _defaultIcon;
                UpdateCoolDown();
            }
            else {
                _timedMod = null;
            }
        }

        public void UpdateCoolDown() {
            if (_timedMod == null) {
                _cooldownImage.fillAmount = 0;
                return;
            }
            _cooldownImage.fillAmount = _timedMod.PercentLeft;
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
                if (_watchedMod.Target.IsPlayer()) {
                    World.Get<ModifierSystem>().Remove(_watchedMod);
                }
            }
        }

        protected virtual void DisplayHoverData() {
            if (_watchedMod == null) {
                return;
            }
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
