using UnityEngine;
using System.Collections;
namespace PixelComrades {
    public class UIRepairDrop : UIGenericDrop {
        protected override void OnDropEvent() {
            if (UIDragDropHandler.CurrentData == null) {
                if (UIDragDropHandler.Active) {
                    UIDragDropHandler.Return();
                }
                return;
            }
            //if (UIDragDropHandler.CurrentData.Durability.CurrentPercent > 0.99f) {
            //    UIFloatingText.Spawn("Doesn't need repairs", transform as RectTransform, Color.green, UIFloatingText.Orietation.Center);
            //    UIDragDropHandler.Return();
            //    return;
            //}
            //var price = RpgSystem.RepairEstimate(UIDragDropHandler.CurrentData);
            //if (Player.Currency.Value < price) {
            //    UIFloatingText.Spawn(string.Format("Repair Cost: {0} Not enough {1}", price, GameLabels.Currency), transform as RectTransform, Color.green, UIFloatingText.Orietation.Center);
            //    UIDragDropHandler.Return();
            //    return;
            //}
            //UIModalQuestion.Set(ConfirmRepair, string.Format("Repair for {0} {1}?", price, GameLabels.Currency));
        }

        private void ConfirmRepair(int id) {
            if (id > 0 || UIDragDropHandler.CurrentData == null) {
                if (UIDragDropHandler.Active) {
                    UIDragDropHandler.Return();
                }
                return;
            }
            var price = GameOptions.RepairEstimate(UIDragDropHandler.CurrentData);
            UIFloatingText.Spawn(string.Format("Repaired for {0} {1}", price, GameText.DefaultCurrencyLabel), transform as RectTransform, Color.green, UIFloatingText.Orientation.Center);
            Player.DefaultCurrencyHolder.AddToValue(-price);
            //UIDragDropHandler.CurrentData.Durability.SetMax();
            UIDragDropHandler.Return();
        }
    }
}
