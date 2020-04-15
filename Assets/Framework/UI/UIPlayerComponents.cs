using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelComrades;

public class UIPlayerComponents : MonoSingleton<UIPlayerComponents> {

    public static UISimpleInventoryList InventoryUI { get { return main._inventoryUI; } }
    public static RectTransform CenterMessage { get { return main._centerMessagePivot; } }
    public static RectTransform LeftMessage { get { return main._leftMessagePivot; } }

    private static ValueHolder<bool> _gameplayUiStatus = new ValueHolder<bool>(true);

    public static void DisableGameplayUi(string id) {
        SetGameplayStatus(false);
        _gameplayUiStatus.AddValue(false, id);
    }

    public static void RemoveDisableGameplayUi(string id) {
        _gameplayUiStatus.RemoveValue(id);
        if (_gameplayUiStatus.Value) {
            SetGameplayStatus(true);
        }
    }

    private static void SetGameplayStatus(bool status) {
        if (main == null) {
            return;
        }
        for (int i = 0; i < main._gameplayUI.Length; i++) {
            main._gameplayUI[i]?.SetActive(status);
        }
    }

    [SerializeField] private UISimpleInventoryList _inventoryUI = null;
    [SerializeField] private RectTransform _centerMessagePivot = null;
    [SerializeField] private RectTransform _leftMessagePivot = null;
    [SerializeField] private CanvasGroup[] _gameplayUI = new CanvasGroup[0];
}