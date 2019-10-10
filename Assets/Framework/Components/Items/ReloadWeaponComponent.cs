using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class ReloadWeaponComponent : IComponent {
        private CachedComponent<AmmoComponent> _ammo = new CachedComponent<AmmoComponent>();
        
        public AmmoComponent Ammo { get => _ammo; }

        public void SetCurrentAmmo(AmmoComponent ammo) {
            if (ammo == null) {
                _ammo.Clear();
                return;
            }
            _ammo.Set(ammo);
        }
        public ReloadWeaponComponent(){}
        
        public ReloadWeaponComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
