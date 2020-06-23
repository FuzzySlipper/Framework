using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace PixelComrades {
    public class UIDropWorldPanel : MonoSingleton<UIDropWorldPanel>, IPointerEnterHandler, IPointerExitHandler, IDropHandler, IPointerClickHandler {

        [SerializeField] private float _dropDistance = 1.5f;
        [SerializeField] private float _minDistance = 2f;
        [SerializeField] private float _maxPlaceScreenY = 0.25f;

        private bool _enabled = true;
        private bool _active = false;

        public static bool Active { get { return main._active; } }
        public static bool Enabled { get { return main._enabled; } set { main._enabled = value; } }

        void Awake() {
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (!_enabled) {
                _active = false;
                return;
            }
            _active = true;
        }

        public void OnPointerExit(PointerEventData eventData) {
            _active = false;
        }

        public void OnDrop(PointerEventData eventData) {
            if (UIDragDropHandler.CurrentData == null) {
                return;
            }
            if (!_enabled || Game.InTown) {
                UIDragDropHandler.Return();
                return;
            }
            TryThrow(UIDragDropHandler.CurrentData);
        }

        public void TryThrow(Entity item) {
            var mouseRay = PlayerInputSystem.GetLookTargetRay;
            RaycastHit hit;
            if (Physics.Raycast(mouseRay, out hit, _dropDistance, LayerMasks.DropPanel)) {
                var use = hit.transform.GetComponent<EntityIdentifier>();
                if (use != null) {
                    var interaction = EntityController.GetEntity(use.EntityID).Get<ItemInteraction>();
                    if (interaction != null && interaction.Interaction(item)) {
                        UIDragDropHandler.Take();
                        return;
                    }
                }
                var screenPnt = CameraSystem.Cam.ScreenToViewportPoint(Mouse.current.position.ReadValue());
                if (screenPnt.y < _maxPlaceScreenY) {
                    var inventoryItem = item.Get<InventoryItem>();
                    if (inventoryItem != null && inventoryItem.Inventory.Remove(item)) {
                        UIDragDropHandler.Take();
                        World.Get<ItemSceneSystem>().Drop(hit.point + new Vector3(0, 0.5f, 0));
                        return;
                    }
                }
                if (hit.distance < _minDistance) {
                    UIDragDropHandler.Return();
                    return;
                }
            }
            UIDragDropHandler.Take();
            var mousePos = Mouse.current.position.ReadValue();
            var pos = CameraSystem.Cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 20));
            World.Get<ItemSceneSystem>().Throw(pos);
        }

        public virtual void OnPointerClick(PointerEventData eventData) {
            if (eventData.button == PointerEventData.InputButton.Left && UIDragDropHandler.Active && UIDragDropHandler.Ready) {
                OnDrop(null);
            }
        }
    }
}