using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public enum AssetType {
        Prefab = 0,
        Scene = 1,
    }
    
    //extra information on stored asset
    [System.Serializable]
    public class SerializedMetaData : ISerializable {

        [SerializeField] private AssetType _assetType;
        [SerializeField] private string _prefabPath = "";
        [SerializeField] private int _serializationId = -1;
        [SerializeField] private SerializedComponentDifferences _componentDiff;

        public string PrefabPath { get { return _prefabPath; } }
        public SerializedComponentDifferences ComponentDiff { get { return _componentDiff; } }
        public AssetType AssetType { get { return _assetType; } set { _assetType = value; } }
        public int SerializationId {
            get {
                if (!HasValidSerializedId) {
                    _serializationId = Serializer.GetSerializedId();
                }
                return _serializationId;
            }
        }
        public bool HasValidSerializedId { get { return _serializationId >= 0; } }

        public void UpdateSerializedId(bool overrideCurrent = false) {
            if (_serializationId < 0 || overrideCurrent) {
                _serializationId = Serializer.GetSerializedId();
            }
        }

        public SerializedMetaData(SerializationInfo info, StreamingContext context) {
            _prefabPath = info.GetValue(nameof(_prefabPath), _prefabPath);
            _serializationId = (int)info.GetValue("SerializationId", typeof(int));
            _assetType = (AssetType)info.GetValue("AssetType", typeof(AssetType));
            _componentDiff = (SerializedComponentDifferences)info.GetValue("ComponentDiff", typeof(SerializedComponentDifferences));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_prefabPath), _prefabPath);
            info.AddValue("SerializationId", _serializationId, typeof(int));
            info.AddValue("AssetType", _assetType, typeof(AssetType));
            info.AddValue("ComponentDiff", _componentDiff, typeof(SerializedComponentDifferences));
        }

        public SerializedMetaData() {
            _componentDiff = new SerializedComponentDifferences();
        }

        public void SetDatabaseEntry(PrefabEntity entity) {
            _prefabPath = entity.ResourcePath;
            _assetType = AssetType.Prefab;
            _componentDiff = new SerializedComponentDifferences();
        }
        
    }
}
