using System;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelComrades {
    public abstract class UIDragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IOnCreate {

        public const float AudioVolume = 0.5f;

        [SerializeField] private Image _hoverGraphic = null;
        [SerializeField] private Color _hoverColor = Color.white;
        [SerializeField] private Image _dragImage = null;
        [SerializeField] private Animator _animator = null;
        [SerializeField] private Image _backgroundImage = null;

        private RectTransform _rectTransform;
        private Color _defaultColor;
        private int _listIndex = 0;
        private PrefabEntity _entity;
        public PrefabEntity Entity {
            get {
                if (_entity == null) {
                    _entity = GetComponent<PrefabEntity>();
                }
                return _entity;
            }
            private set { _entity = value; }
        }
        public RectTransform RectTransform { get { return _rectTransform; } }
        public int Index { get { return _listIndex; } set { _listIndex = value; } }
        public bool Active { get { return _dragImage.enabled; } }

        public Entity Data { get; protected set; }
        public abstract void OnDrop(PointerEventData eventData);
        public abstract void Clear();

        protected virtual void Awake() { 
            _rectTransform = transform as RectTransform;
            if (_hoverGraphic != null) {
                _defaultColor = _hoverGraphic.color;
            }
        }

        public void HoverStatus(bool active) {
            if (_hoverGraphic == null) {
                return;
            }
            _hoverGraphic.color = active ? _hoverColor : _defaultColor;
        }

        public void SetSprite(Sprite sprite) {
            //_dragImage.enabled = false;
            //if (_dragImage == null) {
            //    return;
            //}
            _dragImage.overrideSprite = sprite;
            _dragImage.enabled = sprite != null;
            //_dragImage.sprite = sprite;
        }

        public void SetSpriteStatus(bool status) {
            _dragImage.enabled = status;
        }

        public virtual void OnBeginDrag(PointerEventData eventData) {
            if (Data == null) {
                return;
            }
            StartDrag();
            UIDragDropHandler.IsUiDragging = true;
        }

        public void OnDrag(PointerEventData eventData) {}
        public void OnEndDrag(PointerEventData eventData) {}

        protected virtual void StartDrag() {
            SetSpriteStatus(false);
        }

        public virtual void OnPointerClick(PointerEventData eventData) {
            if (eventData.button == PointerEventData.InputButton.Left) {
                if (UIDragDropHandler.Active && UIDragDropHandler.Ready) {
                    OnDrop(null);
                }
                else if (Data != null && UIDragDropHandler.CanDrag) {
                    StartDrag();
                }
            }
            else if (Data != null && eventData.button == PointerEventData.InputButton.Right) {
                //GameData.TryUse(null, transform.position, false);
            }
        }

        protected virtual void DisplayHoverData() {
            Game.DisplayData(_backgroundImage, Data);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            HoverStatus(true);
            if (Data == null) {
                return;
            }
            if (_animator != null) {
                _animator.ResetTrigger(StringConst.ButtonNormalAnimName);
                _animator.SetTrigger(StringConst.ButtonSelectedAnimName);
            }
            DisplayHoverData();
        }

        
        public void OnPointerExit(PointerEventData eventData) {
            HoverStatus(false);
            if (_animator != null) {
                _animator.ResetTrigger(StringConst.ButtonSelectedAnimName);
                _animator.SetTrigger(StringConst.ButtonNormalAnimName);
            }
            UITooltip.main.HideTooltip();
        }

        public virtual void OnCreate(PrefabEntity entity) {
            if (_hoverGraphic != null) {
                _defaultColor = _hoverGraphic.color;
            }
            Entity = entity;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected() {
            if (Data != null) {
                UnityEditor.Handles.Label(transform.position, Data.DebugId);
            }
        }
#endif
    }
}
