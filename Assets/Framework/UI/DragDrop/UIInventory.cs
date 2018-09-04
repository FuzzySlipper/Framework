using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;


namespace PixelComrades {
    public class UIInventory : UIBasicMenu {

        [SerializeField] private TextMeshProUGUI _titleText = null;
        [SerializeField] private Transform _grid = null;
        
        private ItemInventory _inventory;
        private UIItemDragDrop[] _slots;

        protected Transform Grid { get { return _grid; } }

        void Update() {
            if (Active) {
                RefreshInventory();
            }
        }
        
        public override void SetStatus(bool status) {
            base.SetStatus(status);
            if (status == Active) {
                return;
            }
            if (status) {
                Game.CursorUnlock(name);
            }
            else {
                Game.RemoveCursorUnlock(name);
            }
        }

        public void SetInventory(ItemInventory inventory, string title) {
            ClearOld();
            _inventory = inventory;
            if (_titleText != null) {
                _titleText.text = title;
            }
            _slots = new UIItemDragDrop[_inventory.Limit < 0 ? _inventory.Count * 2 : _inventory.Limit];
            for (int i = 0; i < _slots.Length; i++) {
                _slots[i] = SpawnPrefab();
                _slots[i].Index = i;
                if (_inventory[i] != null) {
                    _slots[i].SetItem(_inventory[i]);
                }
            }
            _inventory.OnRefreshItemList += RefreshInventory;
        }

        protected virtual UIItemDragDrop SpawnPrefab() {
            return ItemPool.SpawnUIPrefab<UIItemDragDrop>(StringConst.ItemDragDrop, _grid);
        }

        protected void SetItemStatus(int index) {
            if (_inventory[index] == null) {
                _slots[index].Clear();
                return;
            }
            _slots[index].SetItem(_inventory[index]);
        }

        private void ClearOld() {
            ClearInventory();
            if (_inventory == null) {
                return;
            }
            _inventory.OnRefreshItemList -= RefreshInventory;
        }

        private void ClearInventory() {
            if (_slots != null) {
                for (int i = 0; i < _slots.Length; i++) {
                    ItemPool.Despawn(_slots[i].gameObject);
                }
                _slots = null;
            }
        }

        protected void RefreshInventory() {
            for (int i = 0; i < _slots.Length; i++) {
                if (_slots[i].Item != _inventory[i]) {
                    SetItemStatus(i);
                }
            }
        }

    }
}
