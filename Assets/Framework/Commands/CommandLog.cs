using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CommandLog : ScriptableSingleton<CommandLog> {

        private const int LimitStore = 50;

        private bool _loggingActive = true;

        private Queue<Command> _pastCommands = new Queue<Command>();
        private List<Command> _currentCommands = new List<Command>();

        public List<Command> CurrentCommands { get { return _currentCommands; } }
        public Queue<Command> PastCommands { get { return _pastCommands; } }
        public static bool LoggingActive { get { return Main._loggingActive; } }

        public static void CommandActive(Command active) {
            if (!LoggingActive) {
                return;
            }
            Main._currentCommands.Add(active);
        }

        public static void CommandCompleted(Command command) {
            if (!LoggingActive) {
                return;
            }
            Main._currentCommands.Remove(command);
            Main.StoreInternal(command);
        }

        private void StoreInternal(Command command) {
            if (_pastCommands.Count >= LimitStore) {
                _pastCommands.Dequeue();
            }
            _pastCommands.Enqueue(command);
        }
    }
}
