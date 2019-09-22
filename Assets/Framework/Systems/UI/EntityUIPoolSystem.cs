using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EntityUIPoolSystem : SystemBase, IEntityFactory {

        private Stack<Entity> _pooled = new Stack<Entity>();

        public bool TryStore(Entity entity) {
            entity.GetNode<UINode>().Clear();
            entity.Pooled = true;
            entity.ParentId = -1;
            _pooled.Push(entity);
            return true;
        }

        public UINode GetNode() {
            Entity entity;
            if (_pooled.Count > 0) {
                entity = _pooled.Pop();
            }
            else {
                entity = Entity.New("UIPool");
                entity.Add(new RenderingComponent(null));
                entity.Add(new LabelComponent(""));
                entity.Add(new DescriptionComponent(""));
                entity.Add(new DataDescriptionComponent(""));
                entity.Add(new IconComponent(null));
            }
            return entity.GetNode<UINode>();
        }

    }
}
