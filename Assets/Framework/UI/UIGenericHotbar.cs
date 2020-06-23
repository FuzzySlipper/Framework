using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class UIGenericHotbar : MonoSingleton<UIGenericHotbar> {

        [SerializeField] private Transform _grid = null;
        [SerializeField] private CanvasGroup _canvas = null;
        [SerializeField] private UIGenericButton _prefab = null;

        private List<UIGenericButton> _slots = new List<UIGenericButton>();
        private List<MenuAction> _menuActions;

        public event System.Action OnMenuDisabled;

        public bool Active { get; private set; }

        public void EnableMenu(List<MenuAction> request) {
            if (!Active) {
                SetStatus(true);
            }
            if (_menuActions != null) {
                MenuAction.Store(_menuActions);
            }
            _menuActions = request;
            GenerateNewList();
        }

        public void Disable() {
            SetStatus(false);
            if (_menuActions != null) {
                MenuAction.Store(_menuActions);
            }
            _menuActions = null;
        }

        private void ClearSlots() {
            for (int i = 0; i < _slots.Count; i++) {
                ItemPool.Despawn(_slots[i].gameObject);
            }
            _slots.Clear();
        }

        private void GenerateNewList() {
            ClearSlots();
            for (int i = 0; i < _menuActions.Count; i++) {
                var listItem = ItemPool.SpawnUIPrefab<UIGenericButton>(_prefab.gameObject, _grid.transform);
                listItem.Index = i;
                listItem.SetText(_menuActions[i].Description);
                listItem.SetIcon(_menuActions[i].Icon);
                listItem.OnButtonClicked += ListItemSelected;
                _slots.Add(listItem);
            }
            _canvas.SetActive(true);
        }

        private void ListItemSelected(int index) {
            if (_menuActions == null || index >= _menuActions.Count) {
                return;
            }
            if (!_menuActions[index].TryUse()) {
                _menuActions[index].OnFail(_slots[index].transform as RectTransform);
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
            _canvas.interactable = status;
            _canvas.blocksRaycasts = status;
            _canvas.FadeTo(status ? 1 : 0, 0.5f, EasingTypes.SinusoidalInOut, true);
            if (!status) {
                if (OnMenuDisabled != null) {
                    OnMenuDisabled();
                }
            }
        }
    }
}
