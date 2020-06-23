using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EntityIdentifier : MonoBehaviour {
        public int EntityID = -1;

        private Entity _entity;
        public Entity Entity {
            get {
                if (_entity == null || _entity.Pooled || _entity.IsDestroyed() || _entity.Id != EntityID) {
                    _entity = EntityController.GetEntity(EntityID);
                }
                return _entity;
            }
        }
    }
}
