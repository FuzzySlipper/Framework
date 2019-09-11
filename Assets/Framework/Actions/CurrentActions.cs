using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {

    public struct CurrentActionsChanged : IEntityMessage {
        public int Index;
        public Action Action;
        public CurrentActions Container;

        public CurrentActionsChanged(int index, Action action, CurrentActions container) {
            Index = index;
            Action = action;
            Container = container;
        }
    }

    public class CurrentActionSlot : IEquipmentHolder, ISerializable {
        public Action<Entity> OnItemChanged { get; set; }
        public string TargetSlot { get { return "Current"; } }
        public string LastEquipStatus { get { return ""; } }
        public Entity Item { get; private set; }
        public Action ActionComponent { get; private set; }
        
        public CurrentActionSlot(){}

        public CurrentActionSlot(SerializationInfo info, StreamingContext context) {
            //TODO: what happens if we restore this before the Item is restored?
            var item = EntityController.Get(info.GetValue(nameof(Item), -1));
            if (item != null) {
                AddItem(item);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Item), Item.Id);
        }
        
        public bool AddItem(Entity item) {
            Item = item;
            ActionComponent = item.Get<Action>();
            OnItemChanged.SafeInvoke(item);
            return true;
        }

        public bool AddItem(Action item) {
            Item = item.Entity;
            ActionComponent = item;
            OnItemChanged.SafeInvoke(Item);
            return true;
        }

        public void Remove() {
            if (ActionComponent == null) {
                return;
            }
            ActionComponent.EquippedSlot = -1;
            Item = null;
            ActionComponent = null;
            OnItemChanged.SafeInvoke(null);
        }

        public Transform EquipTr { get { return null; } }
    }

    public class CurrentActions : IComponent {

        private CurrentActionSlot[] _actions;

        public CurrentActionSlot[] Actions { get => _actions; }

        public CurrentActions(int actionsCnt) {
            _actions = new CurrentActionSlot[actionsCnt];
            for (int i = 0; i < _actions.Length; i++) {
                _actions[i] = new CurrentActionSlot();
            }
        }

        public CurrentActions(SerializationInfo info, StreamingContext context) {
            _actions = info.GetValue(nameof(_actions), _actions);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_actions), _actions);
        }

        public Action GetAction(int index) {
            return _actions[index].ActionComponent;
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
            //Add(usable.Sequence);
            _actions[slot].AddItem(action);
            action.EquippedSlot = slot;
            this.GetEntity().Post(new CurrentActionsChanged(slot, action, this));
        }

        public void RemoveAction(int slot) {
            if (_actions.Length <= slot) {
                return;
            }
            //Remove(usable.Sequence);
            _actions[slot].Remove();
            this.GetEntity().Post(new CurrentActionsChanged(slot, null, this));
        }
    }
}
