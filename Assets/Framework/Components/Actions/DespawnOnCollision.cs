using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Lowest)]
    public class DespawnOnCollision : IComponent, IReceive<CollisionEvent> {

        public int Owner { get; set; }
        public IEntityPool Pool;

        public void Handle(CollisionEvent arg) {
            this.GetEntity().Destroy(Pool);
        }

        public DespawnOnCollision(IEntityPool pool) {
            Pool = pool;
        }
    }
}
