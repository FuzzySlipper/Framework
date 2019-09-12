using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class Action : IComponent {

        public List<ActionLayer> Sequence = new List<ActionLayer>();
        public List<ICommandCost> Costs = new List<ICommandCost>();
        public float Range;
        public AmmoComponent Ammo;
        public ActionFx Fx;
        public string WeaponModel;
        public bool Primary;
        public int EquippedSlot = -1;
        
        public Entity Entity { get { return this.GetEntity(); } }

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

        public virtual void ProcessCost(Entity entity) {
            for (int i = 0; i < Costs.Count; i++) {
                Costs[i].ProcessCost(entity);
            }
        }
    }
}
