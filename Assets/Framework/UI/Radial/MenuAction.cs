using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelComrades;

public class MenuAction {
    private static GenericPool<MenuAction> _actionPool = new GenericPool<MenuAction>(15, action => {action.Clear();});

    public static MenuAction GetAction(string descr, Func<bool> del) {
        var newAction = _actionPool.New();
        newAction.Description = descr;
        newAction.Del = del;
        return newAction;
    }

    public static MenuAction GetAction(string descr, Func<bool> del, Action<RectTransform> onFail) {
        var newAction = _actionPool.New();
        newAction.Description = descr;
        newAction.Del = del;
        newAction.OnFail = onFail;
        return newAction;
    }

    public static MenuAction GetAction(string descr, Sprite icon, Func<bool> del) {
        var newAction = _actionPool.New();
        newAction.Description = descr;
        newAction.Icon = icon;
        newAction.Del = del;
        return newAction;
    }

    public static MenuAction GetAction(string descr, Sprite icon, Func<bool> del, Action<RectTransform> onFail) {
        var newAction = _actionPool.New();
        newAction.Description = descr;
        newAction.Icon = icon;
        newAction.Del = del;
        newAction.OnFail = onFail;
        return newAction;
    }

    public static void ClearActions(List<MenuAction> actions) {
        for (int i = 0; i < actions.Count; i++) {
            Store(actions[i]);
        }
        actions.Clear();
    }

    public static void Store(MenuAction t1) {
        _actionPool.Store(t1);
    }

    public string Description;
    public Sprite Icon;
    public Func<bool> Del;
    public Action<RectTransform> OnFail;

    public void Clear() {
        Del = null;
        Icon = null;
    }
}