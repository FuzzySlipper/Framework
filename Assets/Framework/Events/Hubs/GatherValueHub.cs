using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PixelComrades {

    public class GatherValueHub<TInput, TInput2, TReturn>  {

        private Dictionary<int, List<Func<TInput, TInput2, TReturn, TReturn>>> _messageTable = new Dictionary<int, List<Func<TInput, TInput2, TReturn, TReturn>>>();

        public void addObserver(int messageType, Func<TInput, TInput2, TReturn, TReturn> handler) {
            List<Func<TInput, TInput2, TReturn, TReturn>> list = null;
            if (!_messageTable.TryGetValue(messageType, out list)) {
                list = new List<Func<TInput, TInput2, TReturn, TReturn>>();
                _messageTable.Add(messageType, list);
            }

            if (!list.Contains(handler)) {
                _messageTable[messageType].Add(handler);
            }
        }
    
        public void removeObserver(int messageType, Func<TInput, TInput2, TReturn, TReturn> handler) {
            List<Func<TInput, TInput2, TReturn, TReturn>> list = null;
            if (_messageTable.TryGetValue(messageType, out list)) {
                list.Remove(handler);
            }
        }
    
        public TReturn post(int messageType, TInput input1, TInput2 input2) {
            List<Func<TInput, TInput2, TReturn, TReturn>> list = null;
            if (!_messageTable.TryGetValue(messageType, out list)) {
                return default(TReturn);
            }
            TReturn total = default(TReturn);
            for (var i = list.Count - 1; i >= 0; i--) {
                total = list[i](input1, input2, total);
            }
            return total;
        }

        public List<Func<TInput, TInput2, TReturn, TReturn>> GetList(int messageType) {
            List<Func<TInput, TInput2, TReturn, TReturn>> list = null;
            if (_messageTable.TryGetValue(messageType, out list)) {
                return list;
            }
            return null;
        }

        public void clearMessageTable() {
            _messageTable.Clear();
        }

        public void LogObservers(int messageType) {
		    if (_messageTable.ContainsKey(messageType) == false || _messageTable[messageType].Count == 0) {
			    Debug.LogFormat("MessageType: {0} has no observers", messageType);
			    return;
		    }
		    var sb = new StringBuilder();
		    sb.AppendLine("MessageType: " + messageType + " has " + _messageTable[messageType].Count + " Observers");
		    for (var i = 0; i < _messageTable[messageType].Count; i++) {
			    var action = _messageTable[messageType][i];
			    sb.AppendLine("Target: " + action.Target + "\t Method: " + action.Method);
		    }
		    Debug.Log(sb.ToString());
	    }
    }
}