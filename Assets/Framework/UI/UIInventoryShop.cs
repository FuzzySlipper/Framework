using System;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;

namespace PixelComrades {
    public class UIInventoryShop : UIBasicMenu {
        public static UIInventoryShop Instance;

        [SerializeField] private UISimpleInventoryList _inventory = null;
        [SerializeField] private TextMeshProUGUI _toggleDisplay = null;
        [SerializeField] private TextMeshProUGUI _partyCurrency = null;

        private bool _buyingMode = true;
        private ItemInventory _sellingInventory;
        private UISimpleGameDataButton _currentButton;
        private System.Action _onClose;

        public static void Open(ItemInventory inventory, string title, System.Action del) {
            if (Instance.Active) {
                Instance.SetStatus(false);
            }
            Instance._onClose = del;
            Instance._sellingInventory = inventory;
            Instance.SetStatus(true);
            Instance.SetBuyingMode();
        }

        protected void Awake() {
            Instance = this;
        }

        protected override void OnStatusChanged(bool status) {
            base.OnStatusChanged(status);
            _inventory.SetSceneStatus(status);
            SetBuyingMode();
        }

        public override void SetStatus(bool status) {
            base.SetStatus(status);
            if (!status) {
                if (_onClose != null) {
                    _onClose();
                }
                _onClose = null;
                Player.DefaultCurrencyHolder.OnResourceChanged -= SetPartyCurrency;
            }
            else {
                SetPartyCurrency();
                Player.DefaultCurrencyHolder.OnResourceChanged += SetPartyCurrency;
            }
        }

        private void SetPartyCurrency() {
            _partyCurrency.text = string.Format("Party {0}: {1}", GameText.DefaultCurrencyLabel, Player.DefaultCurrencyHolder.Value);
        }

        public void ToggleMode() {
            if (_buyingMode) {
                SetSellingMode();
            }
            else {
                SetBuyingMode();
            }
        }

        private void FloatingText(RectTransform rectTr, string text) {
            UIFloatingText.Spawn(text, rectTr, Color.red, UIFloatingText.Orietation.Center);
        }

        private void SetModeText() {
            _toggleDisplay.text = _buyingMode ? "Buying" : "Selling";
        }

        private void SetBuyingMode() {
            _buyingMode = true;
            _inventory.OnClickDel = BuyClickDel;
            _inventory.SetInventory(_sellingInventory, "Buying");
            _inventory.RefreshInventory();
            SetModeText();
        }

        private void BuyClickDel(UISimpleGameDataButton button, PointerEventData.InputButton buttonEvent) {
            var item = button.Data.Get<InventoryItem>();
            if (item == null) {
                return;
            }
            if (Player.MainInventory.IsFull) {
                FloatingText(button.RectTransform, "Inventory full");
                return;
            }
            var sellPrice = RuleSystem.TotalPrice(item);
            if (Player.DefaultCurrencyHolder.Value < sellPrice) {
                FloatingText(button.RectTransform, string.Format("Costs {0}, Not enough {1}", sellPrice, GameText.DefaultCurrencyLabel));
                
                return;
            }
            _currentButton = button;
            UIModalQuestion.Set(CheckBuy, string.Format("Buy for {0} {1}?", sellPrice, GameText.DefaultCurrencyLabel));
        }

        private void CheckBuy(int index) {
            if (index > 0) {
                _currentButton = null;
                return;
            }
            var item = _currentButton.Data.Get<InventoryItem>();
            if (item == null) {
                _currentButton = null;
                return;
            }
            var sellPrice = RuleSystem.TotalPrice(item);
            if (Player.MainInventory.TryAdd(item.GetEntity())) {
                Player.DefaultCurrencyHolder.ReduceValue(sellPrice);
                FloatingText(_currentButton.RectTransform, string.Format("Bought for {0} {1}", sellPrice, GameText.DefaultCurrencyLabel));
            }
            _currentButton = null;
        }

        private void SetSellingMode() {
            _buyingMode = false;
            _inventory.OnClickDel = SellClickDel;
            _inventory.SetInventory(Player.MainInventory, "Selling");
            _inventory.RefreshInventory();
            SetModeText();
        }

        private void SellClickDel(UISimpleGameDataButton button, PointerEventData.InputButton buttonEvent) {
            var item = button.Data.Get<InventoryItem>();
            if (item == null) {
                return;
            }
            if (Player.MainInventory.IsFull) {
                FloatingText(button.RectTransform, "Inventory full");
                return;
            }
            var price = GameOptions.PriceEstimateSell(item.GetEntity());
            _currentButton = button;
            UIModalQuestion.Set(CheckSell, string.Format("Sell for {0} {1}?", price, GameText.DefaultCurrencyLabel));
        }

        private void CheckSell(int index) {
            if (index > 0) {
                _currentButton = null;
                return;
            }
            var item = _currentButton.Data.Get<InventoryItem>();
            if (item == null) {
                _currentButton = null;
                return;
            }
            var price = GameOptions.PriceEstimateSell(item.GetEntity());
            FloatingText(_currentButton.RectTransform, string.Format("Sold for {0} {1}", price, GameText.DefaultCurrencyLabel));
            Player.DefaultCurrencyHolder.AddToValue(price);
            _sellingInventory.TryAdd(item.GetEntity());
            //item.Despawn(); better to be able to buy back
            _currentButton = null;
        }
    }
}