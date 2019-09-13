using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class InventoryItem : IComponent, IReceive<DataDescriptionAdded> {
        public InventoryItem(int maxStack, int price, int rarity) {
            MaxStack = maxStack;
            Price = price;
            Rarity = rarity;
        }

        public InventoryItem(SerializationInfo info, StreamingContext context) {
            MaxStack = info.GetValue(nameof(MaxStack), MaxStack);
            Price = info.GetValue(nameof(Price), Price);
            Rarity = info.GetValue(nameof(Rarity), Rarity);
            Index = info.GetValue(nameof(Index), Index);
            Count = info.GetValue(nameof(Count), Count);
            Identified = info.GetValue(nameof(Identified), Identified);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(MaxStack), MaxStack);
            info.AddValue(nameof(Price), Price);
            info.AddValue(nameof(Rarity), Rarity);
            info.AddValue(nameof(Index), Index);
            info.AddValue(nameof(Count), Count);
            info.AddValue(nameof(Identified), Identified);
        }
        
        public int MaxStack { get; }
        public int Price { get; }
        public int Rarity { get; }

        public int Index = -1;
        public int Count = 1;
        public bool Identified = true;
        
        private CachedComponent<ItemInventory> _inventory = new CachedComponent<ItemInventory>();
        public ItemInventory Inventory { get { return _inventory.Value; } }

        public void SetContainer(ItemInventory container) {
            if (container == null) {
                _inventory.Clear();
            }
            else {
                _inventory.Set(container.GetEntity());
            }
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
