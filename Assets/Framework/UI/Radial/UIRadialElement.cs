using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIRadialElement : MonoBehaviour, IComparer<UIRadialElement> {

        [SerializeField] private Button _button = null;
        [SerializeField] private TextMeshProUGUI _textLabel = null;
        [SerializeField] private Image _icon = null;
        [SerializeField] private CanvasGroup _canvasGroup = null;
        //private UIRadialMenu _parentRm;
        //private RectTransform _rt;
        //private int _assignedIndex = 0;
        private CanvasGroup _cg;
        private string _label;
        private Vector2[] _points = new Vector2[4];
        public Button Button { get { return _button; } }
        public RectTransform RectTr { get; private set; }

        private void Awake() {
            //_rt = gameObject.GetComponent<RectTransform>();
            _cg = gameObject.GetComponent<CanvasGroup>();
            if (_cg== null) {
                _cg = gameObject.AddComponent<CanvasGroup>();
            }
            if (_button== null) {
                _button = GetComponentInChildren<Button>();
            }
            RectTr = transform as RectTransform;
        }

        public void ElementClicked() {
        }

        public void SetAlpha(float value) {
            _canvasGroup.alpha = value;
        }

        public void AssignPoints(Vector2[] pnts) {
            _points = pnts;
        }

        public void Lerp(float t) {
            if (_points == null) {
                return;
            }
            RectTr.localPosition = (Vector2) Algorithms.GetCatmullRomPosition(t, _points[0], _points[1], _points[2], _points[3]);
        }

        public void AssignParent(UIRadialMenu menu, int index, MenuAction genericMenuAction) {
            //_parentRm = menu;
            //_assignedIndex = index;
            _label = genericMenuAction.Description;
            if (genericMenuAction.Icon != null) {
                _icon.overrideSprite = genericMenuAction.Icon;
                _textLabel.gameObject.SetActive(false);
                _icon.gameObject.SetActive(true);
            }
            else {
                _textLabel.text = _label;
                _textLabel.gameObject.SetActive(true);
                _icon.gameObject.SetActive(false);
            }
            _cg.blocksRaycasts = false;
            SetAlpha(0);
            ResetScale();
        }


        public void ResetScale() {
            _button.transform.localScale = Vector3.one;
        }

        public void SetScale(float scale) {
            _button.transform.localScale = new Vector3(scale, scale, scale);
        }

        public int Compare(UIRadialElement x, UIRadialElement y) {
            if (x == y) {
                return 0;
            }
            if (y.transform.IsChildOf(x.transform)) {
                return -1;
            }
            if (x.transform.IsChildOf(y.transform)) {
                return 1;
            }
            List<Transform> xparentList = GetParents(x.transform);
            List<Transform> yparentList = GetParents(y.transform);
            for (int xIndex = 0; xIndex < xparentList.Count; xIndex++) {
                if (y.transform.IsChildOf(xparentList[xIndex])) {
                    int yIndex = yparentList.IndexOf(xparentList[xIndex]) - 1;
                    xIndex -= 1;
                    return xparentList[xIndex].GetSiblingIndex() - yparentList[yIndex].GetSiblingIndex();
                }
            }
            return xparentList[xparentList.Count - 1].GetSiblingIndex() - yparentList[yparentList.Count - 1].GetSiblingIndex();
        }

        private List<Transform> GetParents(Transform t) {
            List<Transform> parents = new List<Transform>();
            parents.Add(t);
            while (t.parent != null) {
                parents.Add(t.parent);
                t = t.parent;
            }
            return parents;
        }
    }
}