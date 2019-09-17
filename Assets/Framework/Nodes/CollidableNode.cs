using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CollidableNode : BaseNode {

        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<ColliderComponent> _collider = new CachedComponent<ColliderComponent>();
        private CachedComponent<StatsContainer> _stats = new CachedComponent<StatsContainer>();
        
        public Transform Tr { get => _tr.Value; }
        public Collider Collider { get => _collider.Value.Collider; }
        public StatsContainer Stats => _stats.Value;
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _tr, _collider, _stats
        };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(TransformComponent),
                typeof(ColliderComponent)
            };
        }
    }
}
