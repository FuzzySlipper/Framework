using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IDataFactory {
        void AddComponent(Entity entity, List<string> config);
    }

    public interface IDataFactory<T>: IDataFactory{}

}
