using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public abstract class ComponentBase : IComponent, ISerializable {

        public Entity Entity { get; private set; }

        public int Owner {
            get { return Entity != null ? Entity.Id : -1; }
            set {
                if (Entity != null && Entity.Id == value) {
                    return;
                }
                SetEntity(EntityController.GetEntity(value));
            }
        }

        protected virtual void SetEntity(Entity entity) {
            Entity = entity;
        }

        protected ComponentBase(){}

        protected ComponentBase(SerializationInfo info, StreamingContext context) {
            Owner = info.GetValue(nameof(Owner), Owner);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Owner), Owner);
        }
    }
}
