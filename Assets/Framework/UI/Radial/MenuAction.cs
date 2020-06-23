using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PixelComrades {

    public abstract class MenuAction {
        public abstract string Description { get; }
        public abstract Sprite Icon { get; }
        public abstract bool TryUse();
        public abstract void OnFail(RectTransform pivot);

        public static List<MenuAction> GetList() {
            return ListPool<MenuAction>.Get();
        }

        public static void Store(List<MenuAction> list) {
            for (int i = 0; i < list.Count; i++) {
                if (list[i] is GenericMenuAction genericMenuAction) {
                    GenericMenuAction.Store(genericMenuAction);
                }
                else if (list[i] is ActionTemplateMenuAction actionTemplateMenuAction) {
                    ActionTemplateMenuAction.Store(actionTemplateMenuAction);
                }
            }
            list.Clear();
            ListPool<MenuAction>.Add(list);
        }
    }

    public class GenericMenuAction : MenuAction {

        private static GenericPool<GenericMenuAction> _menuActionPool = new GenericPool<GenericMenuAction>(15, action => { action.Clear(); });

        public static GenericMenuAction GetAction(string descr, Func<bool> del) {
            var newAction = _menuActionPool.New();
            newAction._description = descr;
            newAction.OnUseDel = del;
            return newAction;
        }

        public static GenericMenuAction GetAction(string descr, Func<bool> del, Action<RectTransform> onFail) {
            var newAction = _menuActionPool.New();
            newAction._description = descr;
            newAction.OnUseDel = del;
            newAction.OnFailDel = onFail;
            return newAction;
        }

        public static GenericMenuAction GetAction(string descr, Sprite icon, Func<bool> del) {
            var newAction = _menuActionPool.New();
            newAction._description = descr;
            newAction._icon = icon;
            newAction.OnUseDel = del;
            return newAction;
        }

        public static void Store(GenericMenuAction t1) {
            _menuActionPool.Store(t1);
        }

        private string _description;
        private Sprite _icon;
        public override string Description { get => _description; }
        public override Sprite Icon { get => _icon; }

        public override bool TryUse() {
            if (OnUseDel != null) {
                return OnUseDel();
            }
            return false;
        }

        public override void OnFail(RectTransform pivot) {
            if (OnFailDel != null) {
                OnFailDel(pivot);
            }
        }
        
        public Func<bool> OnUseDel;
        public Action<RectTransform> OnFailDel;

        public void Clear() {
            _description = "";
            OnUseDel = null;
            OnFailDel = null;
            _icon = null;
        }
    }

    public class ActionTemplateMenuAction : MenuAction {
        private static GenericPool<ActionTemplateMenuAction> _menuActionPool = new GenericPool<ActionTemplateMenuAction>(15);

        public static ActionTemplateMenuAction Get(ActionTemplate template, CharacterTemplate owner) {
            var action = _menuActionPool.New();
            action._owner = owner;
            action._actionTemplate = template;
            return action;
        }

        public static void Store(ActionTemplateMenuAction menuAction) {
            menuAction.Clear();
            _menuActionPool.Store(menuAction);
        }
        
        private ActionTemplate _actionTemplate;
        private bool _postStatusUpdates = true;
        private CharacterTemplate _owner;
        public override string Description { get => _actionTemplate.Config.Source.Name; }
        public override Sprite Icon { get => _actionTemplate.Icon.Sprite; }

        private void Clear() {
            _actionTemplate = null;
            _owner = null;
        }

        public override bool TryUse() {
            if (!_actionTemplate.CanAct(_actionTemplate, _owner)) {
                return false;
            }
            PlayerControllerSystem.Current.StartAction(_actionTemplate);
            return true;
        }

        public override void OnFail(RectTransform pivot) {
            if (_postStatusUpdates) {
                var statusUpdate = _owner.Get<StatusUpdateComponent>();
                if (statusUpdate != null && !string.IsNullOrEmpty(statusUpdate.Status)) {
                    UIFloatingText.Spawn(statusUpdate.Status, 2, pivot, Color.yellow);
                }
            }
        }
    }
}