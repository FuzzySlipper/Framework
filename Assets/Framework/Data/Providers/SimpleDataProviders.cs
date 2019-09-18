using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class VelocityMoverProvider : IDataFactory<VelocityMover> {
        public void AddComponent(Entity entity, DataEntry data) {
            entity.Add(new VelocityMover());
        }
    }

    public class CollisionCheckForwardProvider : IDataFactory<CollisionCheckForward> {
        public void AddComponent(Entity entity, DataEntry data) {
            entity.Add(new CollisionCheckForward(data.TryGetValue<float>(DatabaseFields.CollisionDistance, 10f)));
        }
    }

    public class CurrencyProvider : IDataFactory<CurrencyItem> {

        public void AddComponent(Entity entity, DataEntry data) {
            var currency = entity.Add(new CurrencyItem(data.TryGetValue("Count", 1)));
            entity.Add(new LabelComponent(string.Format("{0} {1}", currency.Count, GameText.DefaultCurrencyLabel)));
        }
    }
    
}
