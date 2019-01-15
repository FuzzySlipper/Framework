using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class UnitOccupyingCellNode : BaseNode {

        public CachedComponent<GridPosition> Position = new CachedComponent<GridPosition>();

        public override List<CachedComponent> GatherComponents =>
            new List<CachedComponent>() { Position, };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(GridPosition),
            };
        }
    }
}
