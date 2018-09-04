using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ModifiersContainer : GenericContainer<IEntityModifier> {

        public ModifiersContainer(IList<IEntityModifier> values) : base(values) {}

        public void AttachMod(IEntityModifier mod, Entity owner) {
            List.Add(mod);
            mod.OnAttach(owner, this.GetEntity());
            owner.Post(new ModifiersChanged(owner));
        }

        public void RemoveMod(string id) {
            for (int i = 0; i < List.Count; i++) {
                if (List[i].Id == id) {
                    List[i].OnRemove();
                    List.RemoveAt(i);
                    this.GetEntity().Post(new ModifiersChanged(this.GetEntity()));
                }
            }
        }

        public void RemoveMod(EntityModCategories categ) {
            for (int i = List.Count - 1; i >= 0; i--) {
                if (List[i].Category== categ) {
                    List[i].OnRemove();
                    List.RemoveAt(i);
                    this.GetEntity().Post(new ModifiersChanged(this.GetEntity()));
                }
            }
        }

        public void Update() {
            for (int i = List.Count - 1; i >= 0; i--) {
                List[i].OnUpdate();
                if (List[i].ShouldRemove()) {
                    List[i].OnRemove();
                    List.RemoveAt(i);
                    this.GetEntity().Post(new ModifiersChanged(this.GetEntity()));
                }
            }
        }
    }
}
