//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using PixelComrades;
//using UnityEngine.EventSystems;

//public class PlayerInputAction : PlayerInput {

//    protected override void GameplayInput() {
//        base.GameplayInput();
//        if (Cursor.lockState == CursorLockMode.None && Game.Paused) {
//            Player.Controller.MoveInput = Vector2.zero;
//            FirstPersonCamera.LookInput = Vector2.zero;
//        }
//        else {
//            GetLookInput();
//            GetMoveInput();
//            ActionInput();
//        }
//        if (GameOptions.TurnBased) {
//            CheckPausing();
//        }
//    }

//    private void CheckPausing() {
//        if (Player.Controller.Velocity.sqrMagnitude > 0.05f || 
//            Player.Controller.MoveInput.sqrMagnitude > 0.05f) {
//                Game.Paused = false;
//        }
//        else if (!Player.Actor.CanMove) {
//            Game.Paused = false;
//        }
//        else {
//            Game.Paused = !InputSystem.GetButton(ActionControls.PassTime);
//        }
//    }

//    private void GetLookInput() {
//        if (Cursor.lockState != CursorLockMode.Locked || UIRadialMenu.Active) {
//            FirstPersonCamera.LookInput = Vector2.zero;
//            return;
//        }
//        FirstPersonCamera.LookInput = LookInput;
//    }

//    private void GetMoveInput() {
//        Player.Controller.MoveInput = MoveInput;
//        if (InputSystem.GetButtonDown(ActionControls.Jump)) {
//            Player.Controller.TryJump();
//        }
//        else if (InputSystem.GetButton(ActionControls.Jump)) {
//            Player.Controller.TryClimb();
//        }
//        if (InputSystem.GetButtonDown(ActionControls.Dodge)) {
//            Player.Controller.Dodge();
//        }
//        if (InputSystem.GetButtonDown(ActionControls.ZTarget)) {
//            UICenterTarget.ToggleActorLock();
//        }
//        Player.Controller.InputRun = InputSystem.GetButton(ActionControls.FastMove);
//    }

//    protected override void ActionInput() {
//        base.ActionInput();
//        if (Cursor.lockState != CursorLockMode.Locked) {
//            return;
//        }
//        if (InputSystem.GetButtonDown(ActionControls.Action)) {
//            Player.PlayerActionPlayer.PrimaryStart();    
//        }
//        else if (InputSystem.GetButtonUp(ActionControls.Action)) {
//            Player.PlayerActionPlayer.PrimaryEnd();
//        }
//        if (InputSystem.GetButtonDown(ActionControls.Offhand)) {
//            Player.PlayerActionPlayer.SecondaryStart();    
//        }
//        else if (InputSystem.GetButtonUp(ActionControls.Offhand)) {
//            Player.PlayerActionPlayer.SecondaryEnd();
//        }
//        if (InputSystem.GetButtonDown(ActionControls.FreezeTime)) {
//            GameOptions.TurnBased = !GameOptions.TurnBased;
//            if (!GameOptions.TurnBased) {
//                Game.Paused = false;
//            }
//        }
//    }

//    protected override void ReceivedFocus(bool focusStatus) {
//        if (focusStatus) {
//            Cursor.visible = false;
//            CheckForOpenMenus();
//        }
//        else {
//            Cursor.lockState = CursorLockMode.None;
//            Cursor.visible = true;
//        }
//    }

//    protected override void CheckForOpenMenus() {
//        if (Game.OpenMenu != null) {
//            if (PauseInMenus) {
//                Game.Paused = true;
//            }
//            Cursor.lockState = CursorLockMode.None;
//        }
//        else {
//            if (PauseInMenus) {
//                Game.Paused = false;
//            }
//            Cursor.lockState = CursorLockMode.Locked;
//        }
//    }

//    protected override void MenuInput() {
//        base.MenuInput();
//        if (InputSystem.GetButtonDown(ActionControls.Radial) && !EventSystem.current.IsPointerOverGameObject()) {
//            if (UIRadialMenu.Active) {
//                UIRadialMenu.Cancel();
//            }
//            else {
//                OpenRadial();
//            }
//        }
//        if (InputSystem.GetButtonDown(ActionControls.Reload) && !EventSystem.current.IsPointerOverGameObject()) {
//            if (UIRadialMenu.Active) {
//                UIRadialMenu.Cancel();
//            }
//            else {
//                OpenAmmoRadial();
//            }
//        }
//    }
    
//    private void OpenAmmoRadial() {
//        //var menuActions = new List<MenuAction>(Player.Actor.HotbarInventory.ItemCount);
//        //if (Player.Animator.ActiveSlot.EquippedItem == null) {
//        //    return;
//        //}
//        //var weapon = Player.Animator.ActiveSlot.EquippedItem.Template as RangedWeaponItemTemplate;
//        //if (weapon == null || weapon.Ammo == AmmoTypes.None) {
//        //    return;
//        //}
//        //for (int i = 0; i < Player.Actor.Inventory.ItemCount; i++) {
//        //    var item = Player.Actor.Inventory[i];
//        //    if (item== null) {
//        //        continue;
//        //    }
//        //    var ammo = item.Template as AmmoItemTemplate;
//        //    if (ammo == null || ammo.AmmoType != weapon.Ammo) {
//        //        continue;
//        //    }
//        //    menuActions.Add(MenuAction.GetAction(item, () => {
//        //        Player.Animator.ActiveSlot.EquippedItem.TryCombineItem(item);
//        //    }));
//        //}
//        //if (menuActions.Count > 0) {
//        //    UIRadialMenu.Open("Switch", menuActions, Input.mousePosition);
//        //}
//    }

//    private void OpenRadial() {
//        //var menuActions = new List<MenuAction>(Player.Actor.HotbarInventory.ItemCount);
//        //for (int i = 0; i < Player.Actor.HotbarInventory.ItemCount; i++) {
//        //    var item = Player.Actor.HotbarInventory[i];
//        //    if (item== null) {
//        //        continue;
//        //    }
//        //    menuActions.Add(MenuAction.GetAction(item, () => {
//        //        Player.Animator.ActiveSlot.Equip(item);
//        //    }));
//        //}
//        //if (menuActions.Count > 0) {
//        //    UIRadialMenu.Open("Switch", menuActions, Input.mousePosition);
//        //}
//    }
    
//    public static class ActionControls {
//        public const string FastMove = "FastMove";
//        public const string Jump = "Jump";
//        public const string FreezeTime = "FreezeTime";
//        public const string PassTime = "PassTurn";
//        public const string Action = "Action";
//        public const string Radial = "Radial";
//        public const string Offhand = "Offhand";
//        public const string ZTarget = "ZTarget";
//        public const string Dodge = "Dodge";
//        public const string Reload = "Reload";
//    }

//}