using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CurrencyItem : IComponent, IReceive<ContainerStatusChanged> {
        private int _owner;
        public int Owner {
            get { return _owner; }
            set {
                if (_owner == value) {
                    return;
                }
                _owner = value;
                if (_owner < 0) {
                    return;
                }
                this.GetEntity().Add(new LabelComponent(string.Format("{0} {1}", _count, GameLabels.Currency)));
                this.GetEntity().GetOrAdd<UsableComponent>().OnUsableDel += OnControlUse;
            }
        }


        public void Handle(ContainerStatusChanged arg) {
            if (arg.EntityContainer != null && arg.EntityContainer.GetEntity().HasComponent<PlayerComponent>()) {
                TimeManager.StartUnscaled(WaitToRemove());
            }
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
            Player.Currency.AddToValue(_count);
            Despawn();
        }

        public bool OnControlUse(IComponent component) {
            Player.Currency.AddToValue(_count);
            Despawn();
            return true;
        }
        
    }
}
