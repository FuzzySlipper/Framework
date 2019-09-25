using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PixelComrades {
    public interface IMapProvider {
        BaseCell GetCell(Point3 pos);
    }

    [Priority(Priority.Highest), AutoRegister]
    public sealed class MapSystem : SystemBase, IPeriodicUpdate {

        private IMapProvider _map;
        private NodeList<UnitOccupyingCellNode> _nodeList;
        private List<BaseCell> _occupiedCells = new List<BaseCell>(50);

        public MapSystem() {
            NodeFilter<UnitOccupyingCellNode>.New(UnitOccupyingCellNode.GetTypes());
            _nodeList = EntityController.GetNodeList<UnitOccupyingCellNode>();
        }

        public override void Dispose() {
            base.Dispose();
            _nodeList = null;
        }

        public void OnPeriodicUpdate() {
            for (int i = 0; i < _occupiedCells.Count; i++) {
                _occupiedCells[i].Occupied = null;
            }
            _occupiedCells.Clear();
            _nodeList.Run(UpdateNode);
        }

        private void UpdateNode(ref UnitOccupyingCellNode node) {
            if (node.Entity.IsDead() || node.Tr == null) {
                return;
            }
            var position = node.Tr.position.ToCellGridP3ZeroY();
            ref var gridPos = ref node.PositionComponent.GetReference();
            gridPos.Position = position;
            var cell = GetCell(position);
            if (cell == null) {
                return;
            }
            cell.Occupied = node.Entity;
            _occupiedCells.Add(cell);
        }


        public void SetMapProvider(IMapProvider provider) {
            _map = provider;
        }
        
        public BaseCell GetCell(Point3 pos) {
            if (_map == null) {
                return null;
            }
            return _map.GetCell(pos);
        }

        public bool LevelPositionIsFree(Point3 pos) {
            return GetCell(pos) == null;
        }
    }
}