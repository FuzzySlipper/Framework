using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelComrades {
    public class UISellDrop : UIGenericDrop {

        protected override void OnDropEvent() {
            if (UIDragDropHandler.CurrentData == null) {
                if (UIDragDropHandler.Active) {
                    UIDragDropHandler.Return();
                }
            }
            else {
                //UIInventoryShop.TrySellDrag(transform);
            }
        }
    }
}
