﻿using System;
 using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 using UnityEngine.EventSystems;
 using UnityEngine.InputSystem;

 namespace PixelComrades {
    public class OverheadStrategyInput : UnityInputHandler {

        public OverheadStrategyInput(PlayerInput input) : base(input) { }

        private LevelCell _currentMovePnt;
        private ActionTemplate _currentAction;
        private State _state = State.Waiting;
        private List<MenuAction> _currentMenu;
        private float _lastMoveRequest = -1;
        
        private TurnBasedCharacterTemplate Current { get { return PlayerTurnBasedSystem.Current; } }

        private enum State {
            Waiting,
            Movement,
            Action,
        }
        
        protected override void ActionInput() {
            base.ActionInput();
            if (!Game.InCombat) {
                return;
            }
            if (Current == null) {
                // if (Physics.Raycast(GetLookTargetRay, out var hit1, 5000, LayerMasks.Actor)) {
                //     var hitEntity = UnityToEntityBridge.GetEntity(hit1.collider);
                //     if (hitEntity != null) {
                //         var template = hitEntity.GetTemplate<TurnBasedCharacterTemplate>();
                //         if (template != null) {
                //             UICenterTarget.SetText(string.Format("{0} {1}/{2}", template.GetName(), template.TurnBased.ActionPoints, 3));
                //         }
                //     }
                // }
                return;
            }
            if (Current.Tags.Contain(EntityTags.PerformingAction)) {
                return;
            }
            if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current[Key.Escape].wasPressedThisFrame) {
                if (UISubMenu.Default.Active) {
                    UISubMenu.Default.Disable();
                }
                else if (_state != State.Waiting) {
                    CancelCurrent();
                }
            }
            if (_currentMenu != null) {
                for (int i = 0; i < _currentMenu.Count; i++) {
                    if (Keyboard.current[PlayerControls.NumericKeys[i]].wasPressedThisFrame) {
                        _currentMenu[i].TryUse();
                    }
                }
            }
            if (IsCursorOverUI) {
                return;
            }
            if (!Mouse.current.leftButton.wasPressedThisFrame) {
                return;
            }
            if (Physics.Raycast(GetLookTargetRay, out var hit1, 5000, LayerMasks.CombatInput)) {
                var clickEntity = UnityToEntityBridge.GetEntity(hit1.collider);
                switch (_state) {
                    case State.Movement:
                        if (clickEntity == null) {
                            OnMoveClick(hit1.point);
                        }
                        break;
                    case State.Action:
                        if (clickEntity != null) {
                            if (TryTargetCurrentAction(clickEntity.GetTemplate<TurnBasedCharacterTemplate>())) {
                                ClearAction();
                            }
                        }
                        else if (TryTargetCurrentAction(hit1.point)) {
                            ClearAction();
                        }
                        break;
                }
            }
        }

        public void TurnStarted() {
            _currentAction = null;
            _state = State.Waiting;
            Current.Selection.Renderer.color = LazyDb.Main.SelectionColor;
            SetupActorActions();
        }

        public void TurnContinue() {
            CancelCurrent();
        }

        public void TurnEnded() {
            CancelCurrent();
            if (Current != null) {
                Current.Selection.Renderer.color = LazyDb.Main.FriendlyColor;
            }
            _currentMovePnt = null;
            _currentAction = null;
            _currentMenu = null;
            _lastMoveRequest = -1;
            UIGenericHotbar.main.Disable();
        }

        private bool StartMovementMode() {
            if (_state == State.Movement) {
                ClearMove();
                return true;
            }
            var moveSpeed = Current.Pathfinder.MoveSpeed * Current.TurnBased.TotalMoveActions;
            if (Math.Abs(moveSpeed - _lastMoveRequest) > 0.01f) {
                _lastMoveRequest = moveSpeed;
                Current.Pathfinder.Value.FillReachable(Current.Location.Cell,_lastMoveRequest);
            }
            PathfindingDisplaySystem.Get.SetupPathfindingSprites(PlayerTurnBasedSystem.Current);
            _state = State.Movement;
            return true;
        }

        private void CancelCurrent() {
            switch (_state) {
                case State.Movement:
                    ClearMove();
                    break;
                case State.Action:
                    ClearAction();
                    break;
            }
        }

        private void ClearMove() {
            _state = State.Waiting;
            PathfindingDisplaySystem.Get.ClearDisplay();
        }

        private void ClearAction() {
            _state = State.Waiting;
            _currentAction = null;
            UICursor.main.SetDefault();
        }

        public void OnMoveClick(Vector3 hitPnt) {
            var hitCell = Game.CombatMap.Get(hitPnt);
            if (hitCell == _currentMovePnt) {
                if (PlayerTurnBasedSystem.Get.TryMoveToCurrent()) {
                    ClearMove();
                }
                return;
            }
            _currentMovePnt = hitCell;
            int moveAp = Current.TurnBased.MoveActions + Current.TurnBased.StandardActions;
            Current.Pathfinder.Value.SetCurrentPath(Current.Location, _currentMovePnt, moveAp, Current.Pathfinder.MoveSpeed);
            PathfindingDisplaySystem.Get.SetCurrentPath(Current);
        }

        private void SetupActorActions() {
            _currentMenu = MenuAction.GetList();
            _currentMenu.Add(GenericMenuAction.GetAction("Move", SpriteDatabase.Move, StartMovementMode));
            for (int i = 0; i < Current.ActionSlots.Count; i++) {
                var action = Current.ActionSlots.GetSlot(i).Action;
                if (action == null) {
                    continue;
                }
                _currentMenu.Add(ActionTemplateMenuAction.Get(action, Current));
            }
            _currentMenu.Add(GenericMenuAction.GetAction("Idle", SpriteDatabase.Idle, EndTurn));
            UIGenericHotbar.main.EnableMenu(_currentMenu);
        }

        private bool EndTurn() {
            CommandSystem.GetCommand<IdleCommand>(Current).TryStart(false);
            return true;
        }

        public void UseAction(ActionTemplate actionTemplate) {
            if (actionTemplate.Config.Source.Targeting == TargetType.Self) {
                TryTargetCurrentAction(Current);
                return;
            }
            if (_state == State.Movement) {
                ClearMove();
            }
            _currentAction = actionTemplate;
            UICursor.main.SetTarget();
            _state = State.Action;
        }

        private bool TryTargetCurrentAction(TurnBasedCharacterTemplate target) {
            if (target == null) {
                return false;
            }
            var cmd = CommandSystem.GetCommand<ActionCommand>(Current);
            cmd.SetAction(_currentAction);
            if (cmd.TryStart(target, true)) {
                return true;
            }
            CommandSystem.Store(cmd);
            return false;
        }

        private bool TryTargetCurrentAction(Vector3 target) {
            var cmd = CommandSystem.GetCommand<ActionCommand>(Current);
            cmd.SetAction(_currentAction);
            if (cmd.TryStart(target)) {
                return true;
            }
            CommandSystem.Store(cmd);
            return false;
        }

        // private void OnActorClick(TurnBasedCharacterTemplate target) {
        //     if (target == null) {
        //         return;
        //     }
        //     if (UISubMenu.Default.Active) {
        //         UISubMenu.Default.Disable();
        //         return;
        //     }
        //     var menuActions = MenuAction.GetList();
        //     for (int i = 0; i < PlayerTurnBasedSystem.Current.ActionSlots.Count; i++) {
        //         var action = PlayerTurnBasedSystem.Current.ActionSlots.GetSlot(i).Action;
        //         if (action == null) {
        //             continue;
        //         }
        //         if (!action.Config.CanTarget(action, PlayerTurnBasedSystem.Current, target)) {
        //             continue;
        //         }
        //         menuActions.Add(
        //             MenuAction.GetAction(
        //                 action.Config.Source.Name,
        //                 () => {
        //                     var cmd = CommandSystem.GetCommand<ActionCommand>(PlayerTurnBasedSystem.Current);
        //                     cmd.Action = action;
        //                     if (cmd.TryStart(target, true)) {
        //                         return true;
        //                     }
        //                     CommandSystem.Store(cmd);
        //                     return false;
        //                 }));
        //     }
        //     menuActions.Add(
        //         MenuAction.GetAction(
        //             "Idle",
        //             () => {
        //                 CommandSystem.GetCommand<IdleCommand>(PlayerTurnBasedSystem.Current).TryStart(false);
        //                 return true;
        //             }
        //         ));
        //     UISubMenu.Default.EnableMenu(CameraSystem.Cam.WorldToScreenPoint(target.Tr.position), menuActions);
        // }

        private bool CheckRightClickIso() {
            if (IsCursorOverUI || !Input.GetMouseButtonDown(1)) {
                return false;
            }
            if (!IsCursorOverUI && UISubMenu.Default.Active) {
                UISubMenu.Default.Disable();
            }
            if (UIRadialMenu.Active) {
                return false;
            }
            if (UIDragDropHandler.TryRightClick()) {
                return false;
            }
            return true;
        }

        private bool CheckLeftClickIso() {
            if (Input.GetMouseButton(0) && UIDragDropHandler.Active && !UIDragDropHandler.IsUiDragging && !UIDragDropHandler.IsManualDragging) {
                if (UIDragDropHandler.TimeActive > UIDragDropHandler.DragTime) {
                    UIDragDropHandler.IsManualDragging = true;
                }
            }
            if (Input.GetMouseButtonUp(0) && UIDragDropHandler.IsManualDragging) {
                if (EventSystem.current.currentSelectedGameObject != null) {
                    var drop = EventSystem.current.currentSelectedGameObject.GetComponent<IEndDragHandler>();
                    if (drop != null) {
                        drop.OnEndDrag(null);
                    }
                }
            }
            if (IsCursorOverUI || !Input.GetMouseButtonDown(0)) {
                return false;
            }
            if (UISubMenu.Default.Active) {
                UISubMenu.Default.Disable();
            }
            if (UIDragDropHandler.CurrentData != null && UIDropWorldPanel.Active) {
                UIDropWorldPanel.main.TryThrow(UIDragDropHandler.CurrentData);
                return false;
            }
            if (WorldControlMonitor.Use()) {
                return false;
            }
            return true;
        }
    }
}