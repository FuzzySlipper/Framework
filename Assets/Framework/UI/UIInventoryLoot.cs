using System;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace PixelComrades {
    public class UIInventoryLoot : UIBasicMenu {
        public static UIInventoryLoot Instance;

        [SerializeField] private UISimpleInventoryList _inventoryList = null;

        private System.Action _onClose;
        private System.Action _onTakeAll;

        public static void Open(ItemInventory inventory, Action del, Action del2) {
            if (Instance.Active) {
                Instance.SetStatus(false);
            }
            Instance._onClose = del;
            Instance._onTakeAll = del2;
            Instance._inventoryList.OnClickDel = Instance.ClickDel;
            Instance._inventoryList.SetInventory(inventory, "Loot");
            Instance._inventoryList.RefreshInventory();
            Instance.SetStatus(true);
        }

        protected void Awake() {
            Instance = this;
        }

        public void ClickDel(UISimpleGameDataButton button, PointerEventData.InputButton buttonEvent) {
            var item = button.Data as Entity;
            if (item != null) {
                Player.MainInventory.Add(item);
            }
        }

        public void TakeAll() {
            if (_onTakeAll != null) {
                _onTakeAll();
            }
        }

        protected override void OnStatusChanged(bool status) {
            base.OnStatusChanged(status);
            _inventoryList.SetSceneStatus(status);
        }

        public override void SetStatus(bool status) {
            base.SetStatus(status);
            if (!status) {
                if (_onClose != null) {
                    _onClose();
                }
                _onClose = null;
                _onTakeAll = null;
            }
        }
    }
}