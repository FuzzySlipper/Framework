using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CommandsContainer : ComponentContainer<Command> {
        public CommandsContainer(IList<Command> values) : base(values) {}

        public T GetCommand<T>() where T : Command {
            for (int i = 0; i < List.Count; i++) {
                if (List[i] is T) {
                    return (T) List[i];
                }
            }
            return null;
        }
    }
}
