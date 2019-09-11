using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelComrades;
using UnityEngine.UI;

public abstract class MenuActionLayer {
    
    protected List<MenuAction> Actions = new List<MenuAction>();
    protected System.Action CancelDel;
    protected Status CurrentStatus = Status.Disabled;
    protected UIRadialMenu Radial;
    protected float SavedCursorFill;
    protected float SavedCursorRotation;
    protected float CurrentAnimationPercent;
    private List<UIRadialElement> _currentRadials = new List<UIRadialElement>();

    public string Description { get; protected set; }
    public int Count { get { return Actions.Count; } }
    public bool IsTransitioningOut { get { return CurrentStatus == Status.Closing; } }
    public int SelectedIndex { get; protected set; }
    protected GameObject Cursor { get { return Radial.Cursor.gameObject; } }
    //protected Image CursorIconImage { get { return Radial.CursorIcon; } }
    //protected RectTransform CursorIconImageRect { get { return Radial.Cursor.rectTransform; } }
    //protected RectTransform CursorIconHolder { get { return Radial.CursorIconHolder; } }
    protected Image CursorImage { get { return Radial.Cursor; } }
    public List<UIRadialElement> CurrentRadials { get { return _currentRadials; } }

    public void Add(MenuAction action) {
        Actions.Add(action);
    }

    public void AddFirst(MenuAction action) {
        Actions.Insert(0, action);
    }

    public void AddRange(List<MenuAction> actions, int index) {
        Actions.InsertRange(index, actions);
    }

    public void Add(List<MenuAction> actions) {
        Actions.AddRange(actions);
    }

    public void Add(MenuAction[] actions) {
        Actions.AddRange(actions);
    }

    public void Remove(MenuAction action) {
        Actions.Remove(action);
    }

    public MenuAction this[int index] {
        get {
            if (index >= Actions.Count) {
                return Actions.LastElement();
            }
            if (index < 0) {
                return Actions[0];
            }
            return Actions[index];
        }
    }

    public void Set(List<MenuAction> newActions) {
        Actions = newActions;
    }

    public virtual void Init(UIRadialMenu radialMenu, float targetAngle) {
        if (CurrentStatus == Status.Opening || CurrentStatus == Status.Open) {
            return;
        }
        Radial = radialMenu;
        CurrentAnimationPercent = 0.0f;
        SavedCursorRotation = SavedCursorFill = 0;
        CurrentStatus = Status.Opening;
    }
    
    public virtual void StartClose() {
        if (CurrentStatus == Status.Closing || CurrentStatus == Status.Disabled) {
            return;
        }
        CurrentStatus = Status.Closing;
        CurrentAnimationPercent = 0;
    }

    protected void UpdateDisplayedText(string detailText, Sprite elementSprite, Color spriteColour) {
        Radial.SetCenterText(Description);
        Radial.DetailText.text = detailText;
        Radial.DetailSprite.overrideSprite = elementSprite;
        Radial.DetailSprite.color = spriteColour;
        Radial.DetailSprite.gameObject.SetActive(Radial.DetailSprite.sprite != null);
    }

    public abstract bool TransitionComplete();
    public abstract void Confirm();
    public abstract void Cancel();
    public abstract void Skip();
    public abstract void Pool();
    public abstract void UpdateLayer(float targetAngle, UIRadialMenu.ControlMethod method);
    public abstract void Confirm(int overrideSelect);

    protected enum Status {
        Disabled,
        Opening,
        Open,
        Closing
    }
}