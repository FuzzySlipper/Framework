using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace PixelComrades {
    [AutoRegister, Priority(Priority.Highest)]
    public sealed class PlayerInputSystem : SystemWithSingleton<PlayerInputSystem, PlayerInputComponent>, IMainSystemUpdate {

        private static bool _moveInputBlocked = false;
        public static bool MoveInputLocked { get { return _moveInputBlocked; } set { _moveInputBlocked = value; } }
        private static bool _allInputBlocked = false;
        public static bool AllInputBlocked {
            get { return _allInputBlocked; }
            set {
                _allInputBlocked = value;
                if (!value) {
                    if (LocalInput != null) {
                        LocalInput.LookInput = LocalInput.MoveInput = Vector2.zero;
                    }
                }
            }
        }

        public PlayerInputSystem() {
            MessageKit<bool>.addObserver(Messages.ApplicationFocus, ReceivedFocus);
        }

        private static event System.Action OnCancel;
        private static string _cancelTarget;
        private static RaycastHit[] _hits = new RaycastHit[10];
        
        private static IPlayerInputHandler LocalInput { get; set; }
        public static bool IsCursorOverUI { get { return LocalInput.IsCursorOverUI; } }
        public static Ray GetLookTargetRay { get { return LocalInput.GetLookTargetRay; } }
        public static Vector2 LookInput { get { return LocalInput?.LookInput ?? Vector2.zero; } }
        public static Vector2 MoveInput { get { return LocalInput?.MoveInput ?? Vector2.zero; } }
        public static Vector2 CursorPosition { get { return Mouse.current.position.ReadValue(); } }

        public static bool GetMouseButtonDown(int button) {
            if (button == 0) {
                return Mouse.current.leftButton.isPressed;
            }
            if (button == 1) {
                return Mouse.current.rightButton.isPressed;
            }
            if (button == 2) {
                return Mouse.current.middleButton.isPressed;
            }
            return false;
        }

        public static Vector3 GetMouseRaycastPosition(ActionConfig config) {
            return GetMouseRaycastPosition(DistanceSystem.FromUnitGridDistance(config.Range));
        }

        public static Vector3 GetMouseRaycastPosition(float range = 500) {
            var ray = GetLookTargetRay;
            var cnt = Physics.RaycastNonAlloc(ray, _hits, range, LayerMasks.DefaultCollision);
            _hits.SortByDistanceAsc(cnt);
            for (int i = 0; i < cnt; i++) {
                if (_hits[i].transform.CompareTag(StringConst.TagPlayer)) {
                    continue;
                }
                return _hits[i].point;
            }
            return ray.origin + (ray.direction * range);
        }

        public static bool AreMenusOpen() {
            if (IsCursorOverUI) {
                return true;
            }
            if (UIBasicMenu.OpenMenus.Count > 0) {
                return true;
            }
            if (UIModalQuestion.Active) {
                return true;
            }
            if (UISubMenu.Default.Active) {
                UISubMenu.Default.Disable();
            }
            return false;
        }

        public static bool GetButton(string button) {
            return LocalInput.GetButton(button);
        }
        public static bool GetButtonDown(string button) {
            return LocalInput.GetButtonDown(button);
        }

        public static bool GetKeyDown(Key button) {
            return LocalInput.GetKeyDown(button);
        }

        public static float GetAxis(string button) {
            return LocalInput.GetAxis(button);
        }

        protected override void SetCurrent(PlayerInputComponent current) {
            base.SetCurrent(current);
            LocalInput = current.Handler;
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            if (_allInputBlocked || LocalInput == null) {
                return;
            }
            CheckDebugInput();
            if (LocalInput.GetButtonDown(PlayerControls.Menu)) {
                if (OnCancel != null) {
                    CancelCurrent();
                }
                else if (UIBasicMenu.OpenMenus.Count > 0) {
                    UIBasicMenu.CloseAll();
                }
                else if (Game.GameActive) {
                    MessageKit.post(Messages.ToggleMainMenu);
                }
            }
            LocalInput.RunUpdate();
        }

        private void CheckDebugInput() {
            if (!Game.Debug) {
                return;
            }
            if (LocalInput.GetButtonDown(PlayerControls.ToggleFps)){
                UIFrameCounter.main.Toggle();
            }
            if (LocalInput.GetKeyDown(Key.O)) {
                EcsDebug.ScreenShot();
            }
            if (LocalInput.GetKeyDown(Key.L)) {
                Cursor.lockState = Game.CursorUnlocked ? CursorLockMode.None : CursorLockMode.Locked;
                UICursor.main.SetCursor(UICursor.CrossHair);
            }
            if (LocalInput.GetKeyDown(Key.Backquote)) {
                //SourceConsole.UI.ConsolePanelController.Singleton.Toggle();
                Console.Toggle();
            }
        }

        public static void SetCancelDel(System.Action onCancel, string target) {
            if (OnCancel != null) {
                OnCancel();
            }
            OnCancel = onCancel;
            _cancelTarget = target;
        }

        public static void ClearCancel(string targetName) {
            if (_cancelTarget == targetName) {
                OnCancel = null;
            }
        }

        private void CancelCurrent() {
            if (OnCancel != null) {
                OnCancel();
            }
            OnCancel = null;
            _cancelTarget = "";
        }

        private void ReceivedFocus(bool focusStatus) {
            if (!Game.GameStarted) {
                return;
            }
            if (!focusStatus) {
                Cursor.visible = true;
                return;
            }
            Cursor.visible = false;
            if (GameOptions.MouseLook) {
                Cursor.lockState = Game.CursorUnlocked ? CursorLockMode.None : CursorLockMode.Locked;
                UICursor.main.SetCursor(UICursor.CrossHair);
            }
        }

    }
}
