using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class UIIdentifyDrop : UIGenericDrop {
        protected override void OnDropEvent() {
            if (UIDragDropHandler.CurrentData == null) {
                if (UIDragDropHandler.Active) {
                    UIDragDropHandler.Return();
                }
                return;
            }
            if (UIDragDropHandler.CurrentData.Get<InventoryItem>()?.Identified ?? false) {
                UIFloatingText.Spawn("Doesn't need to be identified", transform as RectTransform, Color.green, UIFloatingText.Orientation.Center);
                UIDragDropHandler.Return();
                return;
            }
            var price = GameOptions.IdentifyEstimate(UIDragDropHandler.CurrentData);
            if (Player.DefaultCurrencyHolder.Value < price) {
                UIFloatingText.Spawn(string.Format("Identify Cost: {0} Not enough {1}", price, GameText.DefaultCurrencyLabel), transform as RectTransform, Color.green, UIFloatingText.Orientation.Center);
                UIDragDropHandler.Return();
                return;
            }
            UIModalQuestion.Set(ConfirmIdentify, string.Format("Identify for {0} {1}?", price, GameText.DefaultCurrencyLabel));
        }

        private void ConfirmIdentify(int id) {
            if (id > 0 || UIDragDropHandler.CurrentData == null) {
                if (UIDragDropHandler.Active) {
                    UIDragDropHandler.Return();
                }
                return;
            }
            var price = GameOptions.IdentifyEstimate(UIDragDropHandler.CurrentData);
            UIFloatingText.Spawn(string.Format("Identified for {0} {1}", price, GameText.DefaultCurrencyLabel), transform as RectTransform, Color.green, UIFloatingText.Orientation.Center);
            Player.DefaultCurrencyHolder.AddToValue(-price);
            UIDragDropHandler.CurrentData.Get<InventoryItem>().Identified = true;
            UIDragDropHandler.Return();
        }
    }
}
