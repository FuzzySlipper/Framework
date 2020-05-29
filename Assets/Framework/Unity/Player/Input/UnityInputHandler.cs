using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace PixelComrades {
    public class UnityInputHandler : IPlayerInputHandler {

        private RiftRidersControls _controls;
        private PlayerInput _input;
        
        public UnityInputHandler(PlayerInput input) {
            _controls = new RiftRidersControls();
            _input = input;
            _controls.Enable();
        }

        public Ray GetLookTargetRay { get { return Player.Cam.ScreenPointToRay(Mouse.current.position.ReadValue()); } }
        public Vector2 LookInput { get; set; }
        public Vector2 MoveInput { get; set; }

        public bool IsCursorOverUI {
            get {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) {
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

        public Vector3 GetMousePosition(float range = 500) {
            var ray = GetLookTargetRay;
            return ray.origin + (ray.direction * range);
        }

        public bool GetKeyDown(Key key) {
            return Keyboard.current[key].wasPressedThisFrame;
        }

        public void RunUpdate() {
            MenuInput();
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
            SetMoveLook();
        }

        protected void SetMoveLook() {
            MoveInput = _controls.Player.Move.ReadValue<Vector2>();
            LookInput = _controls.Player.Look.ReadValue<Vector2>();
        }


        protected virtual void ActionInput() {
            if (!Game.GameActive) {
                return;
            }
            if (UIRadialMenu.Active) {
                for (int i = 0; i < PlayerControls.NumericKeys.Length; i++) {
                    if (GetKeyDown(PlayerControls.NumericKeys[i])) {
                        UIRadialMenu.Confirm(i);
                    }
                }
                return;
            }
            if (GetButtonDown(PlayerControls.Use)) {
                if (!WorldControlMonitor.Use()) {
                    UICenterButton.TryClickEvent();
                }
            }

        }

        protected virtual void MenuInput() {
            if (GetButtonDown(PlayerControls.Map)) {
                UIMap.main.ToggleActive();
            }
        }
        public bool GetButtonDown(string action) {
            return _input.actions[action].triggered;
        }

        public bool GetButton(string action) {
            return _input.actions[action].ReadValue<float>() > InputSystem.settings.defaultButtonPressPoint;
        }

        public bool GetButtonUp(string action) {
            return !_input.actions[action].triggered;
        }

        public float GetAxis(string axis) {
            return _input.actions[axis].ReadValue<float>();
        }

        public float GetAxisRaw(string axis) {
            return _input.actions[axis].ReadValue<float>();
        }
    }
}
