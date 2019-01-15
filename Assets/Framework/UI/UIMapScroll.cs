using UnityEngine;
using System.Collections;
using PixelComrades;
using UnityEngine.EventSystems;

namespace PixelComrades {
    public class UIMapScroll : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, 
        IScrollHandler, IDragHandler, IBeginDragHandler  {

        [SerializeField] private bool _isLevelMap = false;
        [SerializeField] private float _scrollSpeed = 2;
        [SerializeField] private float _moveSpeed = 0.1f;
        [SerializeField] private bool _reverseX = false;
        [SerializeField] private bool _reverseY = true;

        private bool _isActive = false;

        private Camera Cam {
            get {
                return !_isLevelMap ? Player.MinimapCamera : LevelMapCamera.main.MapCamera;
            }
        }


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
            Cam.orthographicSize += (-delta.y)*_scrollSpeed;
        }

        public void OnDrag(PointerEventData eventData) {
            if (!_isActive || !_isLevelMap) {
                return;
            }
            Cam.transform.position+= new Vector3(eventData.delta.x * (_reverseX ? -1 : 1), 0, eventData.delta.y * (_reverseY ? -1 : 1)) * _moveSpeed;
        }

        public void OnBeginDrag(PointerEventData eventData) {}
    }
}