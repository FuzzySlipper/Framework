using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public class PlayerSaveData : ISerializable {
        [SerializeField] private List<Entity> _savedEntities = new List<Entity>();
        [SerializeField] private List<float> _factionRep = new List<float>();
        [SerializeField] private Dictionary<string, string> _dataString = new Dictionary<string, string>();
        [SerializeField] private Dictionary<string, int> _dataInt = new Dictionary<string, int>();

        public List<Entity> SavedEntities { get => _savedEntities; }
        public List<float> FactionRep { get => _factionRep; }
        public Dictionary<string, string> DataString { get => _dataString; }
        public Dictionary<string, int> DataInt { get => _dataInt; }

        public PlayerSaveData(){}

        public PlayerSaveData(SerializationInfo info, StreamingContext context) {
            _factionRep = info.GetValue(nameof(_factionRep), _factionRep);
            _savedEntities = info.GetValue(nameof(_savedEntities), _savedEntities);
            _dataString = info.GetValue(nameof(_dataString), _dataString);
            _dataInt = info.GetValue(nameof(_dataInt), _dataInt);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_factionRep), _factionRep);
            info.AddValue(nameof(_savedEntities), _savedEntities);
            info.AddValue(nameof(_dataString), _dataString);
            info.AddValue(nameof(_dataInt), _dataInt);
        }
    }
}
