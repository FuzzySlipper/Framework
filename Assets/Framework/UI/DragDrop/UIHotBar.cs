using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelComrades {
    public class UIHotBar : MonoSingleton<UIHotBar> {

        public static void UseSlot(int index) {
            main._slots[index].UseSlot();
        }

        public static bool AddToHotbar(Entity item) {
            for (int i = 0; i < main._slots.Length; i++) {
                if (main._slots[i].Data == null) {
                    main._slots[i].Set(item);
                    return true;
                }
            }
            return false;
        }

        public static bool HotbarContains(Entity item) {
            for (int i = 0; i < main._slots.Length; i++) {
                if (main._slots[i].Data == item) {
                    return true;
                }
            }
            return false;
        }

        public static bool RemoveFromHotbar(Entity item) {
            for (int i = 0; i < main._slots.Length; i++) {
                if (main._slots[i].Data == item) {
                    main._slots[i].Clear();
                    return true;
                }
            }
            return false;
        }


        [SerializeField] private Transform _grid = null;
        [SerializeField] private CanvasGroup _canvasgroup = null;
        [SerializeField] private bool _startDisabled = false;
        [SerializeField] private int _size = 9;
        [SerializeField] private UIHotbarSlot _prefab;

        private UIHotbarSlot[] _slots;
        private ItemInventory _playerInventory;

        public bool Active { get; private set; }

        void Awake() {
            SetupSlots();
            if (_startDisabled) {
                SetStatus(false, true);
            }
            MessageKit.addObserver(Messages.PlayerNewGame, ClearHotbar);
        }

        private void ClearHotbar() {
            for (int i = 0; i < _slots.Length; i++) {
                _slots[i].Clear();
            }
            if (_playerInventory != null) {
                _playerInventory.OnRefreshItemList -= CheckSlots;
            }
            _playerInventory = Player.MainInventory;
            if (_playerInventory != null) {
                _playerInventory.OnRefreshItemList += CheckSlots;
            }
        }

        public void ToggleActive() {
            SetStatus(!Active);
        }
        
        public void SetStatus(bool status, bool overrideCheck = false) {
            if (!overrideCheck && status == Active) {
                return;
            }
            Active = status;
            _canvasgroup.interactable = status;
            _canvasgroup.blocksRaycasts = status;
            _canvasgroup.FadeTo(status ? 1 : 0, 0.5f, EasingTypes.SinusoidalInOut, true);
        }

        private void SetupSlots() {
            _slots = new UIHotbarSlot[_size];
            for (int i = 0; i < _slots.Length; i++) {
                _slots[i] = ItemPool.SpawnUIPrefab<UIHotbarSlot>(_prefab.gameObject, _grid);
                _slots[i].SetIndex(i);
            }
        }

        private void CheckSlots() {
            for (int i = 0; i < _slots.Length; i++) {
                if (!_slots[i].Active) {
                    continue;
                }
                var item = _slots[i].Data.Get<InventoryItem>();
                if (item.Count < 1) {
                    _slots[i].Clear();
                }
            }
        }


        public void CheckForDuplicates(UIHotbarSlot origin) {
            for (int s = 0; s < _slots.Length; s++) {
                if (!_slots[s].Active) {
                    continue;
                }
                var slot = _slots[s];
                if (slot.Active && slot.Data == null) {
                    slot.Clear();
                    continue;
                }
                for (int i = 0; i < _slots.Length; i++) {
                    if (i == s || !_slots[i].Active) {
                        continue;
                    }
                    var otherSlot = _slots[i];
                    if (otherSlot.Active && otherSlot.Data == null) {
                        otherSlot.Clear();
                        continue;
                    }
                    if (!slot.Data.Equals(otherSlot.Data)) {
                        continue;
                    }
                    if (origin == otherSlot) {
                        slot.Clear();
                    }
                    else {
                        otherSlot.Clear();
                    }
                    break;
                }
            }
        }
    }
}
