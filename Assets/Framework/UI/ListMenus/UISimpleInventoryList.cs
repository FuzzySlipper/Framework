using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;

namespace PixelComrades {
    public class UISimpleInventoryList : UIBaseActorPanel {

        public System.Action<UISimpleGameDataButton, PointerEventData.InputButton> OnClickDel;

        [SerializeField] private TextMeshProUGUI _titleText = null;
        [SerializeField] private Transform _grid = null;
        [SerializeField] private GameObject _prefab = null;
        [SerializeField] private UIDataDetailDisplay _info = null;
        
        private ItemInventory _inventory;
        private List<GameObject> _slots = new List<GameObject>();
        public Transform Grid { get { return _grid; } }

        protected override void OnStatusChanged(bool status) {
            base.OnStatusChanged(status);
            if (_info != null) {
                _info.SetActive(status);
            }
            if (status) {
                Game.CursorUnlock(name);
                if (_inventory != null) {
                    RefreshInventory();
                }
            }
            else {
                Clear();
                Game.RemoveCursorUnlock(name);
            }
        }

        public override void SetSceneStatus(bool status) {
            gameObject.SetActive(status);
            if (CanvasGroup != null) {
                CanvasGroup.SetActive(status);
            }
            Status = status;
            OnStatusChanged(status);
        }

        public void SetInventory(ItemInventory inventory, string title) {
            if (_inventory != null) {
                _inventory.OnRefreshItemList -= RefreshInventory;
            }
            _inventory = inventory;
            if (_titleText != null) {
                _titleText.text = title;
            }
            _inventory.OnRefreshItemList += RefreshInventory;
            //RefreshInventory();
        }

        public void Clear() {
            for (int i = 0; i < _slots.Count; i++) {
                ItemPool.Despawn(_slots[i].gameObject);
            }
            _slots.Clear();
        }

        public UISimpleGameDataButton AddItem(Entity item) {
            var prefab = ItemPool.SpawnUIPrefab<UISimpleGameDataButton>(_prefab, _grid);
            if (OnClickDel != null) {
                prefab.OnClickDel = OnClickDel;
            }
            prefab.SetData(item);
            prefab.Index = _slots.Count;
            _slots.Add(prefab.gameObject);
            return prefab;
        }

        public void RefreshInventory() {
            if (!Active) {
                return;
            }
            Clear();
            for (int i = 0; i < _inventory.Count; i++) {
                var item = _inventory[i];
                if (item == null) {
                    continue;
                }
                AddItem(item);
            }
        }
    }
}
