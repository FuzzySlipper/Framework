using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public class CurrencyItem : IComponent, IReceive<ContainerStatusChanged> {

        public void Handle(ContainerStatusChanged arg) {
            this.GetEntity().Add(new LabelComponent(string.Format("{0} {1}", _count, GameText.DefaultCurrencyLabel)));
            if (arg.EntityContainer != null && arg.Entity.ParentId >= 0 && arg.Entity.GetParent().HasComponent<PlayerComponent>()) {
                TimeManager.StartUnscaled(WaitToRemove());
            }
        }

        public CurrencyItem(SerializationInfo info, StreamingContext context) {
            _count = info.GetValue(nameof(_count), _count);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_count), _count);
        }
        private int _count;

        public CurrencyItem(int count) {
            _count = count;
        }

        private void Despawn() {
            this.GetEntity().Destroy();
        }

        private IEnumerator WaitToRemove() {
            yield return 0.1f;
            Player.DefaultCurrencyHolder.AddToValue(_count);
            Despawn();
        }

        public bool OnControlUse(IComponent component) {
            Player.DefaultCurrencyHolder.AddToValue(_count);
            Despawn();
            return true;
        }
        
    }
}
