using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PixelComrades {
    public class UISubMenu : MonoBehaviour {

        public static UISubMenu Default { get; private set; }
        private int _currentId;

        public event System.Action OnMenuDisabled;

        [SerializeField] private UIGenericButton _buttonPrefab = null;
        [SerializeField] private VerticalLayoutGroup _grid = null;
        [SerializeField] private CanvasGroup _canvas = null;
        //[SerializeField] private Canvas _topCanvas = null;

        private UIGenericButton[] _items;
        private List<MenuAction> _menuActions;
        private Vector2 _targetPos;
        private RectTransform _rectTr;

        public bool Active { get { return _canvas.alpha > 0; } }

        void Awake() {
            _rectTr = transform as RectTransform;
            if (Default == null) {
                Default = this;
            }
        }
        //
        // void Update() {
        //     if (!Active) {
        //         return;
        //     }
        //     bool isDown = Mouse.current.leftButton.isPressed || Mouse.current.rightButton.isPressed;
        //     if (isDown) {
        //         TimeManager.StartUnscaled(CheckForOrphan(_currentId));
        //     }
        // }
        //
        // private IEnumerator CheckForOrphan(int checkId) {
        //     yield return 0.1f;
        //     if (Active && _currentId == checkId) {
        //         Disable();
        //     }
        // }

        public void EnableMenu(Vector2 pos, List<MenuAction> request) {
            _menuActions = request;
            _targetPos = pos;
            _rectTr.position = _targetPos;
            GenerateNewList();
        }

        public void Disable() {
            UITooltip.CanActivate = true;
            _canvas.alpha = 0;
            _canvas.interactable = false;
            _canvas.blocksRaycasts = false;
            if (OnMenuDisabled != null) {
                OnMenuDisabled();
            }
            if (_menuActions != null) {
                for (int i = 0; i < _menuActions.Count; i++) {
                    MenuAction.Store(_menuActions[i]);
                }
            }
            if (_items != null) {
                for (int i = 0; i < _items.Length; i++) {
                    ItemPool.Despawn(_items[i].gameObject);
                }
            }
            _items = null;
            _menuActions = null;
        }

        private void GenerateNewList() {
            if (_items != null) {
                for (int i = 0; i < _items.Length; i++) {
                    ItemPool.Despawn(_items[i].gameObject);
                }
            }
            _items = new UIGenericButton[_menuActions.Count];
            for (int i = 0; i < _menuActions.Count; i++) {
                var listItem = ItemPool.SpawnUIPrefab<UIGenericButton>(_buttonPrefab.gameObject, _grid.transform);
                listItem.Index = i;
                listItem.SetText(_menuActions[i].Description);
                listItem.OnButtonClicked += ListItemSelected;
                _items[i] = listItem;
            }
            _canvas.alpha = 1;
            _canvas.interactable = true;
            _canvas.blocksRaycasts = true;
            UITooltip.main.HideTooltipImmediate();
            UITooltip.CanActivate = false;
            _currentId = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            //_grid.CalculateLayoutInputHorizontal();
            //_grid.CalculateLayoutInputVertical();
            //if (Mathf.Abs(_targetPos.y) + _rectTr.sizeDelta.y > _topCanvas.pixelRect.height) {
            //    var diff = (Mathf.Abs(_targetPos.y) + _rectTr.sizeDelta.y) - _topCanvas.pixelRect.height;
            //    _targetPos.y -= diff;
            //}
            //if (Mathf.Abs(_targetPos.x) + _rectTr.sizeDelta.x > _topCanvas.pixelRect.height) {
            //    var diff = (Mathf.Abs(_targetPos.y) + _rectTr.sizeDelta.y) - _topCanvas.pixelRect.height;
            //    _targetPos.y -= diff;
            //}
        }

        private void ListItemSelected(int index) {
            if (_menuActions == null || index >= _menuActions.Count || _menuActions[index].Del == null) {
                Disable();
                return;
            }
            if (!_menuActions[index].Del()) {
                if (_menuActions[index].OnFail != null) {
                    _menuActions[index].OnFail(_items[index].transform as RectTransform);
                }
            }
            Disable();
        }
    }
}