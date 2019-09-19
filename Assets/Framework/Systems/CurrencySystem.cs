using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class CurrencySystem : SystemBase, IReceive<ContainerStatusChanged> {

        public CurrencySystem() {
            EntityController.RegisterReceiver<CurrencyItem>(this);
        }
        public void Handle(ContainerStatusChanged arg) {
            if (arg.Entity == null) {
                return;
            }
            var currencyItem = arg.Entity.Get<CurrencyItem>();
            if (currencyItem == null) {
                return;
            }
            if (arg.EntityContainer != null && arg.Entity.ParentId >= 0 && arg.Entity.GetParent().HasComponent<PlayerComponent>()) {
                Player.DefaultCurrencyHolder.AddToValue(currencyItem.Count);
                arg.Entity.Destroy();
            }
        }
    }
}
