using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public class EntityEventHub {

        private static SortByPriorityReceiver _msgSorter = new SortByPriorityReceiver();

        private Dictionary<int, List<System.Action>> _simpleHub = new Dictionary<int, List<System.Action>>();
        private List<ISignalReceiver> _genericReceivers = new List<ISignalReceiver>();

        public BufferedList<IReceive> MessageReceivers = new BufferedList<IReceive>();

        public void AddObserver(ISignalReceiver generic) {
            _genericReceivers.Add(generic);
        }

        public void AddObserver<T>(IReceive<T> handler) {
            if (MessageReceivers.CurrentList.Contains(handler)) {
                return;
            }
            MessageReceivers.CurrentList.Add(handler);
            MessageReceivers.CurrentList.Sort(_msgSorter);
        }

        public void AddObserver(IReceive handler) {
            if (MessageReceivers.CurrentList.Contains(handler)) {
                return;
            }
            MessageReceivers.CurrentList.Add(handler);
            MessageReceivers.CurrentList.Sort(_msgSorter);
        }

        public void RemoveObserver(ISignalReceiver generic) {
            _genericReceivers.Remove(generic);
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

        public void PostSignal(int messageType) {
            for (int i = 0; i < _genericReceivers.Count; i++) {
                _genericReceivers[i].Handle(messageType);
            }
            if (_simpleHub.TryGetValue(messageType, out var list)) {
                for (var i = list.Count - 1; i >= 0; i--) {
                    list[i]();
                }
            }
        }

        public void Post<T>(T msg) where T : IEntityMessage {
            MessageReceivers.Swap();
            var list = MessageReceivers.PreviousList;
            for (int i = 0; i < list.Count; i++) {
                if (list[i] == null) {
                    continue;
                }
                (list[i] as IReceiveRef<T>)?.Handle(ref msg);
            }
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
            _genericReceivers.Clear();
            MessageReceivers.Clear();
        }
    }
}
