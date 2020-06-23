using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIDragDropHandler : MonoSingleton<UIDragDropHandler> {

        public const float DragTime = 1.5f;
        private const float MinActive = 0.2f;

        [SerializeField] private Image _dragDropSprite = null;
        //[SerializeField] private float _minSizeDrag = 75f;

        private static Entity _currentData;
        private static System.Action _onRightClick;
        private static System.Action _onReturn;
        private static System.Action _onTake;
        private static int _currentId;

        public static Entity CurrentData { get { return _currentData; } }
        public static bool Active { get { return CurrentData != null; } }
        public static bool IsManualDragging { get; set; }
        public static bool IsUiDragging { get; set; }
        public static float TimeStart { get; private set; }
        public static float TimeActive { get { return TimeManager.TimeUnscaled - TimeStart; } }
        public static bool Ready { get { return TimeActive > MinActive; } }
        public static bool CanDrag { get; set; } = true;

        void Update() {
            if (!Active) {
                return;
            }
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
                TimeManager.StartUnscaled(CheckForOrphan(_currentId));
            }
        }

        private IEnumerator CheckForOrphan(int checkId) {
            yield return 0.1f;
            if (Active && _currentId == checkId) {
                //Debug.Log("timeout activated");
                AudioPool.PlayClip(StringConst.AudioDefaultItemReturn, transform.position, 0, 0.5f);
                Return();
            }
        }

        public static bool TryRightClick() {
            if (_onRightClick != null) {
                _onRightClick();
                return true;
            }
            return false;
        }

        public static void Return() {
            if (_onReturn != null) {
                _onReturn();
            }
            ClearData();
        }

        public static void Take() {
            if (_onTake != null) {
                _onTake();
            }
            ClearData();
        }

        public static void ClearData() {
            _currentData = null;
            _onRightClick = null;
            _onReturn = null;
            _onTake = null;
            IsUiDragging = false;
            IsManualDragging = false;
            DisableDrag();
        }

        public static void SetItem(Entity item) {
            SetItem(item, AddItemToPlayer, AddItemToPlayer, null);
        }

        public static void AddItemToPlayer() {
            if (World.Get<ContainerSystem>().TryAdd(Player.MainInventory, CurrentData)) {
                Take();
            }
            else {
                ClearData();
            }
        }

        public static void SetItem(Entity item, System.Action onRightClick, System.Action onReturn, System.Action onTake) {
            _currentData = item;
            SetDragSprite(item.Get<IconComponent>());
            _onRightClick = onRightClick;
            _onReturn = onReturn;
            _onTake = onTake;
            TimeStart = TimeManager.TimeUnscaled;
            IsManualDragging = false;
            _currentId = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }

        public static void Set(Entity data, System.Action onRightClick, System.Action onReturn, System.Action onTake) {
            _currentData = data;
            SetDragSprite(data.Get<IconComponent>());
            _onRightClick = onRightClick;
            _onReturn = onReturn;
            _onTake = onTake;
            TimeStart = TimeManager.TimeUnscaled;
            IsManualDragging = false;
        }

        public static void SetDragSprite(Sprite sprite) {
            UITooltip.main.HideTooltipImmediate();
            UITooltip.CanActivate = false;
            main._dragDropSprite.overrideSprite = sprite;
            //main._dragDropSprite.rectTransform.sizeDelta = new Vector2(
            //    Mathf.Clamp(size.x, main._minSizeDrag, 999), Mathf.Clamp(size.y, main._minSizeDrag, 999));
            main._dragDropSprite.enabled = true;
        }

        public static void DisableDrag() {
            UITooltip.CanActivate = true;
            main._dragDropSprite.enabled = false;
        }
    }
}
