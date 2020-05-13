using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class InventorySystem : SystemBase, IReceive<DataDescriptionUpdating> {

        public InventorySystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(InventoryItem)
            }));
        }
        public void Handle(DataDescriptionUpdating arg) {
            var inventoryItem = arg.Data.GetEntity().Get<InventoryItem>();
            if (inventoryItem == null) {
                return;
            }
            FastString.Instance.Clear();
            FastString.Instance.AppendBoldLabelNewLine("Price", RulesSystem.TotalPrice(inventoryItem));
            if (inventoryItem.Count > 1) {
                FastString.Instance.AppendBoldLabelNewLine("Count", inventoryItem.Count);
            }
            if (!inventoryItem.Identified) {
                FastString.Instance.AppendNewLine("Unidentified");
            }
            FastString.Instance.AppendBoldLabelNewLine("Rarity", inventoryItem.Rarity.ToString());
            arg.Data.Text += FastString.Instance.ToString();
        }

        public static bool CanStack(InventoryItem target, Entity other) {
            if (target.Count >= target.MaxStack) {
                return false;
            }
            var owner = target.GetEntity();
            if (owner.Get<TypeId>().Id != other.Get<TypeId>().Id) {
                return false;
            }
            target.Count++;
            other.Destroy();
            var descr = owner.Get<DataDescriptionComponent>();
            if (descr != null) {
                descr.Text = "";
                owner.Post(new DataDescriptionUpdating(descr));
            }
            return true;
        }
    }
}
