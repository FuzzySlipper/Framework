using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace PixelComrades {
    public sealed class TravelMapInput : UnityInputHandler {

        private TravelMapController _controller;
        
        public TravelMapInput(PlayerInput input, TravelMapController controller) : base(input) {
            _controller = controller;
        }

        protected override void ActionInput() {
            base.ActionInput();
            if (PlayerInputSystem.AreMenusOpen()) {
                return;
            }
            var cell = GetCellUnderCursor();
            if (cell == null) {
                if (_controller.DisplayingText) {
                    UICenterTarget.Clear();
                    _controller.DisplayingText = false;
                }
                return;
            }
            var loc = RiftMap.GetLocation(cell);
            if (loc != null) {
                _controller.DisplayingText = true;
                UICenterTarget.SetText(loc.DisplayName);
            }
            else if (_controller.DisplayingText) {
                UICenterTarget.Clear();
                _controller.DisplayingText = false;
            }
            if (!Mouse.current.leftButton.isPressed) {
                return;
            }
            if (_controller.PlayerUnit.Location == cell && loc != null) {
                loc.Clicked();
            }
            else if (_controller.PlayerUnit.IsValidDestination(cell)) {
                var path = HexGridPathfinding.Current.GetPath(HexGrid.main.GetCell(_controller.PlayerUnit.transform.position), cell, _controller.PlayerUnit);
                if (path != null) {
                    _controller.Travel(path);
                }
            }
        }

        private HexCell GetCellUnderCursor() {
            return HexGrid.main.GetCell(_controller.Cam.ScreenPointToRay(PlayerInputSystem.CursorPosition));
        }
    }
}
