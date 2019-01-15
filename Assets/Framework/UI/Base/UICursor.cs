using UnityEngine;
using System.Collections;
using PixelComrades;
using UnityEngine.UI;

public class UICursor : MonoSingleton<UICursor> {
    
    [SerializeField] private Sprite _crossHair = null;
    [SerializeField] private Sprite _useCursor = null;
    [SerializeField] private Sprite _defaultCursor = null;
    [SerializeField] private Image _cursorSprite = null;

    public static Sprite CrossHair { get { return main._crossHair; } }
    public static Sprite UseCursor { get { return main._useCursor; } }
    public static Sprite DefaultCursor { get { return main._defaultCursor; } }

    public static void SetActive(bool status) {
        main._active = status;
        main._cursorSprite.enabled = status;
    }

    private bool _active = true;
    private bool _focused = true;

    void Awake() {
        main = this;
        _focused = true;
        MessageKit.addObserver(Messages.LoadingFinished, EnableCursor);
        MessageKit.addObserver(Messages.Loading, DisableCursor);
    }

    void OnApplicationFocus(bool focusStatus) {
        _focused = focusStatus;
    }

    private void DisableCursor() {
        _cursorSprite.enabled = false;
    }

    private void EnableCursor() {
        _cursorSprite.enabled = true;
    }

    void Update() {
        if (!_active) {
            return;
        }
        if (_focused) {
            transform.position = Input.mousePosition;
        }
        //if (UITooltip.Active) {
        //    SetCursor(_useCursor);
        //}
        //else if (Cursor.lockState != CursorLockMode.Locked) {
        //    SetCursor(_defaultCursor);
        //}
        //else {
        //    SetCursor(_crossHair);
        //}
    }

    public void SetCursor(Sprite newCursor) {
        if (_cursorSprite.sprite == newCursor) {
            return;
        }
        //Cursor.SetCursor(newCursor, GetSize(newCursor), CursorMode.Auto);
        _cursorSprite.sprite = newCursor;
    }
}
