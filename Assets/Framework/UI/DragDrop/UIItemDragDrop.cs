using System;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIItemDragDrop : UIDragDrop, IReceive<ContainerStatusChanged>, IReceive<EntityDetailsChanged>, IReceive<StatusUpdate>, IReceive<EquipmentChanged> {

        [SerializeField] private UIFloatingText.Orietation _textOrientation = UIFloatingText.Orietation.Center;
        [SerializeField] private TextMeshProUGUI _amount = null;
        [SerializeField] private Image _cooldownImage = null;
        [SerializeField] private Slider _durabilitySlider = null;
        [SerializeField] private TextMeshProUGUI _label = null;
        [SerializeField] private bool _postStatusUpdates = true;
        [SerializeField] protected bool PlayAudio = true;

        private bool _animatingCooldown = false;
        private UnscaledTimer _statusTimer = new UnscaledTimer(0.25f);
        private float _minMainText = 3;
        public InventoryItem InventoryItem { get; protected set; }
        
        public override void OnCreate(PrefabEntity entity) {
            base.OnCreate(entity);
            _minMainText = _label.fontSizeMin;
        }

        public override void OnDrop(PointerEventData eventData) {
            if (UIDragDropHandler.CurrentData == null) {
                if (UIDragDropHandler.Active) {
                    if (PlayAudio) {
                        AudioPool.PlayClip(StringConst.AudioDefaultItemReturn, transform.position, 0, AudioVolume);
                    }
                    UIDragDropHandler.Return();
                }
                return;
            }
            if (PlayAudio) {
                AudioPool.PlayClip(StringConst.AudioDefaultItemClick, transform.position, 0, AudioVolume);
            }
            if (Data != null) {
                TrySwap();
            }
            else {
                TryDrop();
            }
        }

        public override void OnPointerClick(PointerEventData eventData) {
            //if (Game.InCombat) {
            //    UIFloatingText.Spawn("Can't adjust items during combat", transform as RectTransform, Color.yellow, UIFloatingText.Orietation.Center);
            //    return;
            //}
            if (eventData.button == PointerEventData.InputButton.Left) {
                if (UIDragDropHandler.Active) {
                    OnDrop(null);
                }
                else if (Data != null) {
                    StartDrag();
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right && Data != null) {
                Data.Get<UsableComponent>()?.TrySecondary(this);
            }
            //else if (Item != null && (eventData.button == PointerEventData.InputButton.Right || !_isPrimary)) {
            //    Item.TryUse(null, RectTransform.position, false);
            //}
        }

        protected override void StartDrag() {
            base.StartDrag();
            if (PlayAudio) {
                AudioPool.PlayClip(StringConst.AudioDefaultItemClick, transform.position, 0, AudioVolume);
            }
            if (Data.Get<InventoryItem>()?.Inventory?.GetEntity().HasComponent<PlayerComponent>() ?? false) {
                UIDragDropHandler.SetItem(Data, StopDrag, StopDrag, Clear);
            }
            else {
                UIDragDropHandler.SetItem(Data, TryAddPlayer, StopDrag, Clear);
            }
            DisableSlotDetails();
        }

        private void TryAddPlayer() {
            UIDragDropHandler.ClearData();
            if (Player.MainInventory.TryAdd(Data)) {
                Clear();
            }
        }

        private void StopDrag() {
            UIDragDropHandler.ClearData();
            SetSpriteStatus(true);
            RefreshItem();
        }

        protected virtual void TryDrop() {
            if (Player.MainInventory.TryAdd(UIDragDropHandler.CurrentData)) {
                UIDragDropHandler.Take();
            }
            else {
                if (PlayAudio) {
                    AudioPool.PlayClip(StringConst.AudioDefaultItemReturn, transform.position, 0, AudioVolume);
                }
                UIDragDropHandler.Return();
            }
        }

        protected virtual void TrySwap() {
            if (InventoryItem.CanStack(UIDragDropHandler.CurrentData)) {
                UIDragDropHandler.Take();
                return;
            }
            Entity oldItem = Data;
            UIDragDropHandler.CurrentData.Get<InventoryItem>(i => i.Index = Index);
            if (Player.MainInventory.TryAdd(UIDragDropHandler.CurrentData)) {
                UIDragDropHandler.Take();
                UIDragDropHandler.SetItem(oldItem);
            }
            else {
                if (PlayAudio) {
                    AudioPool.PlayClip(StringConst.AudioDefaultItemReturn, transform.position, 0, AudioVolume);
                }
                UIDragDropHandler.Return();
                Player.MainInventory.TryAdd(oldItem);
            }
        }

        public override void Clear() {
            CleanUpCurrentItem();
            DisableSlotDetails();
            _label.fontSizeMin = _minMainText;
            Data = null;

        }

        protected override void DisplayHoverData() {
            base.DisplayHoverData();
            if (Data != null) {
                Data.Get<TooltipComponent>()?.Tooltip();
            }
        }

        public void SetItem(Entity item) {
            CleanUpCurrentItem();
            if (item == null) {
                DisableSlotDetails();
                return;
            }
            Data = item;
            InventoryItem = Data.Get<InventoryItem>();
            SetSprite(item.Get<IconComponent>());
            //if (_durabilitySlider != null) {
            //    _durabilitySlider.value = Item.Durability.CurrentPercent;
            //    Item.Durability.OnStatChanged += UpdateDurability;
            //}
            if (_cooldownImage != null) {
                Data.AddObserver(EntitySignals.CooldownTimerChanged, CheckCooldown);
            }
            Data.AddObserver(this);
            CheckContent(Data);
        }

        private void DisableSlotDetails() {
            SetSpriteStatus(false);
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

        public void CleanUpCurrentItem() {
            if (Data == null) {
                return;
            }
            if (_cooldownImage != null) {
                Data.RemoveObserver(EntitySignals.CooldownTimerChanged, CheckCooldown);
            }
            Data.RemoveObserver(this);
            //if (_durabilitySlider != null && Item != null) {
            //    Item.Durability.OnStatChanged -= UpdateDurability;
            //}
            Data = null;
        }

        protected void StatusMessages(Entity item, string message) {
            if (_statusTimer.IsActive) {
                return;
            }
            _statusTimer.StartTimer();
            UIFloatingText.Spawn(message, transform as RectTransform, Color.yellow, _textOrientation);
        }

        private void UpdateDurability(BaseStat stat) {
            _durabilitySlider.value = ((VitalStat)stat).CurrentPercent;
        }

        private void CheckContent(Entity item) {
            RefreshItem();
        }

        private void RefreshItem() {
            if (Data == null) {
                DisableSlotDetails();
                return;
            }
            //if (_amount != null) {
            //    _amount.text = Item.IconAmount;
            //}
            if (_label != null) {
                _label.text = Data.Get<LabelComponent>()?.Text;
            }
            if (_cooldownImage != null) {
                CheckCooldown();
            }
        }

        private void CheckCooldown() {
            if (_animatingCooldown || _cooldownImage == null) {
                return;
            }
            if (Data.Get<CooldownComponent>().Cooldown?.Percent > 1.15f) {
                _cooldownImage.fillAmount = 1;
                return;
            }
            if (System.Math.Abs(_cooldownImage.fillAmount - Data.Get<CooldownComponent>().Cooldown?.Percent ?? 0) > 0.1f) {
                TimeManager.StartUnscaled(UpdateCooldown());
            }
        }

        IEnumerator UpdateCooldown() {
            var coolDown = Data.Get<CooldownComponent>().Cooldown;
            if (coolDown == null) {
                yield break;
            }
            _animatingCooldown = true;
            while (Data != null) {
                if (System.Math.Abs(_cooldownImage.fillAmount - coolDown.Percent) < 0.1f) {
                    _cooldownImage.fillAmount = Mathf.Clamp(coolDown.Percent, 0, 1);
                    break;
                }
                var fillAmt = Mathf.MoveTowards(_cooldownImage.fillAmount, coolDown.Percent, 2f * Time.deltaTime);
                _cooldownImage.fillAmount = Mathf.Clamp(fillAmt, 0, 1);
                yield return null;
            }
            if (Data != null) {
                _cooldownImage.fillAmount = Mathf.Clamp(coolDown.Percent, 0, 1);
            }
            _animatingCooldown = false;
        }

        public void Handle(ContainerStatusChanged arg) {
            RefreshItem();
        }

        public void Handle(EntityDetailsChanged arg) {
            RefreshItem();
        }

        public void Handle(StatusUpdate arg) {
            if (_postStatusUpdates) {
                StatusMessages(Data, arg.Update);
            }
        }

        public void Handle(EquipmentChanged arg) {
            RefreshItem();
        }
    }
}
