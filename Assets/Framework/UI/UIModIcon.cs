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
        private ModEntry? _watchedMod;
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
        }

        public void Assign(ModEntry mod) {
            _watchedMod = mod;
            if (_watchedMod != null) {
                _iconImage.overrideSprite = mod.Icon != null ? mod.Icon : _defaultIcon;
                UpdateCoolDown();
            }
        }

        public void UpdateCoolDown() {
            if (_watchedMod == null) {
                return;
            }
            _cooldownImage.fillAmount = _watchedMod.Value.PercentLeft;
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
                if (_watchedMod.Value.Target.HasComponent<PlayerComponent>()) {
                    World.Get<ModifierSystem>().RemoveStatMod(_watchedMod.Value.Id);
                }
            }
        }

        protected virtual void DisplayHoverData() {
            if (_watchedMod == null) {
                return;
            }
            UITooltip.main.ShowToolTip(_hoverGraphic, _watchedMod.Value.Icon, _watchedMod.Value.Label, _watchedMod.Value.Description);
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
                UnityEditor.Handles.Label(transform.position, _watchedMod.Value.Description);
            }
        }
#endif
        
    }
}
