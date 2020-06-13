using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class PlayerControllerSystem : SystemWithSingleton<PlayerControllerSystem, PlayerController>, IMainSystemUpdate {

        private GameOptions.CachedInt _inventorySize = new GameOptions.CachedInt("InventorySize"); //55
        private Dictionary<System.Type, PlayerController> _controllerDict = new Dictionary<Type, PlayerController>();
        
        public static IFirstPersonController FirstPersonController { get; private set; }

        protected override void SetCurrent(PlayerController current) {
            if (Current != null) {
                Current.Disable();
            }
            base.SetCurrent(current);
            if (!_controllerDict.ContainsKey(current.GetType())) {
                _controllerDict.Add(current.GetType(), current);
            }
            current.Enable();
            Player.Tr = current.Tr;
            FirstPersonController = current as IFirstPersonController;
            Player.MainEntity = current.MainEntity;
            if (!current.MainEntity.HasComponent<ItemInventory>()) {
                Player.MainInventory = Player.MainEntity.Add(new ItemInventory(_inventorySize));
            }
        }

        public void AddToDict(PlayerController current) {
            if (!_controllerDict.ContainsKey(current.GetType())) {
                _controllerDict.Add(current.GetType(), current);
            }
        }

        public T GetController<T>() where T : PlayerController {
            return _controllerDict.TryGetValue(typeof(T), out var current) ? current as T: null;
        }

        public void SetController<T>() where T : PlayerController {
            if (_controllerDict.TryGetValue(typeof(T), out var current)) {
                Set(current);
            }
        }

        public void NewGame() {
            if (Player.MainInventory != null) {
                Player.MainInventory.Clear();
                UIPlayerComponents.InventoryUI.SetInventory(Player.MainInventory, "Party Items");
            }
            foreach (var controller in _controllerDict) {
                controller.Value.NewGame();
            }
            // for (int i = 0; i < SingletonList.Count; i++) {
            //     SingletonList[i].NewGame();
            // }
        }

        public void SetActive(bool status) {
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
