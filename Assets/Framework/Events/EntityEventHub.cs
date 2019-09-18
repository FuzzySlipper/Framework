using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public class EntityEventHub {

        private static SortByPriorityReceiver _msgSorter = new SortByPriorityReceiver();

        private Dictionary<int, List<System.Action>> _simpleHub = new Dictionary<int, List<System.Action>>();
        private BufferedList<IReceive> _messageReceivers = new BufferedList<IReceive>();

        public int Count { get { return _simpleHub.Count + _messageReceivers.Count; } }
        
        public void AddObserver<T>(IReceive<T> handler) {
            if (_messageReceivers.CurrentList.Contains(handler)) {
                return;
            }
            _messageReceivers.CurrentList.Add(handler);
            _messageReceivers.CurrentList.Sort(_msgSorter);
        }

        public void AddObserver(IReceive handler) {
            if (_messageReceivers.CurrentList.Contains(handler)) {
                return;
            }
            _messageReceivers.CurrentList.Add(handler);
            _messageReceivers.CurrentList.Sort(_msgSorter);
        }

        public void AddObserver(int messageType, System.Action handler) {
            if (!_simpleHub.TryGetValue(messageType, out var list)) {
                list = new List<System.Action>();
                _simpleHub.Add(messageType, list);
            }
            if (!list.Contains(handler)) {
                _simpleHub[messageType].Add(handler);
            }
        }

        public void RemoveObserver(int messageType, System.Action handler) {
            if (_simpleHub.TryGetValue(messageType, out var list)) {
                list.Remove(handler);
            }
        }

        public void RemoveObserver<T>(IReceive<T> handler) {
            _messageReceivers.Remove(handler);
        }

        public void RemoveObserver(IReceive handler) {
            _messageReceivers.Remove(handler);
        }

        public void PostSignal(int messageType) {
            if (_simpleHub.TryGetValue(messageType, out var list)) {
                for (var i = list.Count - 1; i >= 0; i--) {
                    list[i]();
                }
            }
        }

        public void Post<T>(T msg) where T : IEntityMessage {
            _messageReceivers.Swap();
            var list = _messageReceivers.PreviousList;
            for (int i = 0; i < list.Count; i++) {
                if (list[i] == null) {
                    continue;
                }
                (list[i] as IReceive<T>)?.Handle(msg);
            }
        }

        public void ClearMessageTable(int messageType) {
            if (_simpleHub.ContainsKey(messageType)) {
                _simpleHub.Remove(messageType);
            }
        }

        public void Clear() {
            _simpleHub.Clear();
            _messageReceivers.Clear();
        }
    }
}
