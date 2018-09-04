using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UIDragPanel : MonoBehaviour, IPointerDownHandler, IDragHandler {

    [SerializeField] private RectTransform _panelRectTransform = null;

    private Vector2 _pointerOffset;
    private RectTransform _canvasRectTransform;

    void Awake () {
        if (_panelRectTransform== null) {
            _panelRectTransform = transform as RectTransform;
        }
    }

    void OnEnable() {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null) {
            _canvasRectTransform = canvas.transform as RectTransform;
        }
    }

    public void OnPointerDown (PointerEventData data) {
        _panelRectTransform.SetAsLastSibling ();
        RectTransformUtility.ScreenPointToLocalPointInRectangle (_panelRectTransform, data.position, data.pressEventCamera, out _pointerOffset);
    }

    public void OnDrag (PointerEventData data) {
        if (_panelRectTransform == null || _canvasRectTransform == null) {
            return;
        }

        Vector2 pointerPostion = ClampToWindow (data);

        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle (_canvasRectTransform, pointerPostion, 
            data.pressEventCamera, out localPointerPosition)) {
            _panelRectTransform.localPosition = localPointerPosition - _pointerOffset;
        }
    }

    Vector2 ClampToWindow (PointerEventData data) {
        Vector2 rawPointerPosition = data.position;

        Vector3[] canvasCorners = new Vector3[4];
        _canvasRectTransform.GetWorldCorners (canvasCorners);
        
        float clampedX = Mathf.Clamp (rawPointerPosition.x, canvasCorners[0].x, canvasCorners[2].x);
        float clampedY = Mathf.Clamp (rawPointerPosition.y, canvasCorners[0].y, canvasCorners[2].y);

        Vector2 newPointerPosition = new Vector2 (clampedX, clampedY);
        return newPointerPosition;
    }
}