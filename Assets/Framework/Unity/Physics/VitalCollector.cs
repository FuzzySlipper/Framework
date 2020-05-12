using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class VitalCollector : MonoBehaviour, IFakePhysicsCollision {

        [SerializeField] private float _amount = 5f;
        [SerializeField] private string _vital = "Vitals.Energy";

        void OnCollisionEnter(Collision collision) {
            if (collision.transform.CompareTag(StringConst.TagPlayer)) {
                var entity = UnityToEntityBridge.GetEntity(collision.collider).GetTemplate<CharacterTemplate>();
                if (entity != null) {
                    World.Get<RulesSystem>().Post(new HealingEvent(null, _amount, entity, entity, _vital));
                    ItemPool.Despawn(gameObject);
                }
            }
        }

        public void Collision(Entity hitEntity) {
            var template = hitEntity.GetTemplate<CharacterTemplate>();
            if (template != null && template.Tags.Contain(EntityTags.Player)) {
                World.Get<RulesSystem>().Post(new HealingEvent(null, _amount, template, template, _vital));
                ItemPool.Despawn(gameObject);
            }
        }
    }
}
