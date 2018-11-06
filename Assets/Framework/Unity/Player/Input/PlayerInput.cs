using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Rewired.Integration.UnityUI;
using UnityEngine.EventSystems;

namespace PixelComrades {
    public class PlayerInput : MonoSingleton<PlayerInput> {

        private static bool _moveInputBlocked = false;
        public static bool MoveInputLocked { get { return _moveInputBlocked; } set { _moveInputBlocked = value; } }
        private static bool _allInputBlocked = false;
        public static bool AllInputBlocked { get { return _allInputBlocked; } set { _allInputBlocked = value; } }


        private event Action OnCancel;
        private string _cancelTarget;
        protected Rewired.Player InputSystem;

        public static Rewired.Player RewiredPlayer { get { return main.InputSystem; } }
        public static bool PauseInMenus { get; set; }
        public static Vector2 LookInput { get; set; }
        public static Vector2 MoveInput { get; set; }
        public static Ray GetTargetRay { get { return Player.Cam.ScreenPointToRay(Input.mousePosition); } }

        public static bool IsCursorOverUI {
            get {
                if (EventSystem.current.IsPointerOverGameObject()) {
                    if (UIDropWorldPanel.Active) {
                        return false;
                    }
                    //if (CurrentInput != null && CurrentInput. == UIDropWorldPanel.main.gameObject) {
                    //    return false;
                    //}
                    return true;
                }
                return false;
            }
        }

        public static Ray GetCenterRay { get { return Player.Cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, -0.25f)); } }

        private static RewiredStandaloneInputModule _currentInput;
        public static RewiredStandaloneInputModule CurrentInput {
            get {
                if (_currentInput == null) {
                    _currentInput = EventSystem.current.currentInputModule as RewiredStandaloneInputModule;
                    if (_currentInput == null) {
                        Debug.LogErrorFormat("Missing RewiredStandaloneInputModule has {0}", EventSystem.current.currentInputModule != null ? EventSystem.current.currentInputModule.GetType().Name : "null");
                    }
                }
                return _currentInput;
            }
        }

        protected virtual void Awake() {
            //MessageKit.addObserver(Messages.MenuStatusChanged, CheckForOpenMenus);
            InputSystem = Rewired.ReInput.players.GetPlayer(0);

        }


        void Update() {
            if (AllInputBlocked) {
                return;
            }
            CheckDebugInput();
            MenuInput();
            if (GetKeyDown(BaseControls.Menu)) {
                if (OnCancel != null) {
                    CancelCurrent();
                }
                else if (UIBasicMenu.OpenMenus.Count > 0) {
                    UIBasicMenu.CloseAll();
                }
                else if (Game.GameActive) {
                    UIMainMenu.main.Toggle();
                }
            }
            if (!Game.GameActive) {
                return;
            }
            GameplayInput();
            ActionInput();
        }

        protected virtual void GameplayInput() {
            if (!Game.GameActive) {
                return;
            }
            MoveInput = new Vector2(InputSystem.GetAxisRaw(Axis.MoveX), InputSystem.GetAxisRaw(Axis.MoveY));
            LookInput = new Vector2(InputSystem.GetAxisRaw(Axis.LookX), InputSystem.GetAxisRaw(Axis.LookY));
        }


        protected virtual void ActionInput() {
            if (!Game.GameActive) {
                return;
            }
            if (UIRadialMenu.Active) {
                for (int i = 0; i < _numericKeys.Length; i++) {
                    if (Input.GetKeyDown(_numericKeys[i])) {
                        UIRadialMenu.Confirm(i);
                    }
                }
                return;
            }
            if (InputSystem.GetButtonDown(BaseControls.Use)) {
                if (!WorldControlMonitor.Use()) {
                    UICenterButton.TryClickEvent();
                }
            }

        }

        protected virtual void MenuInput() {
            if (InputSystem.GetButtonDown(BaseControls.Map)) {
                UIMap.main.ToggleActive();
            }
        }

        //protected virtual void CheckForOpenMenus() {
        //if (Game.OpenMenu != null) {
        //    if (PauseInMenus && Game.OpenMenu.Active) {
        //        Game.Pause("OpenMenu");
        //    }
        //}
        //else {

        //}
        //if (PauseInMenus) {
        //    Game.RemovePause("OpenMenu");
        //}
        //}

        void OnApplicationFocus(bool focusStatus) {
            ReceivedFocus(focusStatus);
        }

        protected virtual void ReceivedFocus(bool focusStatus) {
            if (!focusStatus) {
                Cursor.visible = true;
                return;
            }
            Cursor.visible = false;
            if (GameOptions.MouseLook) {
                Cursor.lockState = Game.CursorUnlocked ? CursorLockMode.None : CursorLockMode.Locked;
            }
        }

        private void CancelCurrent() {
            if (OnCancel != null) {
                OnCancel();
            }
            OnCancel = null;
            _cancelTarget = "";
        }

        private void CheckDebugInput() {
            if (!Game.Debug) {
                return;
            }
            if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown(KeyCode.F)) {
                UIFrameCounter.main.Toggle();
            }
            if (Input.GetKeyDown(KeyCode.P)) {
                ScreenCapture.CaptureScreenshot(
                    string.Format(
                        "Screenshots/{0}-{1:MM-dd-yy hh-mm-ss}.png",
                        Game.Title, DateTime.Now));
            }
        }

        public static void SetCancelDel(Action onCancel, string target) {
            if (main.OnCancel != null) {
                main.OnCancel();
            }
            main.OnCancel = onCancel;
            main._cancelTarget = target;
        }

        public static void ClearCancel(string targetName) {
            if (main._cancelTarget == targetName) {
                main.OnCancel = null;
            }
        }

        public static bool GetKeyDown(string action) {
            return main.InputSystem.GetButtonDown(action);
        }

        public static bool GetKey(string action) {
            return main.InputSystem.GetButton(action);
        }

        public static bool GetKeyUp(string action) {
            return main.InputSystem.GetButtonUp(action);
        }

        public static float GetAxis(string axis) {
            return main.InputSystem.GetAxis(axis);
        }

        public static float GetAxisRaw(string axis) {
            return main.InputSystem.GetAxisRaw(axis);
        }

        protected static KeyCode[] _numericKeys = new[] {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0,
        };

        public static class Axis {
            public const string MoveX = "MoveX";
            public const string MoveY = "MoveY";
            public const string LookX = "LookX";
            public const string LookY = "LookY";
            public const string Scroll = "ScrollAxis";
        }

        public static class BaseControls {
            public static string[] MoveButtons = new string[] {
                MovePosX, MoveNegX, MoveNegY, MovePosY
            };

            public const string MovePosX = "MoveX+";
            public const string MoveNegX = "MoveX-";
            public const string MovePosY = "MoveY+";
            public const string MoveNegY = "MoveY-";
            public const string Map = "Map";
            public const string Cancel = "Cancel";
            public const string Inventory = "Inventory";
            public const string Character = "Character";
            public const string Menu = "Menu";
            public const string Use = "Use";
        }
    }
}