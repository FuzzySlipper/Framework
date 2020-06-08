using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class PlayerControllerSystem : SystemBase<PlayerControllerSystem>, IMainSystemUpdate {

        private GameOptions.CachedInt _inventorySize = new GameOptions.CachedInt("InventorySize"); //55
        
        public static PlayerController Current { get; private set; }
        public static IFirstPersonController FirstPersonController { get; private set; }

        public void SetController(PlayerController controller) {
            if (Current != null) {
                Current.Disable();
            }
            Current = controller;
            Current.Enable();
            Player.Tr = controller.Tr;
            FirstPersonController = controller as IFirstPersonController;
            Player.MainEntity = controller.MainEntity;
            if (!controller.MainEntity.HasComponent<ItemInventory>()) {
                Player.MainInventory = Player.MainEntity.Add(new ItemInventory(_inventorySize));
            }
        }

        public void NewGame() {
            if (Player.MainInventory != null) {
                Player.MainInventory.Clear();
                UIPlayerComponents.InventoryUI.SetInventory(Player.MainInventory, "Party Items");
            }
            if (Current != null) {
                Current.NewGame();
            }
        }

        public void SetActive(bool status) {
            PlayerCamera.Cam.gameObject.SetActive(status);
            Current.SetActive(status);
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            if (Current != null) {
                Current.SystemUpdate(dt);
            }
        }

        public void OnDeath() {
            MessageKit.post(Messages.PlayerDead);
            Game.SetGameActive(false);
        }
    }
}
