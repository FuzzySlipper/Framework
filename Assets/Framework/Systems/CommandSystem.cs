using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CommandSystem : SystemBase, IMainSystemUpdate {

        private BufferedList<Command> _commands = new BufferedList<Command>();

        public bool TryAddCommand(Command cmd) {
            if (!cmd.CanStart()) {
                return false;
            }
            var otherCmd = GetCommand(cmd.EntityOwner);
            if (otherCmd != null) {
                if (!otherCmd.CanBeReplacedBy(cmd)) {
                    cmd.EntityOwner.PostAll(new StatusUpdate("Can't replace current command"));
                    return false;
                }
                otherCmd.Cancel();
                _commands.Remove(otherCmd);
            }
            cmd.StartCommand();
            _commands.Add(cmd);
            return true;
        }

        public Command GetCommand(int id) {
            for (int i = 0; i < _commands.Count; i++) {
                if (_commands[i]?.EntityOwner.Id == id) {
                    return _commands[i];
                }
            }
            return null;
        }

        public Command GetCommandOrParent(int id) {
            for (int i = 0; i < _commands.Count; i++) {
                if (_commands[i]?.EntityOwner.Id == id || _commands[i]?.EntityOwner.ParentId == id) {
                    return _commands[i];
                }
            }
            return null;
        }

        public void OnSystemUpdate(float dt) {
            _commands.Swap();
            for (int i = 0; i < _commands.PreviousList.Count; i++) {
                if (_commands.PreviousList[i] == null) {
                    continue;
                }
                if (_commands.PreviousList[i].TryComplete()) {
                    _commands.PreviousList[i].Complete();
                    _commands.CurrentList.Remove(_commands.PreviousList[i]);
                }
            }
        }
    }
}
