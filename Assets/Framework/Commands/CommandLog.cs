using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CommandLog : ScriptableObject {

        private static CommandLog _static;
        public static CommandLog Instance {
            get {
                if (_static == null) {
                    _static = Resources.Load<CommandLog>("CommandLog");
                }
                return _static;
            }
        }

        private const int LimitStore = 50;

        private bool _loggingActive = true;

        private Queue<Command> _pastCommands = new Queue<Command>();
        private List<Command> _currentCommands = new List<Command>();

        public List<Command> CurrentCommands { get { return _currentCommands; } }
        public Queue<Command> PastCommands { get { return _pastCommands; } }
        public static bool LoggingActive { get { return Instance._loggingActive; } }

        public static void CommandActive(Command active) {
            if (!LoggingActive) {
                return;
            }
            Instance._currentCommands.Add(active);
        }

        public static void CommandCompleted(Command command) {
            if (!LoggingActive) {
                return;
            }
            Instance._currentCommands.Remove(command);
            Instance.StoreInternal(command);
        }

        private void StoreInternal(Command command) {
            if (_pastCommands.Count >= LimitStore) {
                _pastCommands.Dequeue();
            }
            _pastCommands.Enqueue(command);
        }
    }
}
