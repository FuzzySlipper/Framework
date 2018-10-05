using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public enum UIAnchor {
        Center,
        BottomLeft,
        TopLeft,
        TopRight,
        BottomRight,
    }

    public static class UIAnchors {
        private static Dictionary<UIAnchor, RectTransform> _anchors = new Dictionary<UIAnchor, RectTransform>();

        public static void Register(UIAnchor root, RectTransform tr) {
            if (tr == null) {
                return;
            }
            _anchors.SafeAdd(root, tr);
        }

        public static RectTransform Get(this UIAnchor root) {
            return _anchors.TryGetValue(root, out var tr) ? tr : null;
        }

        public static RectTransform GetAnchor(UIAnchor root) {
            return _anchors.TryGetValue(root, out var tr) ? tr : null;
        }

        public static void SetAnchorPosition(this UIAnchor root, RectTransform tr) {
            switch (root) {
                default:
                    tr.SetAnchorsAndPivots(new Vector2(0.5f, 0.5f));
                    break;
                case UIAnchor.BottomLeft:
                    tr.SetAnchorsAndPivots(new Vector2(0f, 0f));
                    break;
                case UIAnchor.TopLeft:
                    tr.SetAnchorsAndPivots(new Vector2(0f, 1f));
                    break;
                case UIAnchor.TopRight:
                    tr.SetAnchorsAndPivots(new Vector2(1f, 1f));
                    break;
                case UIAnchor.BottomRight:
                    tr.SetAnchorsAndPivots(new Vector2(1f, 0f));
                    break;
            }
        }
    }

    public class UIAnchorSetter : MonoBehaviour {
        [SerializeField] private UIAnchor _target;

        void Awake() {
            UIAnchors.Register(_target, transform as RectTransform);
        }
    }
}
