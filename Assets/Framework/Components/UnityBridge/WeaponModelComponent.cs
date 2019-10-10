using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class WeaponModelComponent : IComponent {
        
        public string Prefab { get; }
        public IWeaponModel Loaded { get; private set; }

        public WeaponModelComponent(string prefab) {
            Prefab = prefab;
        }

        public void Set(IWeaponModel weaponModel) {
            Loaded = weaponModel;
            if (Loaded == null) {
                return;
            }
            Loaded.Setup();
            Loaded.SetFx(false);
        }
        
        public WeaponModelComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
