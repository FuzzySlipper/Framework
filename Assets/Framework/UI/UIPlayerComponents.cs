using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelComrades;
<<<<<<< HEAD

public class UIPlayerComponents : MonoSingleton<UIPlayerComponents> {

    public static UIInventory InventoryUI { get { return main._inventoryUI; } }
=======
using TMPro;

public class UIPlayerComponents : MonoSingleton<UIPlayerComponents> {

    public static UISimpleInventoryList InventoryUI { get { return main._inventoryUI; } }
>>>>>>> FirstPersonAction
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
<<<<<<< HEAD
            main._gameplayUI[i].SetActive(status);
        }
    }

    [SerializeField] private UIInventory _inventoryUI = null;
    [SerializeField] private RectTransform _centerMessagePivot = null;
    [SerializeField] private RectTransform _leftMessagePivot = null;
    [SerializeField] private CanvasGroup[] _gameplayUI = new CanvasGroup[0];
=======
            main._gameplayUI[i]?.SetActive(status);
        }
    }

    [SerializeField] private UISimpleInventoryList _inventoryUI = null;
    [SerializeField] private RectTransform _centerMessagePivot = null;
    [SerializeField] private RectTransform _leftMessagePivot = null;
    [SerializeField] private CanvasGroup[] _gameplayUI = new CanvasGroup[0];
    [SerializeField] private TextMeshProUGUI _upperRightText = null;
    [SerializeField] private UIGenericHotbar _hotbar = null;
    
    public static UIGenericHotbar Hotbar { get => main._hotbar; }
    public static TextMeshProUGUI UpperRightText { get => main._upperRightText; }
>>>>>>> FirstPersonAction
}