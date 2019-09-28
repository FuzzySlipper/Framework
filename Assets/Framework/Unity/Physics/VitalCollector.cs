using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class VitalCollector : MonoBehaviour, IFakePhysicsCollision {

        [SerializeField] private float _amount = 5f;
        [SerializeField] private string _vital = "Vitals.Energy";

        void OnCollisionEnter(Collision collision) {
            if (collision.transform.CompareTag(StringConst.TagPlayer)) {
                var entity = UnityToEntityBridge.GetEntity(collision.collider);
                if (entity != null) {
                    entity.Post(new HealingEvent(_amount, entity, entity, _vital));
                    ItemPool.Despawn(gameObject);
                }
            }
        }

        public void Collision(Entity entity) {
            if (entity != null && entity.Tags.Contain(EntityTags.Player)) {
                entity.Post(new HealingEvent(_amount, entity, entity, _vital));
                ItemPool.Despawn(gameObject);
            }
        }
    }
}
