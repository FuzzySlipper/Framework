using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelComrades {
    [ExecuteInEditMode]
    public class VerticalTextFit : UIBehaviour {

        [SerializeField] private float ratio = 0.7f;
        private Text text;
        private RectTransform tf;

        protected override void OnEnable() {
            UpdateSize();
        }

        protected override void OnTransformParentChanged() {
            UpdateSize();
        }

        protected override void OnValidate() {
            UpdateSize();
        }

#if UNITY_EDITOR
        private void Update() {
            if (!EditorApplication.isPlaying) {
                UpdateSize();
            }
        }
#endif
        private void UpdateSize() {
            if (text == null) {
                text = GetComponent<Text>();
            }
            if (tf == null) {
                tf = GetComponent<RectTransform>();
            }
            text.fontSize = (int) (tf.rect.height * ratio);
        }
    }
}