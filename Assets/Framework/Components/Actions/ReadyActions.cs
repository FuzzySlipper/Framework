using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {

    public struct ReadyActionsChanged : IEntityMessage {
        public int Index { get; }
        public ActionTemplate Action { get; }
        public ReadyActions Container { get; }

        public ReadyActionsChanged(int index, ActionTemplate action, ReadyActions container) {
            Index = index;
            Action = action;
            Container = container;
        }
    }

    [System.Serializable]
	public sealed class ReadyActions : IComponent {

        private ActionTemplate[] _actions;

        public ActionTemplate QueuedChange;
        public int QueuedSlot;

        public ActionTemplate this[int index] { get { return _actions[index]; } }
        public ReadyActions(int actionsCnt) {
            _actions = new ActionTemplate[actionsCnt];
        }

        public ReadyActions(SerializationInfo info, StreamingContext context) {
            _actions = info.GetValue(nameof(_actions), _actions);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_actions), _actions);
        }

        public ActionTemplate GetAction(int index) {
            return _actions[index];
        }

        public void EquipToEmpty(ActionTemplate actionConfig) {
            for (int i = 0; i < _actions.Length; i++) {
                if (_actions[i] == null) {
                    EquipAction(actionConfig, i);
                    return;
                }
            }
        }

        public void EquipAction(ActionTemplate template, int slot) {
            if (_actions.Length <= slot) {
                System.Array.Resize(ref _actions, slot + 1);
            }
            if (_actions[slot] != null) {
                RemoveAction(slot);
            }
            if (template == null) {
                return;
            }
            _actions[slot] = template;
            template.Config.EquippedSlot = slot;
            this.GetEntity().Post(new ReadyActionsChanged(slot, template, this));
        }

        public void RemoveAction(int slot) {
            if (_actions.Length <= slot) {
                return;
            }
            if (_actions[slot] != null && _actions[slot].Config.EquippedSlot == slot) {
                var action = _actions[slot];
                action.Config.EquippedSlot = -1;
                action.Entity.Post(new ReadyActionsChanged(-1, action, null));
            }
            _actions[slot] = null;
            this.GetEntity().Post(new ReadyActionsChanged(slot, null, this));
        }
    }
}
