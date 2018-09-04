using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIGenericButtonIncrement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPoolEvents, IOnCreate {

        [SerializeField] private Button _buttonIncrement = null;
        [SerializeField] private Button _buttonDecrement = null;
        [SerializeField] private TextMeshProUGUI _text = null;
        [SerializeField] private Image _iconHolder = null;
        [SerializeField] private Image _buttonImage = null;
        [SerializeField] private AudioClip _clickAudio = null;

        public System.Action<int, bool> OnButtonClicked;
        public System.Action<int, bool> OnButtonHover;

        private Color _defaultButtonColor;
        private PrefabEntity _entity;

        public int Index { get; set; }
        public Image IconHolder { get { return _iconHolder; } }
        public Image ButtonImage { get { return _buttonImage; } }
        public Button ButtonIncrement { get { return _buttonIncrement; } }
        public Button ButtonDecrement { get { return _buttonDecrement; } }

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

        public void ButtonIncrementClicked() {
            if (_clickAudio != null) {
                AudioPool.PlayClip(_clickAudio, transform.position, 0);
            }
            if (OnButtonClicked != null) {
                OnButtonClicked(Index, true);
            }
        }

        public void ButtonDecrementClicked() {
            if (_clickAudio != null) {
                AudioPool.PlayClip(_clickAudio, transform.position, 0);
            }
            if (OnButtonClicked != null) {
                OnButtonClicked(Index, false);
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

        public void OnPoolSpawned() {}

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