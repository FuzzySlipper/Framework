using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CollidableNode : BaseNode {

        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<ColliderComponent> _collider = new CachedComponent<ColliderComponent>();
        
        public Transform Tr { get => _tr.Value; }
        public Collider Collider { get => _collider.Value.Collider; }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _tr, _collider
        };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(TransformComponent),
                typeof(ColliderComponent)
            };
        }
    }
}
