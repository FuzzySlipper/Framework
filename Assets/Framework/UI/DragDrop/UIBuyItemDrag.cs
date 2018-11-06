using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace PixelComrades {
    public class UIBuyItemDrag : UIItemDragDrop {
        public override void OnDrop(PointerEventData eventData) {
            if (UIDragDropHandler.CurrentData == null) {
                if (UIDragDropHandler.Active) {
                    UIDragDropHandler.Return();
                }
            }
            else {
                //UIInventoryShop.TrySellDrag(transform);
            }
        }

        public override void OnPointerClick(PointerEventData eventData) {
            if (UIDragDropHandler.CurrentData == null) {
                if (UIDragDropHandler.Active) {
                    UIDragDropHandler.Return();
                }
            }
            else {
               // UIInventoryShop.TrySellDrag(transform);
                return;
            }
            TryBuy();
        }

        protected override void StartDrag() {
            TryBuy();
        }

        private void TryBuy() {
            if (Data == null) {
                return;
            }
            if (Player.MainInventory.IsFull) {
                StatusMessages(null, "Inventory full");
                return;
            }
            var sellPrice = InventoryItem.TotalPrice();
            if (Player.Currency.Value < sellPrice) {
                StatusMessages(null, string.Format("Costs {0}, Not enough {1}", sellPrice, GameLabels.Currency));
                return;
            }
            UIModalQuestion.Set(CheckBuy, string.Format("Buy for {0} {1}?", sellPrice, GameLabels.Currency));
        }

        private void CheckBuy(int index) {
            if (index > 0 || Data == null) {
                return;
            }
            var sellPrice = InventoryItem.TotalPrice();
            if (Player.MainInventory.TryAdd(Data)) {
                StatusMessages(null, string.Format("Bought for {0} {1}", sellPrice, GameLabels.Currency));
                Clear();
            }
        }
    }
}
