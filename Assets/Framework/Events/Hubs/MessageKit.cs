using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {

    public static class Messages {

        public const int SetupNewGame = 0;
        public const int StartNewGame = 1;
        public const int GameStarted = 2;
        public const int Loading = 3;
        public const int LoadingFinished = 4;
        public const int LevelClear = 5;
        public const int NewLevelControllerLoaded = 6;
        public const int LevelLoadingFinished = 7;
        public const int LevelChanged = 8;
        
        public const int PlayerNewGame = 10;
        public const int PlayerDead = 11;
        public const int PlayerDamaged = 12;
        public const int SelectedActorChanged = 14;
        public const int PlayerMoving = 15;
        public const int PlayerRotated = 16;
        public const int PlayerReachedDestination = 17;
        public const int PlayerCharactersChanged = 18;

        public const int SwitchToggle = 25;
        public const int Locked = 26;
        public const int Unlocked = 27;
        public const int EncounterStatusChanged = 28;
        
        public const int TownEntered = 30;
        public const int TownExited = 31;
        public const int QuestEntriesChanged = 32;
        public const int QuestEventsChanged = 33;
        public const int CombatStarted = 34;
        public const int CombatEnded = 35;

        public const int GlobalDataChanged = 40;
        public const int TurnBasedChanged = 41;
        public const int LoadTextChanged = 42;
        public const int PauseChanged = 43;
        public const int CameraFocusChanged = 44;
        public const int MenuClosed = 45;
        public const int MessageLog = 46;
        public const int ModifiersUpdated = 47;
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