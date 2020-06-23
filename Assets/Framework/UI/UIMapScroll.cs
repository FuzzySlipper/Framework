using UnityEngine;
using System.Collections;
using PixelComrades;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace PixelComrades {
    public class UIMapScroll : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, 
        IScrollHandler, IDragHandler, IBeginDragHandler  {

        [SerializeField] private bool _isLevelMap = false;
        [SerializeField] private float _scrollSpeed = 2;
        [SerializeField] private float _moveSpeed = 0.1f;
        [SerializeField] private bool _reverseX = false;
        [SerializeField] private bool _reverseY = true;

        private bool _isActive = false;

        private IMapCamera MapCamera { get { return _isLevelMap ? Game.LevelMap : Game.MiniMap; } }

        public void OnPointerEnter(PointerEventData eventData) {
            _isActive = true;
        }

        public void OnPointerExit(PointerEventData eventData) {
            _isActive = false;
        }

        public void OnScroll(PointerEventData eventData) {
            if (!_isActive) {
                return;
            }

            Vector2 delta = eventData.scrollDelta;
            MapCamera.UpdateInput(Vector2.zero, (-delta.y) * _scrollSpeed, false);
        }

        public void OnDrag(PointerEventData eventData) {
            if (!_isActive) {
                return;
            }
            MapCamera.UpdateInput(new Vector2(eventData.delta.x * (_reverseX ? -1 : 1), eventData.delta.y * (_reverseY ? -1 : 1)) * _moveSpeed, 0, Mouse.current.leftButton.wasPressedThisFrame);
            //Cam.transform.position+= new Vector3(eventData.delta.x * (_reverseX ? -1 : 1), 0, eventData.delta.y * (_reverseY ? -1 : 1)) * _moveSpeed;
        }

        public void OnBeginDrag(PointerEventData eventData) {}
    }
}