using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class GenericDataComponent : IComponent {
        
        [SerializeField] private Dictionary<string, string> _string = new Dictionary<string, string>();
        [SerializeField] private Dictionary<string, int> _int = new Dictionary<string, int>();

        public string GetString(string id) {
            return _string.TryGetValue(id, out var value) ? value : null;
        }

        public bool HasString(string id) {
            return _string.ContainsKey(id);
        }

        public void SetData(string id, string data) {
            _string.AddOrUpdate(id, data);
        }

        public void RemoveString(string id) {
            _string.Remove(id);
        }

        public int GetInt(string id) {
            return _int.TryGetValue(id, out var value) ? value : -1;
        }

        public bool HasInt(string id) {
            return _int.ContainsKey(id);
        }

        public void SetData(string id, int data) {
            _int.AddOrUpdate(id, data);
        }

        public void RemoveInt(string id) {
            _int.Remove(id);
        }
        
        public GenericDataComponent(){}
        
        public GenericDataComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }

    public class GenericDataTypes : StringEnum<GenericDataTypes> {
        public const string ItemType = "ItemType";
        public const string WeaponType = "WeaponType";
        public const string Size = "Size";
    }

    public class CharacterSizes : StringEnum<CharacterSizes> {
        public const string Small = "Small";
        public const string Medium = "Medium";
        public const string Large = "Large";
    }
    
}
