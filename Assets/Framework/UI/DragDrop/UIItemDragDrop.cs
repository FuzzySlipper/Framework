using System;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIItemDragDrop : UIDragDrop, IReceive<ContainerStatusChanged>, IReceive<EntityDetailsChanged>, 
        IReceive<StatusUpdate>, IReceive<EquipmentChanged>, IPoolEvents {

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
        private IntValueHolder _currentAmmo;
        private TooltipComponent _tooltip;
        public InventoryItem InventoryItem { get; protected set; }
        
        public override void OnCreate(PrefabEntity entity) {
            base.OnCreate(entity);
            if (_label != null) {
                _minMainText = _label.fontSizeMin;
            }
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
            if (Data.Get<InventoryItem>()?.Inventory?.Owner.HasComponent<PlayerComponent>() ?? false) {
                UIDragDropHandler.SetItem(Data, StopDrag, StopDrag, Clear);
            }
            else {
                UIDragDropHandler.SetItem(Data, TryAddPlayer, StopDrag, Clear);
            }
            DisableSlotDetails();
        }

        private void TryAddPlayer() {
            UIDragDropHandler.ClearData();
            if (World.Get<ContainerSystem>().TryAdd(Player.MainInventory, Data)) {
                Clear();
            }
        }

        private void StopDrag() {
            UIDragDropHandler.ClearData();
            SetSpriteStatus(true);
            RefreshItem();
        }

        protected virtual void TryDrop() {
            var dragInventoryData = UIDragDropHandler.CurrentData.Get<InventoryItem>();
            if (dragInventoryData.Inventory == Player.MainInventory) {
                if (World.Get<ContainerSystem>().TrySwap(Player.MainInventory, dragInventoryData.Index, Index)) {
                    StatusMessages("TrySwap");
                    UIDragDropHandler.Take();
                }
                else {
                    StatusMessages("Rejected same inventory");
                    RejectDrag();
                }
                return;
            }
            if (World.Get<ContainerSystem>().TryAdd(Player.MainInventory, UIDragDropHandler.CurrentData, Index)) {
                StatusMessages("TryAdd)");
                UIDragDropHandler.Take();
            }
            else {
                StatusMessages("Rejected different inventory");
                RejectDrag();
            }
        }

        protected virtual void TrySwap() {
            if (InventorySystem.CanStack(InventoryItem, UIDragDropHandler.CurrentData)) {
                UIDragDropHandler.Take();
                return;
            }
            Entity oldItem = Data;
            var currentInventoryData = oldItem.Get<InventoryItem>();
            if (World.Get<ContainerSystem>().TryReplace(Player.MainInventory, UIDragDropHandler.CurrentData, currentInventoryData.Index)) {
                StatusMessages("World.Get<ContainerSystem>().TryReplace(Player.MainInventory, UIDragDropHandler.CurrentData, currentInventoryData.Index)");
                UIDragDropHandler.Take();
                UIDragDropHandler.SetItem(oldItem);
            }
            else {
                StatusMessages("Rejected different inventory");
                RejectDrag();
            }
        }

        protected void RejectDrag() {
            if (PlayAudio) {
                AudioPool.PlayClip(StringConst.AudioDefaultItemReturn, transform.position, 0, AudioVolume);
            }
            UIDragDropHandler.Return();
        }

        public override void Clear() {
            CleanUpCurrentItem();
            DisableSlotDetails();
            if (_label != null) {
                _label.fontSizeMin = _minMainText;
            }
            Data = null;
            _tooltip = null;
        }

        protected override void DisplayHoverData() {
            base.DisplayHoverData();
            if (Data != null && _tooltip != null) {
                Data.Post(new TooltipDisplaying(_tooltip));
            }
        }

        public void SetItem(Entity item) {
            CleanUpCurrentItem();
            if (item == null) {
                DisableSlotDetails();
                return;
            }
            Data = item;
            _tooltip = item.Get<TooltipComponent>();
            InventoryItem = Data.Get<InventoryItem>();
            SetSprite(item.Get<IconComponent>()?.Sprite);
            var ammo = item.Get<AmmoComponent>();
            if (ammo != null) {
                _currentAmmo = ammo.Amount;
                _currentAmmo.OnResourceChanged += CheckAmmo;
                CheckAmmo();
            }
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
                World.Get<EntityEventSystem>().RemoveObserver(Data, EntitySignals.CooldownTimerChanged, CheckCooldown);
            }
            Data.RemoveObserver(this);
            if (_currentAmmo != null) {
                _currentAmmo.OnResourceChanged -= CheckAmmo;
            }
            Data = null;
        }

        protected void StatusMessages(string message) {
            if (_statusTimer.IsActive) {
                return;
            }
            _statusTimer.StartTimer();
            UIFloatingText.Spawn(message, transform as RectTransform, Color.yellow, _textOrientation);
        }

        private void CheckAmmo() {
            if (_durabilitySlider != null) {
                _durabilitySlider.value = _currentAmmo.CurrentPercent;
            }
            if (_amount != null) {
                _amount.text = _currentAmmo.Value.ToString();
            }
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
                var fillAmt = Mathf.MoveTowards(_cooldownImage.fillAmount, coolDown.Percent, 2f * TimeManager.DeltaUnscaled);
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
            if (_postStatusUpdates && gameObject != null && gameObject.activeInHierarchy) {
                StatusMessages(arg.Update);
            }
        }

        public void Handle(EquipmentChanged arg) {
            RefreshItem();
        }

        public void OnPoolSpawned() {}

        public void OnPoolDespawned() {
            CleanUpCurrentItem();
        }
    }
}
