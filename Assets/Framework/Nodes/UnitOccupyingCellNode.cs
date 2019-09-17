using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelComrades {
    public class UnitOccupyingCellNode : BaseNode {

        private CachedComponent<GridPosition> _position = new CachedComponent<GridPosition>();
        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        
        public GridPosition Position { get => _position; }
        public Transform Tr { get => _tr.Value; }
        public override List<CachedComponent> GatherComponents =>
            new List<CachedComponent>() { _position, _tr};

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(GridPosition),
            };
        }
    }
}
