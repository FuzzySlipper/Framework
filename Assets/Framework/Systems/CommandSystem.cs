using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public class CommandSystem : SystemBase<CommandSystem>, IMainSystemUpdate {
        
        private BufferedList<Command> _commands = new BufferedList<Command>();
        private ManagedArray<Command>.RefDelegate _del;
        private static Dictionary<System.Type, Queue<Command>> _commandPool = new Dictionary<System.Type, Queue<Command>>();

        public CommandSystem() {
            _del = Update;
        }

        public static T GetCommand<T>(TurnBasedCharacterTemplate character) where T : Command, new() {
            var type = typeof(T);
            if (!_commandPool.TryGetValue(type, out var queue)) {
                queue = new Queue<Command>();
                _commandPool.Add(type, queue);
            }
            T cmd;
            if (queue.Count == 0) {
                cmd = new T();
            }
            else {
                cmd = (T) queue.Dequeue();
            }
            cmd.Owner = character;
            return cmd;
        }

        public static void Store<T>(T command) where T : Command {
            var type = typeof(T);
            if (!_commandPool.TryGetValue(type, out var queue)) {
                queue = new Queue<Command>();
                _commandPool.Add(type, queue);
            }
            command.Clear();
            queue.Enqueue(command);
        }

        public bool TryAddCommand(Command cmd) {
            if (!cmd.CanStart()) {
                return false;
            }
            var otherCmd = GetCommand(cmd.Owner.Entity);
            if (otherCmd != null) {
                if (!otherCmd.CanBeReplacedBy(cmd)) {
                    cmd.Owner.Post(new StatusUpdate(cmd.Owner,"Can't replace current command"));
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

        private void Update(ref Command node) {
            if (node.TryComplete()) {
                node.Complete();
                 TurnBasedSystem.Get.CommandComplete(node.Owner);
                _commands.Remove(node);
                Store(node);
            }
        }

        [Command("checkCommands")]
        public static void CheckCommands() {
            Get._commands.Run(
                c => {
                    Console.Log(string.Format("{0} Time Active {1} Status {2}", c.Owner.Entity.DebugId, c.TimeActive, c.GetStatus()));
                });
        }

        [Command("clearCommands")]
        public static void ClearCommands() {
            Get._commands.Run(
                c => {
                    c.Cancel();
                });
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            _commands.Run(_del);
        }
    }
}
