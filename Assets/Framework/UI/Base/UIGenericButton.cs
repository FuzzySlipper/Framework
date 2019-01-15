using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIGenericButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPoolEvents, IOnCreate {

        [SerializeField] private Button _button = null;
        [SerializeField] private TextMeshProUGUI _text = null;
        [SerializeField] private Image _iconHolder = null;
        [SerializeField] private Image _buttonImage = null;
        [SerializeField] private AudioClip _clickAudio = null;

        public System.Action<int> OnButtonClicked;
        public System.Action<int, bool> OnButtonHover;

        private Color _defaultButtonColor;
        public int Index { get; set; }
        public Image IconHolder { get { return _iconHolder; } }
        public Image ButtonImage { get { return _buttonImage; } }
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

        void Awake() {
            if (_button == null) {
                _button = GetComponent<Button>();
            }
            if (_text == null) {
                _text = GetComponent<TextMeshProUGUI>();
            }
            if (_buttonImage != null) {
                _defaultButtonColor = _buttonImage.color;
            }
        }

        public void ChangeButtonColor(Color newColor) {
            if (_buttonImage != null) {
                _buttonImage.color = newColor;
            }
        }

        public void ResetButtonColor() {
            if (_buttonImage != null) {
                _buttonImage.color = _defaultButtonColor;
            }
        }

        public void SetText(string text) {
            if (_text != null) {
                _text.text = text;
            }
        }

        public void SetIcon(Sprite icon) {
            if (_iconHolder != null) {
                _iconHolder.sprite = icon;
                _iconHolder.enabled = _iconHolder.sprite != null;
            }
        }

        public void ButtonClicked() {
            if (_clickAudio != null) {
                AudioPool.PlayClip(_clickAudio, transform.position, 0);
            }
            if (OnButtonClicked != null) {
                OnButtonClicked(Index);
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData) {
            if (OnButtonHover != null) {
                OnButtonHover(Index, true);
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData) {
            if (OnButtonHover != null) {
                OnButtonHover(Index, false);
            }
        }

        public void OnPoolSpawned() {
        }

        public void OnPoolDespawned() {
            OnButtonClicked = null;
            OnButtonHover = null;
            if (_buttonImage != null) {
                _buttonImage.color = _defaultButtonColor;
            }
        }

        public void OnCreate(PrefabEntity entity) {
            Entity = entity;
        }
    }
}