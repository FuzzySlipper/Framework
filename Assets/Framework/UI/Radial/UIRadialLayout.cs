using UnityEngine;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIRadialLayout : LayoutGroup {

        [SerializeField] private float _distance = 0;
        [Range(0f, 360f)] [SerializeField] private float _minAngle = 0, _maxAngle = 0, _startAngle = 0;
        [SerializeField] private bool _onlyLayoutVisible = false;

        private void CalculateRadial() {
            m_Tracker.Clear();
            if (transform.childCount == 0) {
                return;
            }
            int childrenToFormat = 0;
            if (_onlyLayoutVisible) {
                for (int i = 0; i < transform.childCount; i++) {
                    RectTransform child = (RectTransform) transform.GetChild(i);
                    if (child != null && child.gameObject.activeSelf) {
                        ++childrenToFormat;
                    }
                }
            }
            else {
                childrenToFormat = transform.childCount;
            }
            float fOffsetAngle = (_maxAngle - _minAngle) / childrenToFormat;
            float fAngle = _startAngle;
            for (int i = 0; i < transform.childCount; i++) {
                RectTransform child = (RectTransform) transform.GetChild(i);
                if (child != null && (!_onlyLayoutVisible || child.gameObject.activeSelf)) {
                    //Adding the elements to the tracker stops the user from modifying their positions via the editor.
                    m_Tracker.Add(
                        this, child,
                        DrivenTransformProperties.Anchors |
                        DrivenTransformProperties.AnchoredPosition |
                        DrivenTransformProperties.Pivot);
                    Vector3 vPos = new Vector3(Mathf.Cos(fAngle * Mathf.Deg2Rad), Mathf.Sin(fAngle * Mathf.Deg2Rad), 0);
                    child.localPosition = vPos * _distance;
                    //Force objects to be center aligned, this can be changed however I'd suggest you keep all of the objects with the same anchor points.
                    child.anchorMin = child.anchorMax = child.pivot = new Vector2(0.5f, 0.5f);
                    fAngle += fOffsetAngle;
                }
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            CalculateRadial();
        }
#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();
            CalculateRadial();
        }
#endif
        public override void CalculateLayoutInputHorizontal() {
            CalculateRadial();
        }

        public override void CalculateLayoutInputVertical() {
            CalculateRadial();
        }

        public override void SetLayoutHorizontal() {
        }

        public override void SetLayoutVertical() {
        }
    }
}