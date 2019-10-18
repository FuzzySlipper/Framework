using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIActionDisplay : MonoBehaviour, IReceive<ReadyActionsChanged>, IReceive<EntityDetailsChanged>,
        IReceive<StatusUpdate>, IPointerEnterHandler, IPointerExitHandler  {
        
        [SerializeField] private Image _iconDisplay = null;
        [SerializeField] private int _index = 0;
        [SerializeField] private TextMeshProUGUI _amount = null;
        [SerializeField] private TextMeshProUGUI _label = null;
        [SerializeField] private Slider _durabilitySlider = null;
        [SerializeField] private Image _cooldownImage = null;
        [SerializeField] private bool _postStatusUpdates = true;
        [SerializeField] private UIFloatingText.Orietation _textOrientation = UIFloatingText.Orietation.Center;
        
        private UnscaledTimer _statusTimer = new UnscaledTimer(0.25f);
        private Entity _entity;
        private TooltipComponent _tooltip;
        private IntValueHolder _currentAmmo;
        private bool _animatingCooldown = false;

        public void OnPointerEnter(PointerEventData eventData) {
            if (_entity == null) {
                return;
            }
            Game.DisplayData(_iconDisplay, _entity);
            if (_entity != null) {
                _entity.Post(new TooltipDisplaying(_tooltip));
            }
        }


        public void OnPointerExit(PointerEventData eventData) {
            UITooltip.main.HideTooltip();
        }
        
        public void Handle(ReadyActionsChanged arg) {
            if (arg.Index != _index) {
                return;
            }
            if (arg.Action == null) {
                Clear();
                return;
            }
            SetItem(arg.Action.Entity);
        }

        public void Handle(EntityDetailsChanged arg) {
            RefreshItem();
        }

        public void Handle(StatusUpdate arg) {
            if (_postStatusUpdates && gameObject != null && gameObject.activeInHierarchy) {
                StatusMessages(_entity, arg.Update);
            }
        }

        private void SetItem(Entity item) {
            CleanUpCurrentItem();
            _entity = item;
            _tooltip = item.Get<TooltipComponent>();
            _iconDisplay.sprite = item.Get<IconComponent>().Sprite;
            _iconDisplay.enabled = _iconDisplay.sprite != null;
            var ammo = item.Get<AmmoComponent>();
            if (ammo != null) {
                _currentAmmo = ammo.Amount;
                _currentAmmo.OnResourceChanged += CheckAmmo;
                CheckAmmo();
            }
            if (_cooldownImage != null) {
                item.AddObserver(EntitySignals.CooldownTimerChanged, CheckCooldown);
            }
            item.AddObserver(this);
            RefreshItem();
        }

        public void Clear() {
            CleanUpCurrentItem();
            DisableSlotDetails();
            _currentAmmo = null;
            _entity = null;
            _tooltip = null;
        }

        private void CheckCooldown() {
            if (_animatingCooldown || _cooldownImage == null) {
                return;
            }
            if (_entity.Get<CooldownComponent>().Cooldown?.Percent > 1.15f) {
                _cooldownImage.fillAmount = 1;
                return;
            }
            if (System.Math.Abs(_cooldownImage.fillAmount - _entity.Get<CooldownComponent>().Cooldown?.Percent ?? 0) > 0.1f) {
                TimeManager.StartUnscaled(UpdateCooldown());
            }
        }

        IEnumerator UpdateCooldown() {
            var coolDown = _entity.Get<CooldownComponent>().Cooldown;
            if (coolDown == null) {
                yield break;
            }
            _animatingCooldown = true;
            while (_entity != null) {
                if (System.Math.Abs(_cooldownImage.fillAmount - coolDown.Percent) < 0.1f) {
                    _cooldownImage.fillAmount = Mathf.Clamp(coolDown.Percent, 0, 1);
                    break;
                }
                var fillAmt = Mathf.MoveTowards(_cooldownImage.fillAmount, coolDown.Percent, 2f * TimeManager.DeltaUnscaled);
                _cooldownImage.fillAmount = Mathf.Clamp(fillAmt, 0, 1);
                yield return null;
            }
            if (_entity != null) {
                _cooldownImage.fillAmount = Mathf.Clamp(coolDown.Percent, 0, 1);
            }
            _animatingCooldown = false;
        }

        private void RefreshItem() {
            if (_entity == null) {
                Clear();
                return;
            }
            if (_label != null) {
                _label.text = _entity.Get<LabelComponent>()?.Text;
            }
            if (_cooldownImage != null) {
                CheckCooldown();
            }
        }

        private void StatusMessages(Entity item, string message) {
            if (_statusTimer.IsActive) {
                return;
            }
            _statusTimer.StartTimer();
            UIFloatingText.Spawn(message, transform as RectTransform, Color.yellow, _textOrientation);
        }
        
        public void CleanUpCurrentItem() {
            if (_entity == null) {
                return;
            }
            if (_cooldownImage != null) {
                World.Get<EntityEventSystem>().RemoveObserver(_entity, EntitySignals.CooldownTimerChanged, CheckCooldown);
            }
            _entity.RemoveObserver(this);
            if (_currentAmmo != null) {
                _currentAmmo.OnResourceChanged -= CheckAmmo;
            }
        }

        private void DisableSlotDetails() {
            _iconDisplay.enabled = false;
            if (_amount != null) {
                _amount.text = "";
            }
            if (_durabilitySlider != null) {
                _durabilitySlider.value = 0;
            }
            if (_label != null) {
                _label.text = "";
            }
        }

        private void CheckAmmo() {
            if (_durabilitySlider != null) {
                _durabilitySlider.value = _currentAmmo.CurrentPercent;
            }
            if (_amount != null) {
                _amount.text = _currentAmmo.Value.ToString();
            }
        }
    }
}
