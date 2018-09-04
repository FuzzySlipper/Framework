using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace PixelComrades {
    [System.Serializable]
    public class SerializedSaveGame : ISerializable {
        
        [SerializeField] private string _name;
        [SerializeField] private string _riftLocation;
        [SerializeField] private int _riftLevel;
        [SerializeField] private float _encounterRate;
        [SerializeField] private int _encounterSteps;
        [SerializeField] private SerializedPlayerData _playerData;
        [SerializeField] private List<SerializedGenericData> _mapData = new List<SerializedGenericData>();
        [SerializeField] private SerializedTransform _playerPosition;
        //[SerializeField] private List<SerializedGameObject> _activePooled;
        //[SerializeField] private List<SerializedGameObject> _level;
        public List<SerializedGenericData> MapData { get { return _mapData; } }
        public SerializedTransform PlayerPosition { get { return _playerPosition; } }
        public float EncounterRate { get { return _encounterRate; } }
        public int EncounterSteps { get { return _encounterSteps; } }

        public SerializedSaveGame() {}

        public SerializedSaveGame(SerializationInfo info, StreamingContext context) {
            _name = (string)info.GetValue("Name", typeof(string));
            _playerData = (SerializedPlayerData) info.GetValue("PlayerData", typeof(SerializedPlayerData));
            _riftLocation = (string) info.GetValue("RiftLocation", typeof(string));
            _riftLevel = (int) info.GetValue("RiftLevel", typeof(int));
            _encounterSteps = (int) info.GetValue("EncounterSteps", typeof(int));
            _encounterRate = (float) info.GetValue("EncounterRate", typeof(float));
            _playerPosition = (SerializedTransform) info.GetValue("PlayerPosition", typeof(SerializedTransform));
            _mapData = (List<SerializedGenericData>) info.GetValue("MapData", typeof(List<SerializedGenericData>));
            //_activePooled = (List<SerializedGameObject>)info.GetValue("ActivePooled", typeof(List<SerializedGameObject>));
            //_level = (List<SerializedGameObject>) info.GetValue("Level", typeof(List<SerializedGameObject>));
        }


        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("Name", _name, typeof(string));
            info.AddValue("PlayerData", _playerData, typeof(SerializedPlayerData));
            info.AddValue("RiftLocation", _riftLocation, typeof(string));
            info.AddValue("RiftLevel", _riftLevel, typeof(int));
            info.AddValue("EncounterSteps", _encounterSteps, typeof(int));
            info.AddValue("EncounterRate", _encounterRate, typeof(float));
            info.AddValue("PlayerPosition", _playerPosition, typeof(SerializedTransform));
            info.AddValue("MapData", _mapData, typeof(List<SerializedGenericData>));
            //info.AddValue("ActivePooled", _activePooled, typeof(List<SerializedGameObject>));
            //info.AddValue("Level", _level, typeof(List<SerializedGameObject>));
        }

        public SerializedSaveGame(string name) {
            Construct(name);
        }

        private void Construct(string name) {
            _name = name;
            //_level = new List<SerializedGameObject>();
            //_activePooled = new List<SerializedGameObject>();
            _playerData = new SerializedPlayerData();
            //_riftLocation = RiftTown.Current != null ? RiftTown.Current.DisplayName : GlobalLevelController.CurrentConfig.Name;
            //_riftLevel = RiftTown.Current != null ? 0 : GlobalLevelController.CurrentIndex;
            //_playerPosition = new SerializedTransform(Player.Controller.Tr);
            //_encounterRate = Player.Controller.CurrentEncounterRate;
            //_encounterSteps = Player.Controller.CurrentSteps;
            //if (GlobalLevelController.Root != null) {
            //    var receivers = GlobalLevelController.Root.gameObject.GetComponentsInChildren<ILevelLoadEvents>();
            //    if (receivers != null) {
            //        for (int i = 0; i < receivers.Length; i++) {
            //            receivers[i].LevelSaved(ref _mapData);
            //        }
            //    }
            //}
            //if (GlobalLevelController.Loaded != null) {
            //    GlobalLevelController.Loaded.SaveData(ref _mapData);
            //}
            //_riftLocation =  RiftTown.Current != null ? RiftTown.Current.DisplayName : RiftTown.Last.DisplayName;
            //Serializer.ResetGlobalSerializationIndex();
            //ResetSerializedIds(LevelController.Root);
            //ResetSerializedIds(ItemPool.ActiveSceneTr);
            //ScanChildren(LevelController.Root.gameObject, _level);
            //ScanChildren(ItemPool.ActiveSceneTr.gameObject, _activePooled);
        }

        private void ResetSerializedIds(Transform root) {
            var we = root.GetComponentsInChildren<PrefabEntity>();
            for (int i = 0; i < we.Length; i++) {
                we[i].Metadata.UpdateSerializedId(true);
            }
        }

        private void ScanChildren(GameObject root, List<SerializedGameObject> list) {
            foreach (Transform child in root.transform) {
                if (child.gameObject.hideFlags == HideFlags.DontSave || child.gameObject.hideFlags == HideFlags.HideAndDontSave) {
                    continue;
                }
                var we = child.GetComponent<PrefabEntity>();
                if (we == null) {
                    continue;
                }
                list.Add(new SerializedGameObject(child.gameObject, we));
            }
        }

        public void Load() {
            if (string.IsNullOrEmpty(_riftLocation)) {
                Debug.LogError("Error Loading Scene: Empty Location");
                return;
            }
            //var config = RiftDatabase.GetRift(_riftLocation);
            //if (config == null) {
            //    Debug.LogErrorFormat("Error Loading Scene: {0}", _riftLocation);
            //    return;
            //}
            //var location = RiftsMap.GetLocationFromConfig(config);
            //if (location != null) {
            //    if (location is RiftTown) {
            //        _playerData.Load();
            //        location.Entered();
            //        return;
            //    }
            //}
            //_playerData.Load();
            //GlobalLevelController.LoadLevelFromSaveGame(config, _riftLevel, this);
            //LevelController.Clear();
            //LoadList(LevelController.Root.gameObject, _level);
            //LoadList(ItemPool.ActiveSceneTr.gameObject, _activePooled);
        }

        private void LoadList(GameObject root, List<SerializedGameObject> list) {
            if (root == null) {
                root = new GameObject();
            }
            root.name = _name;
            for (var i = 0; i < list.Count; i++) {
                var child = list[i].ToGameObject(root.transform);
                child.transform.SetParent(root.transform);
            }
            Serializer.ClearList();
            var worldEntities = root.GetComponentsInChildren<PrefabEntity>();
            RegisterSerializedIds(worldEntities, true);
            RegisterSerializedIds(worldEntities, false);
            for (int i = 0; i < worldEntities.Length; i++) {
                worldEntities[i].Metadata.ComponentDiff.ApplyDifferences(worldEntities[i]);
            }
        }

        private void RegisterSerializedIds(PrefabEntity[] prefabEntities, bool onlyValid) {
            for (int i = 0; i < prefabEntities.Length; i++) {
                var we = prefabEntities[i];
                if (onlyValid && !we.Metadata.HasValidSerializedId) {
                    continue;
                }
                if (!onlyValid && we.Metadata.HasValidSerializedId) {
                    continue;
                }
                Serializer.AddEntity(we);
            }
        }
    }
}