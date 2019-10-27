using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CollidableTemplate : BaseTemplate {

        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<ColliderComponent> _collider = new CachedComponent<ColliderComponent>();
        private CachedComponent<ColliderDetailComponent> _colliderDetail = new CachedComponent<ColliderDetailComponent>();
        
        public TransformComponent Tr { get => _tr.Value; }
        public Collider Collider { get => _collider.Value.Collider; }
        public ColliderDetailComponent DetailCollider { get => _colliderDetail.Value; }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _tr, _collider, _colliderDetail
        };

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(TransformComponent),
                typeof(ColliderComponent)
            };
        }
    }
}
