using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CommandsContainer : GenericContainer<Command> {
        public CommandsContainer(IList<Command> values) : base(values) {}

        public override void Add(Command item) {
            base.Add(item);
            item.Container = this;
            if (item.Owner < 0) {
                item.Owner = Owner;
            }
            else if (item.Owner != Owner) {
                var entity = item.GetEntity();
                if (entity.ParentId < 0) {
                    entity.ParentId = Owner;
                }
            }
        }

        public override void Remove(Command item) {
            base.Remove(item);
            if (item.Owner != Owner) {
                var entity = item.GetEntity();
                entity.ClearParent(Owner);
            }
        }

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
