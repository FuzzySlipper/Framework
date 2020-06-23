using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;


namespace PixelComrades {
    public class UIInventory : UIBasicMenu, IReceive<ContainerChanged> {

        [SerializeField] private TextMeshProUGUI _titleText = null;
        [SerializeField] private Transform _grid = null;
        
        private ItemInventory _inventory;
        private UIItemDragDrop[] _slots;

        protected Transform Grid { get { return _grid; } }

        //void Update() {
        //    if (Active) {
        //        RefreshInventory();
        //    }
        //}
        
        //public override void SetStatus(bool status) {
        //    base.SetStatus(status);
        //    if (status == Active) {
        //        return;
        //    }
        //    if (status) {
        //        Game.CursorUnlock(name);
        //    }
        //    else {
        //        Game.RemoveCursorUnlock(name);
        //    }
        //}

        

        public void SetInventory(ItemInventory inventory, string title) {
            ClearOld();
            _inventory = inventory;
            if (_titleText != null) {
                _titleText.text = title;
            }
            if (_inventory == null) {
                return;
            }
            //_slots = new UIItemDragDrop[_inventory.Limit < 0 ? _inventory.Count : _inventory.Limit];
            ClearSlots();
            _slots = new UIItemDragDrop[_inventory.Max];
            for (int i = 0; i < _slots.Length; i++) {
                IconLoader.New(this, _inventory[i], i);
            }
            _inventory.Owner.AddObserver(this);
        }
        
        

        private class IconLoader : LoadOperationEvent {
            private static GenericPool<IconLoader> _pool = new GenericPool<IconLoader>(5);
<<<<<<< HEAD

            private Entity _item;
            private int _index;
            private UIInventory _inventory;
                
            public static void New(UIInventory inventory, Entity item, int index) {
                var loader = _pool.New();
                loader.SourcePrefab = LazyDb.Main.ItemDragDrop;
                loader._inventory = inventory;
                loader._item = item;
                loader._index = index;
            }

=======

            private Entity _item;
            private int _index;
            private UIInventory _inventory;
                
            public static void New(UIInventory inventory, Entity item, int index) {
                var loader = _pool.New();
                loader.SourcePrefab = LazyDb.Main.ItemDragDrop;
                loader._inventory = inventory;
                loader._item = item;
                loader._index = index;
            }

>>>>>>> FirstPersonAction
            public override void OnComplete() {
                var itemDragDrop = NewPrefab.GetComponent<UIItemDragDrop>();
                itemDragDrop.RectTransform.SetParent(_inventory._grid);
                itemDragDrop.Index = _index;
                itemDragDrop.SetItem(_item);
                _inventory._slots[_index] = itemDragDrop;
                
                _item = null;
                _inventory = null;
                SourcePrefab = null;
                NewPrefab = null;
                _pool.Store(this);
            }
        }

        protected void SetItemStatus(int index) {
            var item = _inventory[index];
            if (item == null) {
                _slots[index].Clear();
                return;
            }
            _slots[index].SetItem(item);
        }

        private void ClearOld() {
            if (_inventory != null) {
                _inventory.Owner.RemoveObserver(this);
            }
        }

        private void ClearSlots() {
            if (_slots == null) {
                return;
            }
            for (int i = 0; i < _slots.Length; i++) {
                if (_slots[i] == null) {
                    continue;
                }
                ItemPool.Despawn(_slots[i].gameObject);
            }
            _slots = null;
        }

        public void Clear() {
            ClearOld();
            ClearSlots();
        }

        protected void RefreshInventory() {
            for (int i = 0; i < _slots.Length; i++) {
                if (_slots[i] == null) {
                    continue;
                }
                if (_slots[i].Data != _inventory[i]) {
                    SetItemStatus(i);
                }
            }
        }

        public void Handle(ContainerChanged arg) {
            RefreshInventory();
        }
    }
}
