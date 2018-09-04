using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;

namespace PixelComrades {
    public class UIHotbarSlot : UIDragDrop {

        [SerializeField] private TextMeshProUGUI _title = null;

        public void SetIndex(int index) {
            Index = index;
            if (_title != null) {
                _title.text = (index < 9 ? index + 1 : 0).ToString();
            }
        }

        public override void OnDrop(PointerEventData eventData) {
            if (!UIDragDropHandler.Active) {
                return;
            }
            if (UIDragDropHandler.CurrentData != null && !(UIDragDropHandler.CurrentData.GetParent()?.HasComponent<PlayerComponent>() ?? false)) {
                UIDragDropHandler.Return();
                return;
            }
            Set(UIDragDropHandler.CurrentData);
            UIDragDropHandler.Return();
        }

        public void Set(Entity data) {
            GameData = data;
            ////var refData =  data as HotbarReference;
            //if (refData != null) {
            //    refData.Slot.Clear();
            //    _refData = refData;
            //}
            //else {
            //    _refData = new HotbarReference(data, this);
            //}
            if (GameData != null) {
                SetSprite(GameData.Get<IconComponent>());
                UIHotBar.main.CheckForDuplicates(this);
            }
        }

        public override void Clear() {
            GameData = null;
           SetSpriteStatus(false);
        }

        public override void OnPointerClick(PointerEventData eventData) {
            if (UIDragDropHandler.Active) {
                OnDrop(null);
                return;
            }
            if (GameData == null) {
                return;
            }
            if (eventData.button == PointerEventData.InputButton.Right) {
                StartDrag();
            }
            else {
                UseSlot();
            }
        }

        public void UseSlot() {
            if (GameData == null) {
                return;
            }
            if (!GameData.Get<UsableComponent>().TryUse(this)) {
                UIFloatingText.InventoryMessage(GameData.Get<StatusUpdateComponent>(), RectTransform);
            }
        }

        public void UseSlotTarget() {
            if (GameData == null) {
                return;
            }
            if (!GameData.Get<Command>()?.TryStart(UICenterTarget.CurrentCharacter?.Entity ?? null) ?? false) {
                UIFloatingText.InventoryMessage(GameData.Get<StatusUpdateComponent>(), RectTransform);
            }
        }

        protected override void StartDrag() {
            base.StartDrag();
            UIDragDropHandler.Set(GameData, StopDrag, StopDrag, Clear);
        }

        private void StopDrag() {
            UIDragDropHandler.ClearData();
            SetSpriteStatus(true);
        }
    }
}
