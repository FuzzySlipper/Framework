using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace PixelComrades {
    public class PlayerGridInput : UnityInputHandler {

        private PlayerGridController _controller;
        private static float _moveCheck = 0.1f;

        public PlayerGridInput(PlayerInput input, PlayerGridController controller) : base(input) {
            _controller = controller;
        }

        public void DebugPlayerMove() {
            Debug.LogFormat("PlayerMoveEnabled {0}", PlayerControllerSystem.Current.MoveEnabledHolder.Debug());
        }

        public void HealAll() {
            for (int i = 0; i < PlayerPartySystem.Party.Length; i++) {
                PlayerPartySystem.Party[i].Stats.SetMax();
            }
        }

        protected override void MenuInput() {
            base.MenuInput();
            if (GetButtonDown(PlayerControls.Character) || GetButtonDown(PlayerControls.Inventory)) {
                UIGameplayPanel.main.ToggleActive();
            }
        }

        protected override void GameplayInput() {
            if (!Game.GameActive || _controller == null || !PlayerControllerSystem.Current.CanMove) {
                return;
            }
            SetMoveLook();
            // if (GetButtonDown(PlayerControls.RotateLeft)) {
            //       gridMover.RotateActorTo(Directions.Left);
            // }
            // if (GetButtonDown(PlayerControls.RotateRight)) {
            //      gridMover.RotateActorTo(Directions.Right);
            // }
            LevelCell targetCell = null;
            if (MoveInput.x > _moveCheck) {
                targetCell = GetLevelCell(_controller.GridPosition + _controller.Tr.right.toPoint3());
            }
            else if (MoveInput.x < -_moveCheck) {
                targetCell = GetLevelCell(_controller.GridPosition - _controller.Tr.right.toPoint3());
            }
            else if (MoveInput.y > _moveCheck) {
                targetCell = GetLevelCell(_controller.GridPosition + _controller.Tr.forward.toPoint3());
            }
            else if (MoveInput.y < -_moveCheck) {
                targetCell = GetLevelCell(_controller.GridPosition - _controller.Tr.forward.toPoint3());
            }
            else if (!IsCursorOverUI && Mouse.current.rightButton.isPressed) {
                targetCell = GetLevelCell(_controller.GridPosition + _controller.Tr.forward.toPoint3());
            }
            // if (GetKeyDown(GridControls.LookDown)) {
            //     GridCamera.main.LookDown();
            // }
            if (targetCell != null) {
                _controller.TryMove(targetCell);
            }
        }

        private LevelCell GetLevelCell(Point3 position) {
            return World.Get<MapSystem>().GetCell(position) as LevelCell;
        }

        protected override void ActionInput() {
            CheckLeftClick();
            CheckRightClick();
            base.ActionInput();
            for (int i = 0; i < PlayerPartySystem.Party.Length; i++) {
                if (GetKeyDown(PlayerControls.NumericKeys[i])) {
                    Player.SelectedActor = PlayerPartySystem.Party[i];
                }
            }
            if (GetButtonDown(PlayerControls.Pause)) {
                if (Game.Paused) {
                    Game.RemovePause("TogglePause");
                    Game.RemoveCursorUnlock("TogglePause");
                }
                else {
                    Game.CursorUnlock("TogglePause");
                    Game.Pause("TogglePause");
                }
            }
            
            if (GetButtonDown(PlayerControls.TurnBased)) {
                GameOptions.TurnBased = !GameOptions.TurnBased;
            }
            //if (GetKeyDown(GridControls.SelfCast)) {
            //    UIActionController.main.OpenRadial(Player.SelectedActor);
            //}
        }

        private void CheckLeftClick() {
            var button = Mouse.current.leftButton;
            if (button.isPressed && UIDragDropHandler.Active && !UIDragDropHandler.IsUiDragging && !UIDragDropHandler.IsManualDragging) {
                if (UIDragDropHandler.TimeActive > UIDragDropHandler.DragTime) {
                    UIDragDropHandler.IsManualDragging = true;
                }
            }
            if (button.wasReleasedThisFrame && UIDragDropHandler.IsManualDragging) {
                if (EventSystem.current.currentSelectedGameObject != null) {
                    var drop = EventSystem.current.currentSelectedGameObject.GetComponent<IEndDragHandler>();
                    if (drop != null) {
                        drop.OnEndDrag(null);
                    }
                }
            }
            if (IsCursorOverUI || !button.wasPressedThisFrame) {
                return;
            }
            if (UISubMenu.Default.Active) {
                UISubMenu.Default.Disable();
            }
            if (UIDragDropHandler.CurrentData != null && UIDropWorldPanel.Active) {
                UIDropWorldPanel.main.TryThrow(UIDragDropHandler.CurrentData);
                return;
            }
            if (UICenterTarget.CurrentCharacter != null) {
                var actor = Player.SelectedActor;
                if (!actor.Stats.GetVital("Vitals.Recovery").IsMax) {
                    for (int i = 0; i < PlayerPartySystem.Party.Length; i++) {
                        if (PlayerPartySystem.Party[i].Entity != null && PlayerPartySystem.Party[i].Stats.GetVital("Vitals.Recovery").IsMax) {
                            actor = PlayerPartySystem.Party[i];
                            break;
                        }
                    }
                }
                // var attack = actor.Entity.Get<DefaultCommand>();
                //actor.Entity.GetOrAdd<CommandTarget>().Target = UICenterTarget.CurrentVisible.Entity;
                // if (attack == null) {
                //     actor.Entity.Post(new StatusUpdate(actor.Entity,"No Default Attack", Color.yellow));
                // }
                // else if (!attack.Get.TryStart(UICenterTarget.CurrentCharacter)) {
                //     actor.Entity.Post(new StatusUpdate(actor.Entity, actor.Entity.Find<StatusUpdateComponent>().Status, Color.yellow));
                // }
            }
            if (WorldControlMonitor.Use()) {
                return;
            }
        }

        private void CheckRightClick() {
            var button = Mouse.current.rightButton;
            if (!button.wasPressedThisFrame) {
                return;
            }
            if (!IsCursorOverUI && UISubMenu.Default.Active) {
                UISubMenu.Default.Disable();
            }
            else if (UICenterTarget.CurrentCharacter != null && !UIRadialMenu.Active) {
                UIActionController.OpenRadial(UICenterTarget.CurrentCharacter);
            }
            else {
                UIDragDropHandler.TryRightClick();
            }
        }

        //private void CheckIsoClick(bool leftClick, bool rightClick) {
        //    if (!leftClick && !rightClick) {
        //        return;
        //    }
        //    _mouseRay = WorldControlMonitor.Cam.ScreenPointToRay(PlayerInputSystem.CursorPosition);
        //    var cnt = Physics.RaycastNonAlloc(_mouseRay, _hits, 500, LayerMasks.DefaultCollision);
        //    _hits.SortByDistanceAsc(cnt);
        //    for (int i = 0; i < cnt; i++) {
        //        var hit = _hits[i];
        //        Actor actor = Actor.Get(hit.collider);
        //        if (actor != null) {
        //            if (leftClick) {
        //                if (actor.Faction == Factions.Enemy && Player.SelectedActor != null) {
        //                    var attack = Player.SelectedActor.GetAttack(Player.SelectedActor.ActionDistance(UICenterTarget.CurrentVisible));
        //                    if (!attack.TryStart(UICenterTarget.CurrentVisible)) {
        //                        UIFloatingText.Spawn(attack.LastStatusUpdate, 2f, Player.SelectedActor.PartySlot.RectTr, Color.yellow);
        //                    }
        //                }
        //                //else {
        //                //    var playActor = actor as PlayerActor;
        //                //    if (playActor != null) {
        //                //        Player.SelectedActor = playActor;
        //                //    }
        //                //}
        //            }
        //            if (rightClick) {
        //                UIActionController.main.OpenRadial(actor);
        //            }
        //            continue;
        //        }
        //        #if AStarPathfinding
        //        if (LayerMasks.Environment.ContainsLayer(hit.transform.gameObject.layer)) {
        //            if (Player.SelectedActor != null && leftClick) {
        //                var info = CellGridGraph.Current.GetNearest(hit.point, _constraint);
        //                if (info.node != null) {
        //                    Player.SelectedActor.StateController.MoveTo((Vector3) info.node.position);
        //                }
        //            }
        //        }
        //        #endif
        //    }
        //}
    }
}