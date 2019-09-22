using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class VisibleNode : BaseNode {

        private CachedComponent<RenderingComponent> _rendering = new CachedComponent<RenderingComponent>();
        private CachedComponent<LabelComponent> _label = new CachedComponent<LabelComponent>();
        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<RigidbodyComponent> _rb = new CachedComponent<RigidbodyComponent>();
        private CachedComponent<ColliderComponent> _collider = new CachedComponent<ColliderComponent>();
        
        public TransformComponent Tr { get => _tr.Value; }
        public RenderingComponent Rendering { get => _rendering; }
        public LabelComponent Label { get => _label; }
        public RigidbodyComponent Rb { get => _rb; }
        public ColliderComponent Collider { get => _collider; }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _label, _rendering, _tr, _rb,  _collider
        };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(RigidbodyComponent),
                typeof(RenderingComponent),
                typeof(LabelComponent),
            };
        }

        public void Setup(GameObject obj) {
            Rendering.Set(obj.GetComponent<RenderingWrapper>());
        }

        public Vector3 position {
            get {
                if (Tr != null) {
                    return Tr.position + Collider?.LocalCenter ?? new Vector3(0,1,0);
                }
                return Vector3.zero;
            }
        }
        public Quaternion rotation { get { return  Tr != null ? Tr.rotation : Quaternion.identity; } }

    }
}
