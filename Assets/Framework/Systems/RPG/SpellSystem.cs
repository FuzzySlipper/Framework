using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class SpellSystem : SystemBase, IReceive<ContainerStatusChanged> {

        public SpellSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(SpellsContainer)
            }));
        }

        public void Handle(ContainerStatusChanged arg) {
            if (arg.EntityContainer == null) {
                return;
            }
            var spellContainer = arg.EntityContainer.Owner.Get<SpellsContainer>();
            if (spellContainer == null) {
                return;
            }
            if (arg.EntityContainer.Count == 0 && arg.Entity == null) {
                spellContainer.KnownSpells.Clear();
                return;
            }
            if (arg.Entity == null) {
                return;
            }
            var spellData = arg.Entity.Get<SpellData>();
            if (spellData == null) {
                return;
            }
            if (spellContainer.KnownSpells.ContainsKey(spellData.Template.ID)) {
                arg.EntityContainer.Remove(arg.Entity);
            }
            else {
                spellContainer.KnownSpells.Add(spellData.Template.ID, spellData);
                spellContainer.AddToSpellLists(spellData);
            }
        }
        
    }
}
