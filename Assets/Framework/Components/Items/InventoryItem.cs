using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class InventoryItem : IComponent {
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
            return true;
        }

        public int TotalPrice() {
            return Price * Count;
        }
    }
}
