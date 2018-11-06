using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class InventoryItem : IComponent, IReceive<DataDescriptionAdded> {
        public InventoryItem(int maxStack, int price, int rarity) {
            MaxStack = maxStack;
            Price = price;
            Rarity = rarity;
        }

        public int Owner { get; set; }
        public int MaxStack { get; }
        public int Price { get; }
        public int Rarity { get; }

        public int Index = -1;
        public int Count = 1;
        public bool Identified = true;
        public EntityContainer Inventory { get; private set; }

        public void SetContainer(EntityContainer container) {
            Inventory = container;
        }

        public bool CanStack(Entity entity) {
            if (Count >= MaxStack) {
                return false;
            }
            if (this.Get<TypeId>().Id != entity.Get<TypeId>().Id) {
                return false;
            }
            Count++;
            entity.Destroy();
            var descr = this.Get<DataDescriptionComponent>();
            if (descr != null) {
                descr.Text = "";
                this.GetEntity().Post(new DataDescriptionAdded(descr));
            }
            return true;
        }

        public int TotalPrice() {
            return Price * Count;
        }

        public void Handle(DataDescriptionAdded arg) {
            FastString.Instance.Clear();
            FastString.Instance.AppendBoldLabelNewLine("Price", TotalPrice());
            if (Count > 1) {
                FastString.Instance.AppendBoldLabelNewLine("Count", Count);
            }
            if (!Identified) {
                FastString.Instance.AppendNewLine("Unidentified");
            }
            FastString.Instance.AppendBoldLabelNewLine("Rarity", GameData.Enums[EnumTypes.ItemRarity].GetNameAt(Rarity));
            arg.Data.Text += FastString.Instance.ToString();
        }
    }
}
