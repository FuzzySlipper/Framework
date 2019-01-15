using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelComrades {
    public class UISimpleGameDataButton : MonoBehaviour, IPointerClickHandler, IOnCreate, IPointerEnterHandler, IPointerExitHandler, IPoolEvents, IReceive<StatusUpdate> {
        
        public const float AudioVolume = 0.5f;
        public System.Action<UISimpleGameDataButton, PointerEventData.InputButton> OnClickDel;
        public System.Action<UISimpleGameDataButton> OnDespawn;
        public System.Action<UISimpleGameDataButton> OnTooltip;

        [SerializeField] private Image _hoverGraphic = null;
        [SerializeField] private Color _hoverColor = Color.white;
        [SerializeField] private Animator _animator = null;
        [SerializeField] private Image _backgroundImage = null;
        [SerializeField] private Image _sprite = null;
        [SerializeField] private TextMeshProUGUI _amount = null;
        [SerializeField] private TextMeshProUGUI _label = null;
        [SerializeField] protected bool PlayAudio = true;
        [SerializeField] private bool _displayHoverData = true;
        [SerializeField] private bool _postStatusUpdates = true;

        public bool PostStatusUpdates { get { return _postStatusUpdates; } }
        private UnscaledTimer _statusTimer = new UnscaledTimer(0.25f);
        private RectTransform _rectTransform;
        private TooltipComponent _tooltip;
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
        public int Index { get { return _listIndex; } set { _listIndex = value; } }
        public Entity Data { get; protected set; }
        public TextMeshProUGUI Amount { get { return _amount; } }
        public TextMeshProUGUI Label { get { return _label; } }
        public RectTransform RectTransform {
            get {
                if (_rectTransform == null) {
                    _rectTransform = transform as RectTransform;
                }
                return _rectTransform;
            }
        }

        public virtual void OnCreate(PrefabEntity entity) {
            Entity = entity;
            _rectTransform = transform as RectTransform;
            if (_hoverGraphic != null) {
                _defaultColor = _hoverGraphic.color;
            }
        }

        public void OnPoolSpawned() {
        }

        public void OnPoolDespawned() {
            if (OnDespawn != null) {
                OnDespawn(this);
            }
            OnClickDel = null;
            OnDespawn = null;
            OnTooltip = null;
            Clear();
        }

        public virtual void Clear() {
            CleanUpCurrentItem();
            DisableSlotDetails();
            _tooltip = null;
            Data = null;
        }

        public void HoverStatus(bool active) {
            if (_hoverGraphic == null) {
                return;
            }
            _hoverGraphic.color = active ? _hoverColor : _defaultColor;
        }

        public void SetSprite(Sprite sprite) {
            if (_sprite == null) {
                return;
            }
            _sprite.sprite = sprite;
            if (sprite != null) {
                _sprite.enabled = true;
            }
        }

        public void SetSpriteStatus(bool status) {
            if (_sprite == null) {
                return;
            }
            _sprite.enabled = status;
        }

        public virtual void OnPointerClick(PointerEventData eventData) {
            if (OnClickDel != null) {
                OnClickDel(this, eventData.button);
                return;
            }
            if (eventData.button == PointerEventData.InputButton.Right) {
                Data.Get<UsableComponent>().TrySecondary(this);
            }
            else {
                DisplayData();
            }
        }

        public virtual void DisplayData() {
            Game.DisplayData(_backgroundImage, Data);
            if (_tooltip != null) {
                _tooltip.Tooltip();
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            HoverStatus(true);
            if (_animator != null) {
                _animator.ResetTrigger(StringConst.ButtonNormalAnimName);
                _animator.SetTrigger(StringConst.ButtonSelectedAnimName);
            }
            if (OnTooltip != null) {
                OnTooltip(this);
                return;
            }
            if (Data == null) {
                return;
            }
            if (_displayHoverData) {
                DisplayData();
            }
        }

        
        public void OnPointerExit(PointerEventData eventData) {
            HoverStatus(false);
            if (_animator != null) {
                _animator.ResetTrigger(StringConst.ButtonSelectedAnimName);
                _animator.SetTrigger(StringConst.ButtonNormalAnimName);
            }
            if (_displayHoverData) {
                Game.HideDataDisplay();
            }
        }

        protected void DisableSlotDetails() {
            SetSpriteStatus(false);
            if (_amount != null) {
                _amount.text = "";
            }
            if (_label != null) {
                _label.text = "";
            }
            Data = null;
        }

        public virtual void SetData(Entity data) {
            Clear();
            if (data == null) {
                DisableSlotDetails();
                return;
            }
            Data = data;
            _tooltip = data.Get<TooltipComponent>();
            SetSprite(data.Get<IconComponent>()?.Sprite);
            RefreshItem();
        }

        protected virtual void RefreshItem() {
            if (Data == null) {
                DisableSlotDetails();
                return;
            }
            if (Label != null) {
                Label.text = Data.Get<LabelComponent>()?.Text;
            }
            if (Amount != null) {
                Amount.text = Data.Get<AmountDescriptionComponent>().Text;
            }
        }

        protected virtual void CleanUpCurrentItem() {}

        protected void StatusMessages(string message, Color color) {
            if (_statusTimer.IsActive) {
                return;
            }
            _statusTimer.StartTimer();
            UIFloatingText.Spawn(message, transform as RectTransform, color, UIFloatingText.Orietation.Center);
        }


#if UNITY_EDITOR
        void OnDrawGizmosSelected() {
            if (Data != null) {
                UnityEditor.Handles.Label(transform.position, Data.Get<LabelComponent>()?.Text);
            }
        }
#endif
        public void Handle(StatusUpdate arg) {
            if (_postStatusUpdates) {
                StatusMessages(arg.Update, arg.Color);
            }
        }
    }
}
