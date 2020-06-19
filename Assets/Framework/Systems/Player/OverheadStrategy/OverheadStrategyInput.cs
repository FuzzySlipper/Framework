﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 using UnityEngine.EventSystems;
 using UnityEngine.InputSystem;

 namespace PixelComrades {
    public class OverheadStrategyInput : UnityInputHandler {

        public OverheadStrategyInput(PlayerInput input) : base(input) { }
        
        private List<MenuAction> _menuActions = new List<MenuAction>();

        protected override void GameplayInput() {
            base.GameplayInput();
            if (!Game.InCombat) {
                return;
            }
            if (PlayerTurnBasedSystem.Current == null) {
                if (Physics.Raycast(GetLookTargetRay, out var hit1, 5000, LayerMasks.Actor)) {
                    var hitEntity = UnityToEntityBridge.GetEntity(hit1.collider);
                    if (hitEntity != null) {
                        var template = hitEntity.GetTemplate<TurnBasedCharacterTemplate>();
                        if (template != null) {
                            UICenterTarget.SetText(string.Format("{0} {1}/{2}", template.GetName(), template.TurnBased.ActionPoints, 3));
                        }
                    }
                }
                return;
            }
            if (PlayerTurnBasedSystem.Current.Tags.Contain(EntityTags.PerformingAction)) {
                return;
            }
            if (Mouse.current.rightButton.wasPressedThisFrame) {
                if (UISubMenu.Default.Active) {
                    UISubMenu.Default.Disable();
                }
                else if (!IsCursorOverUI) {
                    if (Physics.Raycast(GetLookTargetRay, out var hit1, 5000, LayerMasks.Environment)) {
                        World.Get<PlayerTurnBasedSystem>().OnMoveClick(hit1.point);
                    }    
                }
                
            }
        }

        protected override void ActionInput() {
            base.ActionInput();
            if (!Game.InCombat) {
                return;
            }
            if (PlayerTurnBasedSystem.Current == null || PlayerTurnBasedSystem.Current.Tags.Contain(EntityTags.PerformingAction) || IsCursorOverUI) {
                return;
            }
            if (Mouse.current.leftButton.wasPressedThisFrame) {
                if (Physics.Raycast(GetLookTargetRay, out var hit1, 5000, LayerMasks.Actor)) {
                    var clickEntity = UnityToEntityBridge.GetEntity(hit1.collider);
                    if (clickEntity != null) {
                        OnActorClick(clickEntity.GetTemplate<TurnBasedCharacterTemplate>());
                    }

                }
            }
        }

        private void OnActorClick(TurnBasedCharacterTemplate target) {
            if (target == null) {
                return;
            }
            if (UISubMenu.Default.Active) {
                UISubMenu.Default.Disable();
                return;
            }
            MenuAction.ClearActions(_menuActions);
            for (int i = 0; i < PlayerTurnBasedSystem.Current.ActionSlots.Count; i++) {
                var action = PlayerTurnBasedSystem.Current.ActionSlots.GetSlot(i).Action;
                if (action == null) {
                    continue;
                }
                if (!action.Config.CanTarget(action, PlayerTurnBasedSystem.Current, target)) {
                    continue;
                }
                _menuActions.Add(MenuAction.GetAction(action.Config.Source.Name, () => {
                    var cmd = CommandSystem.GetCommand<ActionCommand>(PlayerTurnBasedSystem.Current);
                    cmd.Action = action;
                    if (cmd.TryStart(target, true)) {
                        return true;
                    }
                    CommandSystem.Store(cmd);
                    return false;
                }));
            }
            _menuActions.Add(MenuAction.GetAction("Idle", () => {
                    CommandSystem.GetCommand<IdleCommand>(PlayerTurnBasedSystem.Current).TryStart(false);
                    return true;
                }
                ));
            UISubMenu.Default.EnableMenu( CameraSystem.Cam.WorldToScreenPoint(target.Tr.position), _menuActions);
        }

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