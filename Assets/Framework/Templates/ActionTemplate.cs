using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ActionTemplate : BaseActionTemplate {

        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<AmmoComponent> _ammo = new CachedComponent<AmmoComponent>();
        private CachedComponent<WeaponModelComponent> _weapon = new CachedComponent<WeaponModelComponent>();
        
        public TransformComponent Tr { get => _tr; }
        public AmmoComponent Ammo { get => _ammo; }
        public WeaponModelComponent Weapon { get => _weapon; }

        public override List<CachedComponent> GatherComponents {
            get {
                var baseList = base.GatherComponents;
                baseList.Add(_tr);
                baseList.Add(_weapon);
                baseList.Add(_ammo);
                return baseList;
            }
        }

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(ActionConfig),
                typeof(AmmoComponent),
            };
        }
    }
}
