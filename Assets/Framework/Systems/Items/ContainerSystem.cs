using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class ContainerSystem : SystemBase  {
        
        private CircularBuffer<ActionStateEvent> _eventLog = new CircularBuffer<ActionStateEvent>(10, true);

        public ContainerSystem() {
            NodeFilter<ContainerItemNode>.Setup(ContainerItemNode.GetTypes());
        }

        public bool TryAdd(IEntityContainer holder, Entity item) {
            if (item == null || holder == null || holder.IsFull || holder.Contains(item)) {
                return false;
            }
            var inventoryItem = item.Get<InventoryItem>();
            if (inventoryItem == null) {
                return false;
            }
            if (inventoryItem.Inventory != null) {
                inventoryItem.Inventory.Remove(item);
            }
            ProcessHolderChange(holder, item, inventoryItem, holder.ContainerSystemAdd(item));
            return true;
        }

        public bool TryAdd(IEntityContainer holder, Entity item, int index) {
            return TryAddInternal(holder, item, index, false);
        }

        public bool TryReplace(IEntityContainer holder, Entity item, int index) {
            return TryAddInternal(holder, item, index, true);
        }

        public bool TrySwap(IEntityContainer holder, int index1, int index2) {
            var item1 = holder[index1];
            var item2 = holder[index2];
            if (item1 == null && item2 == null) {
                return false;
            }
            if (item1 != null) {
                item1.Get<InventoryItem>().Index = index2;
                holder.ContainerSystemSet(item1, index2);
            }
            if (item2 != null) {
                item2.Get<InventoryItem>().Index = index1;
                holder.ContainerSystemSet(item2, index1);
            }
            holder.Owner.Post(new ContainerChanged(holder));
            return true;
        }

        private bool TryAddInternal(IEntityContainer holder, Entity item, int index, bool replace) {
            if (item == null || holder == null || holder.Contains(item)) {
                return false;
            }
            var inventoryItem = item.Get<InventoryItem>();
            if (inventoryItem == null) {
                return false;
            }
            if (holder[index] != null) {
                if (!replace) {
                    return false;
                }
                holder.Remove(holder[index]);
            }
            if (inventoryItem.Inventory != null) {
                inventoryItem.Inventory.Remove(item);
            }
            holder.ContainerSystemSet(item, index);
            ProcessHolderChange(holder, item, inventoryItem, index);
            return true;
        }

        private void ProcessHolderChange(IEntityContainer holder, Entity item, InventoryItem inventoryItem, int newIndex) {
            inventoryItem.SetContainer(holder);
            item.ParentId = holder.Owner;
            inventoryItem.Index = newIndex;
            
            var msg = new ContainerStatusChanged(holder, item);
            item.Post(msg);
            holder.Owner.Post(msg);
            holder.Owner.Post(new ContainerChanged(holder));
        }
        
//
//        public void HandleGlobal(EntityDestroyed arg) {
//            var node = arg.Entity.GetNode<ContainerItemNode>();
//            if (node != null && node.Item.Inventory != null) {
//                node.Item.Inventory.Remove(arg.Entity);
//                node.Item.SetContainer(null);
//            }
//        }
    }

    public class ContainerItemNode : BaseNode {

        private CachedComponent<InventoryItem> _item = new CachedComponent<InventoryItem>();
        
        public InventoryItem Item { get => _item.Value; }
        
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _item, 
        };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(InventoryItem),
            };
        }
    }
    
}
