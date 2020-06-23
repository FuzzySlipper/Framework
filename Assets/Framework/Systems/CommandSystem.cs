using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CommandSystem : SystemBase, IMainSystemUpdate {

        private BufferedList<Command> _commands = new BufferedList<Command>();
        private ManagedArray<Command>.RefDelegate _del;

        public CommandSystem() {
            _del = Update;
        }

        public bool TryAddCommand(Command cmd) {
            if (!cmd.CanStart()) {
                return false;
            }
            var otherCmd = GetCommand(cmd.Owner.Entity);
            if (otherCmd != null) {
                if (!otherCmd.CanBeReplacedBy(cmd)) {
                    cmd.Owner.Entity.Post(new StatusUpdate(cmd.Owner.Entity,"Can't replace current command"));
                    return false;
                }
                otherCmd.Cancel();
                _commands.Remove(otherCmd);
            }
            cmd.StartCommand();
#if DEBUG
            DebugLog.Add(cmd.Owner.Entity.DebugId + " started command " + cmd.GetType());
#endif
            _commands.Add(cmd);
            return true;
        }

        public Command GetCommand(int id) {
            for (int i = 0; i < _commands.Count; i++) {
                if (_commands[i]?.Owner.Entity.Id == id) {
                    return _commands[i];
                }
            }
            return null;
        }

        public Command GetCommandOrParent(int id) {
            for (int i = 0; i < _commands.Count; i++) {
                if (_commands[i]?.Owner.Entity.Id == id || _commands[i]?.Owner.Entity.ParentId == id) {
                    return _commands[i];
                }
            }
            return null;
        }

        private void Update(ref Command node) {
            if (node.TryComplete()) {
                node.Complete();
                _commands.Remove(node);
            }
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            _commands.Run(_del);
        }
    }
}
