using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {

    public struct CurrentActionsChanged : IEntityMessage {
        public int Index { get; }
        public Action Action { get; }
        public CurrentActions Container { get; }

        public CurrentActionsChanged(int index, Action action, CurrentActions container) {
            Index = index;
            Action = action;
            Container = container;
        }
    }

    [System.Serializable]
	public sealed class CurrentActions : IComponent {

        private CachedComponent<Action>[] _actions;

        public Action this[int index] { get { return _actions[index].Value; } }
        public CurrentActions(int actionsCnt) {
            _actions = new CachedComponent<Action>[actionsCnt];
            for (int i = 0; i < _actions.Length; i++) {
                _actions[i] = new CachedComponent<Action>();
            }
        }

        public CurrentActions(SerializationInfo info, StreamingContext context) {
            _actions = info.GetValue(nameof(_actions), _actions);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_actions), _actions);
        }

        public Action GetAction(int index) {
            return _actions[index];
        }

        public void EquipToEmpty(Action action) {
            for (int i = 0; i < _actions.Length; i++) {
                if (_actions[i] == null) {
                    EquipAction(action, i);
                    return;
                }
            }
        }

        public void EquipAction(Action action, int slot) {
            if (_actions.Length <= slot) {
                System.Array.Resize(ref _actions, slot +1);
            }
            if (action == null) {
                RemoveAction(slot);
                return;
            }
            _actions[slot].Set(action);
            action.EquippedSlot = slot;
            this.GetEntity().Post(new CurrentActionsChanged(slot, action, this));
        }

        public void RemoveAction(int slot) {
            if (_actions.Length <= slot) {
                return;
            }
            if (_actions[slot].Value != null && _actions[slot].Value.EquippedSlot == slot) {
                _actions[slot].Value.EquippedSlot = -1;
            }
            _actions[slot].Clear();
            this.GetEntity().Post(new CurrentActionsChanged(slot, null, this));
        }
    }
}
