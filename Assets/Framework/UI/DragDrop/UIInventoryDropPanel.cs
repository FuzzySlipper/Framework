using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace PixelComrades {
    public class UIInventoryDropPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler, IPointerClickHandler {
        
    
        private bool _active = false;

        public void OnPointerEnter(PointerEventData eventData) {
            _active = true;
        }

        public void OnPointerExit(PointerEventData eventData) {
            _active = false;
        }

        public void OnDrop(PointerEventData eventData) {
            if (UIDragDropHandler.CurrentData == null) {
                return;
            }
            if (!_active) {
                UIDragDropHandler.Return();
                return;
            }
            if (Player.MainInventory.Add(UIDragDropHandler.CurrentData)) {
                UIDragDropHandler.Take();
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData) {
            if (eventData.button == PointerEventData.InputButton.Left && UIDragDropHandler.Active && UIDragDropHandler.Ready) {
                OnDrop(null);
            }
        }
    }
}
