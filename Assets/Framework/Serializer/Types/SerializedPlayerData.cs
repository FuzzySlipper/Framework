using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace PixelComrades {
    [System.Serializable]
    public class SerializedPlayerData : ISerializable {

        //[SerializeField] private PlayerSaveData _playerData;
        [SerializeField] private string[] _currentActors = new string[4];

        public void Load() {
            //Player.Data = _playerData;
            //for (int i = 0; i < _currentActors.Length; i++) {
            //    var actor = _playerData.GetActor(_currentActors[i]);
            //    if (actor == null) {
            //        PlayerControllerSystem.Current.Party[i].ChangeCharacter(SavedActorData.Random());
            //        continue;
            //    }
            //    PlayerControllerSystem.Current.Party[i].ChangeCharacter(actor);
            //}
            //Player.MainInventory.DespawnContents();
            //for (int i = 0; i < _inventoryItems.Count; i++) {
            //    Player.MainInventory.AddItem(_inventoryItems[i].GetItem());
            //}
        }

        public SerializedPlayerData(SerializationInfo info, StreamingContext context) {
            //_playerData = (PlayerSaveData)info.GetValue("PlayerData", typeof(PlayerSaveData));
            _currentActors = (string[]) info.GetValue("CurrentActors", typeof(string[]));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            UpdateCurrentData();
            //info.AddValue("PlayerData", _playerData, typeof(PlayerSaveData));
            info.AddValue("CurrentActors", _currentActors, typeof(string[]));
        }

        private void UpdateCurrentData() {
            //_playerData = Player.Data;
            //_currentActors = new string[PlayerControllerSystem.Current.Party.Length];
            //for (int i = 0; i < PlayerControllerSystem.Current.Party.Length; i++) {
            //    _currentActors[i] = PlayerControllerSystem.Current.Party[i].Id;
            //}
            //_inventoryItems.Clear();
            //for (int i = 0; i < Player.MainInventory.ActiveItems.Count; i++) {
            //    _inventoryItems.Add(new SerializedItem(Player.MainInventory.ActiveItems[i]));
            //}
        }

        public SerializedPlayerData() {
            UpdateCurrentData();
        }
    }
}