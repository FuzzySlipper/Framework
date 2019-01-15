using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IDataFactory {
        void AddComponent(Entity entity, DataEntry data);
    }

    public interface IDataFactory<T>: IDataFactory where T : IComponent{}
}
