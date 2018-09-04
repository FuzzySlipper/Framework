using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {

    public static class Messages {

        public const int SetupNewGame = 1000;
        public const int StartNewGame = 1001;
        public const int GameStarted = 0;
        public const int Loading = 1;
        public const int LoadingFinished = 2;
        public const int LevelClear = 3;
        public const int NewControllerLoaded = 4;
        public const int LevelLoadingFinished = 5;
        public const int LevelChanged = 6;
        
        public const int PlayerNewGame = 10;
        public const int PlayerDead = 11;
        public const int PlayerDamaged = 12;
        public const int PlayerAttachedChanged = 13;
        public const int SelectedActorChanged = 14;

        public const int PlayerMoving = 20;
        public const int PlayerRotated = 21;
        public const int PlayerReachedDestination = 22;
        public const int PlayerCharactersChanged = 23;

        public const int SwitchToggle = 50;
        public const int Locked = 51;
        public const int Unlocked = 52;
        public const int EncounterStatusChanged = 53;

        public const int TownEntered = 60;
        public const int TownExited = 61;

        public const int GlobalDataChanged = 70;
        public const int TurnBasedChanged = 71;
        public const int LoadTextChanged = 72;
        public const int PauseChanged = 73;
        public const int CameraFocusChanged = 74;

        public const int QuestEntriesChanged = 80;
        public const int QuestEventsChanged = 81;

        public const int MenuClosed = 90;
        public const int MenuOpened = 91;
        public const int MenuStatusChanged = 92;
        public const int MessageLog = 93;
        public const int ModifiersUpdated = 94;

        public const int CombatStarted = 100;
        public const int CombatEnded = 101;

        public const int CooldownTimerChanged = 500;

    }

    public enum DialogueMsg {
        PartyManagement = 0,
        ClassManagement = 1,
        Merchant = 3,
    }

    public static class MessageKit {
        private static Dictionary<int, List<Action>> _messageTable = new Dictionary<int, List<Action>>();

        public static void addObserver(int messageType, Action handler) {
            List<Action> list = null;
            if (!_messageTable.TryGetValue(messageType, out list)) {
                list = new List<Action>();
                _messageTable.Add(messageType, list);
            }

            if (!list.Contains(handler)) {
                _messageTable[messageType].Add(handler);
            }
        }


        public static void removeObserver(int messageType, Action handler) {
            List<Action> list = null;
            if (_messageTable.TryGetValue(messageType, out list)) {
                if (list.Contains(handler)) {
                    list.Remove(handler);
                }
            }
        }


        public static void post(int messageType) {
            List<Action> list = null;
            if (_messageTable.TryGetValue(messageType, out list)) {
                for (var i = list.Count - 1; i >= 0; i--) {
                    list[i]();
                }
            }
        }


        public static void clearMessageTable(int messageType) {
            if (_messageTable.ContainsKey(messageType)) {
                _messageTable.Remove(messageType);
            }
        }


        public static void clearMessageTable() {
            _messageTable.Clear();
        }
    }


    public static class MessageKit<U> {
        private static Dictionary<int, List<Action<U>>> _messageTable = new Dictionary<int, List<Action<U>>>();

        public static void addObserver(int messageType, Action<U> handler) {
            List<Action<U>> list = null;
            if (!_messageTable.TryGetValue(messageType, out list)) {
                list = new List<Action<U>>();
                _messageTable.Add(messageType, list);
            }

            if (!list.Contains(handler)) {
                _messageTable[messageType].Add(handler);
            }
        }


        public static void removeObserver(int messageType, Action<U> handler) {
            List<Action<U>> list = null;
            if (_messageTable.TryGetValue(messageType, out list)) {
                //if (list.Contains(handler)) {
                list.Remove(handler);
                //}
            }
        }


        public static void post(int messageType, U param) {
            List<Action<U>> list = null;
            if (_messageTable.TryGetValue(messageType, out list)) {
                for (var i = list.Count - 1; i >= 0; i--) {
                    list[i](param);
                }
            }
            MessageKit.post(messageType);
        }


        public static void clearMessageTable(int messageType) {
            if (_messageTable.ContainsKey(messageType)) {
                _messageTable.Remove(messageType);
            }
        }


        public static void clearMessageTable() {
            _messageTable.Clear();
        }
    }


    public static class MessageKit<U, V> {
        private static Dictionary<int, List<Action<U, V>>> _messageTable = new Dictionary<int, List<Action<U, V>>>();

        public static void addObserver(int messageType, Action<U, V> handler) {
            List<Action<U, V>> list = null;
            if (!_messageTable.TryGetValue(messageType, out list)) {
                list = new List<Action<U, V>>();
                _messageTable.Add(messageType, list);
            }

            if (!list.Contains(handler)) {
                _messageTable[messageType].Add(handler);
            }
        }


        public static void removeObserver(int messageType, Action<U, V> handler) {
            List<Action<U, V>> list = null;
            if (_messageTable.TryGetValue(messageType, out list)) {
                //if (list.Contains(handler)) {
                list.Remove(handler);
                //}
            }
        }


        public static void post(int messageType, U firstParam, V secondParam) {
            List<Action<U, V>> list = null;
            if (_messageTable.TryGetValue(messageType, out list)) {
                for (var i = list.Count - 1; i >= 0; i--) {
                    list[i](firstParam, secondParam);
                }
            }
        }


        public static void clearMessageTable(int messageType) {
            if (_messageTable.ContainsKey(messageType)) {
                _messageTable.Remove(messageType);
            }
        }


        public static void clearMessageTable() {
            _messageTable.Clear();
        }
    }

    public class MessageKitLocal {
        private Dictionary<int, List<Action>> _messageTable = new Dictionary<int, List<Action>>();
        private List<ISignalReceiver> _genericReceivers = new List<ISignalReceiver>();

        public void addObserver(ISignalReceiver generic) {
            _genericReceivers.Add(generic);
        }

        public void removeObserver(ISignalReceiver generic) {
            _genericReceivers.Remove(generic);
        }

        public void addObserver(int messageType, Action handler) {
            if (!_messageTable.TryGetValue(messageType, out var list)) {
                list = new List<Action>();
                _messageTable.Add(messageType, list);
            }

            if (!list.Contains(handler)) {
                _messageTable[messageType].Add(handler);
            }
        }


        public void removeObserver(int messageType, Action handler) {
            if (_messageTable.TryGetValue(messageType, out var list)) {
                //if (list.Contains(handler)) {
                list.Remove(handler);
                //}
            }
        }


        public void post(int messageType) {
            for (int i = 0; i < _genericReceivers.Count; i++) {
                _genericReceivers[i].Handle(messageType);
            }
            if (_messageTable.TryGetValue(messageType, out var list)) {
                for (var i = list.Count - 1; i >= 0; i--) {
                    list[i]();
                }
            }
        }


        public void clearMessageTable(int messageType) {
            if (_messageTable.ContainsKey(messageType)) {
                _messageTable.Remove(messageType);
            }
        }


        public void clearMessageTable() {
            _messageTable.Clear();
        }
    }


    public class MessageKitLocal<U> {
        private Dictionary<int, List<Action<U>>> _messageTable = new Dictionary<int, List<Action<U>>>();

        public void addObserver(int messageType, Action<U> handler) {
            List<Action<U>> list = null;
            if (!_messageTable.TryGetValue(messageType, out list)) {
                list = new List<Action<U>>();
                _messageTable.Add(messageType, list);
            }

            if (!list.Contains(handler)) {
                _messageTable[messageType].Add(handler);
            }
        }


        public void removeObserver(int messageType, Action<U> handler) {
            List<Action<U>> list = null;
            if (_messageTable.TryGetValue(messageType, out list)) {
                //if (list.Contains(handler)) {
                list.Remove(handler);
                //}
            }
        }


        public void post(int messageType, U param) {
            List<Action<U>> list = null;
            if (_messageTable.TryGetValue(messageType, out list)) {
                for (var i = list.Count - 1; i >= 0; i--) {
                    list[i](param);
                }
            }
        }


        public void clearMessageTable(int messageType) {
            if (_messageTable.ContainsKey(messageType)) {
                _messageTable.Remove(messageType);
            }
        }


        public void clearMessageTable() {
            _messageTable.Clear();
        }
    }



    public class MessageKitLocal<U, V> {
        private Dictionary<int, List<Action<U, V>>> _messageTable = new Dictionary<int, List<Action<U, V>>>();

        public void addObserver(int messageType, Action<U, V> handler) {
            List<Action<U, V>> list = null;
            if (!_messageTable.TryGetValue(messageType, out list)) {
                list = new List<Action<U, V>>();
                _messageTable.Add(messageType, list);
            }

            if (!list.Contains(handler)) {
                _messageTable[messageType].Add(handler);
            }
        }


        public void removeObserver(int messageType, Action<U, V> handler) {
            List<Action<U, V>> list = null;
            if (_messageTable.TryGetValue(messageType, out list)) {
                //if (list.Contains(handler)) {
                list.Remove(handler);
                //}
            }
        }


        public void post(int messageType, U firstParam, V secondParam) {
            List<Action<U, V>> list = null;
            if (_messageTable.TryGetValue(messageType, out list)) {
                for (var i = list.Count - 1; i >= 0; i--) {
                    list[i](firstParam, secondParam);
                }
            }
        }


        public void clearMessageTable(int messageType) {
            if (_messageTable.ContainsKey(messageType)) {
                _messageTable.Remove(messageType);
            }
        }


        public void clearMessageTable() {
            _messageTable.Clear();
        }
    }
}