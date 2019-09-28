using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace PixelComrades {
    public class UISimpleEquipDragDrop : UIItemDragDrop {

        private IEquipmentHolder _slot;

        public void SetSlot(IEquipmentHolder slot) {
            if (_slot != null) {
                _slot.OnItemChanged -= UpdateItem;
            }
            _slot = slot;
            _slot.OnItemChanged += UpdateItem;
            UpdateItem(_slot.Item);
        }

        protected override void TryDrop() {
            var newItem = UIDragDropHandler.CurrentData;
            if (World.Get<EquipmentSystem>().TryEquip(_slot, newItem)) {
                UIDragDropHandler.Take();
            }
            else {
                if (PlayAudio) {
                    AudioPool.PlayClip(StringConst.AudioDefaultItemReturn, transform.position, 0, AudioVolume);
                }
                UIFloatingText.InventoryMessage(_slot.LastEquipStatus, RectTransform);
                UIDragDropHandler.Return();
            }
        }

        protected override void TrySwap() {
            var newItem = UIDragDropHandler.CurrentData;
            Entity oldItem = Data;
            if (World.Get<EquipmentSystem>().TryEquip(_slot, newItem)) {
                UIDragDropHandler.Take();
                UIDragDropHandler.SetItem(oldItem);
            }
            else {
                if (PlayAudio) {
                    AudioPool.PlayClip(StringConst.AudioDefaultItemReturn, transform.position, 0, AudioVolume);
                }
                UIFloatingText.InventoryMessage(_slot.LastEquipStatus, RectTransform);
                UIDragDropHandler.Return();
                Player.MainInventory.Add(oldItem);
            }
        }

        public override void OnPointerClick(PointerEventData eventData) {
            if (eventData.button == PointerEventData.InputButton.Left) {
                if (UIDragDropHandler.Active) {
                    OnDrop(null);
                }
                else if (Data != null) {
                    StartDrag();
                }
            }
        }
        
        private void UpdateItem(Entity item) {
            if (Data == item) {
                return;
            }
            if (item != null) {
                SetItem(item);
            }
            else {
                Clear();
            }
        }
    }
}
