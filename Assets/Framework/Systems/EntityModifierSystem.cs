using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.High)]
    public class EntityModifierSystem : SystemBase, IMainSystemUpdate {

        private ManagedArray<ModifiersContainer> _list;
        private ManagedArray<ModifiersContainer>.RunDel<ModifiersContainer> _del;

        public EntityModifierSystem() {
            _del = Update;
        }

        public void OnSystemUpdate(float dt) {
            if (_list == null) {
                _list = EntityController.GetComponentArray<ModifiersContainer>();
            }
            if (_list != null) {
                _list.Run(_del);
                MessageKit.post(Messages.ModifiersUpdated);
            }
        }

        private void Update(ModifiersContainer mod) {
            mod.Update();
        }
    }
}
