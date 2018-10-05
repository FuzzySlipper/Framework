using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EntityUIPoolSystem : SystemBase, IEntityPool {

        private Stack<Entity> _pooled = new Stack<Entity>();

        public void Store(Entity entity) {
            entity.GetNode<UINode>().Clear();
            entity.Pooled = true;
            entity.ParentId = -1;
            _pooled.Push(entity);
        }

        public UINode GetNode() {
            Entity entity;
            if (_pooled.Count > 0) {
                entity = _pooled.Pop();
            }
            else {
                entity = Entity.New("UIPool");
                entity.Add(new TransformComponent(null));
                entity.Add(new ModelComponent(null));
                entity.Add(new LabelComponent(""));
                entity.Add(new DescriptionComponent(""));
                entity.Add(new DataDescriptionComponent(""));
                entity.Add(new IconComponent(null));
            }
            return entity.GetNode<UINode>();
        }

    }
}
