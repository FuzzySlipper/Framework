using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    
    [System.Serializable]
	public sealed class Action : IComponent {

        public List<ActionLayer> Sequence = new List<ActionLayer>();
        public List<ICommandCost> Costs = new List<ICommandCost>();
        public float Range;
        public ActionFx Fx;
        public string WeaponModel;
        public bool Primary;
        public int EquippedSlot = -1;
        private CachedComponent<AmmoComponent> _ammo;

        public AmmoComponent Ammo {
            get {
                return _ammo;
            }
            set {
                _ammo = new CachedComponent<AmmoComponent>(value);
            }
        }
        public Entity Entity { get { return this.GetEntity(); } }

        public Action() {}

        public Action(SerializationInfo info, StreamingContext context) {
            Sequence = info.GetValue(nameof(Sequence), Sequence);
            Costs = info.GetValue(nameof(Costs), Costs);
            Range = info.GetValue(nameof(Range), Range);
            Fx = info.GetValue(nameof(Fx), Fx);
            WeaponModel = info.GetValue(nameof(WeaponModel), WeaponModel);
            Primary = info.GetValue(nameof(Primary), Primary);
            EquippedSlot = info.GetValue(nameof(EquippedSlot), EquippedSlot);
            _ammo = info.GetValue(nameof(_ammo), _ammo);
            Fx = ItemPool.LoadAsset<ActionFx>(info.GetValue(nameof(Fx), ""));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Sequence), Sequence);
            info.AddValue(nameof(Costs), Costs);
            info.AddValue(nameof(Range), Range);
            info.AddValue(nameof(Fx), Fx);
            info.AddValue(nameof(WeaponModel), WeaponModel);
            info.AddValue(nameof(Primary), Primary);
            info.AddValue(nameof(EquippedSlot), EquippedSlot);
            info.AddValue(nameof(_ammo), _ammo);
            info.AddValue(nameof(Fx), ItemPool.GetAssetLocation(Fx));
        }

        public bool CanStart(Entity entity) {
            if (entity == null) {
                return false;
            }
            for (int i = 0; i < Costs.Count; i++) {
                if (!Costs[i].CanAct(entity)) {
                    return false;
                }
            }
            return true;
        }

        public void ProcessCost(Entity entity) {
            for (int i = 0; i < Costs.Count; i++) {
                Costs[i].ProcessCost(entity);
            }
        }
    }
}
