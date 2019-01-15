using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace PixelComrades {
    public class SimpleThreadSafeGrid : IPathfindingGrid {

        private const int CellNone = 0;
        private const int CellWalkable = 1 << 0;
        private const int CellEdge = 1 << 1;

        private class SimpleCell : IPathfindingGridCell {
            public int Cost;
            public int Flags;

            public int Occupied = -1;
            public int PathClaimed = -1;
            public int TempLock = -1;

            public bool IsOccupied { get { return Occupied >= 0; } }
            public bool IsPathClaimed { get { return PathClaimed >= 0; } }
            public bool IsTempLocked { get { return TempLock >= 0; } }
            public int TraversalCost { get { return Cost; } }
            public bool IsWalkable { get { return (Flags & CellWalkable) != 0; } }
            public bool IsEdge { get { return (Flags & CellEdge) != 0; } }

            public void Clear() {
                Cost = 0;
                Flags = 0;
                Occupied = PathClaimed = TempLock = -1;
            }
        }

        private GameOptions.CachedInt _defaultCost = new GameOptions.CachedInt("PathfindGridDefaultCost");
        private GameOptions.CachedInt _edgeCost = new GameOptions.CachedInt("PathfindGridEdgeCost");
        private GameOptions.CachedInt _occupiedCost = new GameOptions.CachedInt("PathfindGridOccupiedCost");
        private GameOptions.CachedInt _claimedCost = new GameOptions.CachedInt("PathfindGridPathCost");
        private GameOptions.CachedInt _playerCost = new GameOptions.CachedInt("PathfindGridPlayerCost");
        private GenericPool<SimpleCell> _cellPool = new GenericPool<SimpleCell>(1500, cell => cell.Clear());

        private int _playerID = -100;
        private List<Point3> _oldPlayerWalkable = new List<Point3>();
        private List<Point3> _oldPlayerOccupied = new List<Point3>();
        private System.Threading.ReaderWriterLockSlim _threadLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private Dictionary<Point3, SimpleCell> _cells = new Dictionary<Point3, SimpleCell>();
        private List<Point3> _cellKeys = new List<Point3>();

        public int CellsCount { get; private set; }

        public void ClearAll() {
            _threadLock.EnterWriteLock();
            foreach (var cell in _cells) {
                _cellPool.Store(cell.Value);
            }
            _cellKeys.Clear();
            _cells.Clear();
            _oldPlayerOccupied.Clear();
            _oldPlayerWalkable.Clear();
            CellsCount = 0;
            _threadLock.ExitWriteLock();
        }

        public void ClearLocks() {
            _threadLock.EnterWriteLock();
            foreach (var cell in _cells) {
                cell.Value.Occupied = cell.Value.PathClaimed = cell.Value.TempLock = -1;
                cell.Value.Cost = _defaultCost;
            }
            _oldPlayerOccupied.Clear();
            _threadLock.ExitWriteLock();
        }

        public bool CanAgentEnter(int id, Point3 pos, bool finalDestination) {
            _threadLock.EnterUpgradeableReadLock();
            if (!_cells.TryGetValue(pos, out var cell) || !cell.IsWalkable) {
                _threadLock.ExitUpgradeableReadLock();
                return false;
            }
            if (cell.IsOccupied && cell.Occupied == id) {
                _threadLock.ExitUpgradeableReadLock();
                return true;
            }
            if (finalDestination && cell.IsOccupied && cell.Occupied != id) {
                _threadLock.ExitUpgradeableReadLock();
                return false;
            }
            if (cell.IsTempLocked && cell.TempLock != id) {
                _threadLock.ExitUpgradeableReadLock();
                return false;
            }
            _threadLock.EnterWriteLock();
            cell.TempLock = id;
            SetCellCostInternal(cell);
            _threadLock.ExitWriteLock();
            _threadLock.ExitUpgradeableReadLock();
            return true;
        }

        public void Exit(int id, Point3 pos) {
            _threadLock.EnterWriteLock();
            if (_cells.TryGetValue(pos, out var cell)) {
                if (cell.TempLock == id) {
                    cell.TempLock = -1;
                }
                if (cell.Occupied == id) {
                    cell.Occupied = -1;
                }
                SetCellCostInternal(cell);
            }
            _threadLock.ExitWriteLock();
        }

        public bool IsWalkable(Point3 pos, bool isOversized) {
            _threadLock.EnterReadLock();
            bool val = false;
            if (_cells.TryGetValue(pos, out var cell)) {
                val = cell.IsWalkable;
                if (isOversized && cell.IsEdge) {
                    val = false;
                }
            }
            _threadLock.ExitReadLock();
            return val;
        }

        public bool HasCurrentAgentPath(Point3 pos) {
            _threadLock.EnterReadLock();
            bool val = false;
            if (_cells.TryGetValue(pos, out var cell)) {
                val = cell.IsPathClaimed;
            }
            _threadLock.ExitReadLock();
            return val;
        }

        public bool IsValidDestination(Point3 pos) {
            _threadLock.EnterReadLock();
            bool val = false;
            if (_cells.TryGetValue(pos, out var cell)) {
                val = !cell.IsOccupied && cell.IsWalkable;
            }
            _threadLock.ExitReadLock();
            return val;
        }

        public int GetTraversalCost(Point3 pos) {
            _threadLock.EnterReadLock();
            int val = 0;
            if (_cells.TryGetValue(pos, out var cell)) {
                val = cell.Cost;
            }
            _threadLock.ExitReadLock();
            return val;
        }

        public void SetAgentCurrentPath(Point3 pos, int id, bool status) {
            _threadLock.EnterWriteLock();
            if (_cells.TryGetValue(pos, out var cell)) {
                if (!status && cell.PathClaimed == id) {
                    cell.PathClaimed = -1;
                    SetCellCostInternal(cell);
                }
                else if (status && !cell.IsPathClaimed) {
                    cell.PathClaimed = id;
                    SetCellCostInternal(cell);
                }
            }
            _threadLock.ExitWriteLock();
        }

        public void SetStationaryAgent(Point3 pos, int id, bool status) {
            _threadLock.EnterWriteLock();
            if (_cells.TryGetValue(pos, out var cell)) {
                if (!status && cell.Occupied == id) {
                    cell.Occupied = -1;
                    SetCellCostInternal(cell);
                }
                else if (status && !cell.IsOccupied) {
                    cell.Occupied = id;
                    SetCellCostInternal(cell);
                }
            }
            _threadLock.ExitWriteLock();
        }

        public void SetWalkable(Point3 pos, bool status) {
            _threadLock.EnterWriteLock();
            SetWalkableInternal(pos, status);
            _threadLock.ExitWriteLock();
        }

        private void SetWalkableInternal(Point3 pos, bool status) {
            if (!_cells.TryGetValue(pos, out var cell)) {
                cell = _cellPool.New();
                cell.Cost = _defaultCost;
                _cells.Add(pos, cell);
                _cellKeys.Add(pos);
            }
            if (status) {
                cell.Flags |= CellWalkable;
            }
            else {
                cell.Flags &= ~CellWalkable;
            }
            CellsCount = _cells.Count;
        }

        private void SetCellCostInternal(SimpleCell cell) {
            if (cell.IsOccupied) {
                cell.Cost = cell.Occupied == _playerID ? _playerCost : _occupiedCost;
            }
            else if (cell.IsPathClaimed){
                cell.Cost = _claimedCost;
            }
            else {
                cell.Cost = cell.IsEdge? _edgeCost : _defaultCost;
            }
        }

        public Point3 GetOpenWalkablePosition() {
            WhileLoopLimiter.ResetInstance();
            Point3 pos = Point3.zero;
            _threadLock.EnterReadLock();
            while (WhileLoopLimiter.InstanceAdvance()) {
                pos = _cellKeys.RandomElement();
                var cell = _cells[pos];
                if (cell.IsWalkable && !cell.IsOccupied) {
                    break;
                }
            }
            _threadLock.ExitReadLock();
            return pos;
        }

        public void SetPlayerPosition(Point3 pos, int playerID, int impassableSize, int occupiedSize) {
            _threadLock.EnterWriteLock();
            _playerID = playerID;
            for (int i = 0; i < _oldPlayerWalkable.Count; i++) {
                SetWalkableInternal(_oldPlayerWalkable[i], true);
            }
            _oldPlayerWalkable.Clear();
            if (impassableSize > 0) {
                for (int x = -impassableSize; x <= impassableSize; x++) {
                    for (int z = -impassableSize; z <= impassableSize; z++) {
                        var chkPos = new Point3(pos.x + x, pos.y, pos.z + z);
                        if (!_cells.TryGetValue(chkPos, out var cell) || !cell.IsWalkable) {
                            continue;
                        }
                        cell.Flags &= ~CellWalkable;
                        _oldPlayerWalkable.Add(chkPos);
                    }
                }
            }
            for (int i = 0; i < _oldPlayerOccupied.Count; i++) {
                if (!_cells.TryGetValue(_oldPlayerOccupied[i], out var cell)) {
                    continue;
                }
                cell.Occupied = -1;
                SetCellCostInternal(cell);
            }
            _oldPlayerOccupied.Clear();
            if (occupiedSize > 0) {
                for (int x = -occupiedSize; x <= occupiedSize; x++) {
                    for (int z = -occupiedSize; z <= occupiedSize; z++) {
                        var chkPos = new Point3(pos.x + x, pos.y, pos.z + z);
                        if (!_cells.TryGetValue(chkPos, out var cell) || !cell.IsWalkable || _oldPlayerWalkable.Contains(chkPos)) {
                            continue;
                        }
                        cell.Occupied = _playerID;
                        SetCellCostInternal(cell);
                        _oldPlayerOccupied.Add(chkPos);
                    }
                }
            }
            _threadLock.ExitWriteLock();
        }

        public void SetWalkable(Bounds bounds, bool status) {
            var p3 = bounds.center.toPoint3ZeroY();
            var size = (int) bounds.extents.AbsMax();
            _threadLock.EnterWriteLock();
            for (int x = -size; x <= size; x++) {
                for (int z = -size; z <= size; z++) {
                    var checkPos = new Point3(p3.x + x, p3.y, p3.z + z);
                    SetWalkableInternal(checkPos, status);
                }
            }
            _threadLock.ExitWriteLock();
        }

        public void SetWalkable(BaseCell c) {
            _threadLock.EnterWriteLock();
            var p3 = new Point3(c.WorldPositionV3);
            var size = Game.MapCellSize / 2;
            for (int x = -size; x <= size; x++) {
                for (int z = -size; z <= size; z++) {
                    var pos = new Point3(p3.x + x, p3.y, p3.z + z);
                    if (!c.Walkable) {
                        SetWalkableInternal(pos, false);
                        continue;
                    }
                    var checkDir = new bool[4];
                    if (x == -size) {
                        checkDir[3] = true;
                    }
                    if (z == -size) {
                        checkDir[2] = true;
                    }
                    if (x == size) {
                        checkDir[1] = true;
                    }
                    if (z == size) {
                        checkDir[0] = true;
                    }
                    bool walkable = true;
                    for (int i = 0; i < checkDir.Length; i++) {
                        if (!checkDir[i]) {
                            continue;
                        }
                        if (!c.CanExit((Directions) i)) {
                            walkable = false;
                            break;
                        }
                    }
                    SetWalkableInternal(pos, walkable);
                }
            }
            _threadLock.ExitWriteLock();
        }

        public void FinishPathfindingSetup() {
            _threadLock.EnterWriteLock();
            var enumerator = _cells.GetEnumerator();
            try {
                while (enumerator.MoveNext()) {
                    var cell = enumerator.Current.Value;
                    bool isBorder = false;
                    for (int i = 0; i < DirectionsExtensions.DiagonalLength; i++) {
                        var pos = enumerator.Current.Key + ((DirectionsEight) i).ToP3();
                        if (!_cells.TryGetValue(pos, out var neighbor)) {
                            isBorder = true;
                            break;
                        }
                        if (!neighbor.IsWalkable) {
                            isBorder = true;
                            break;
                        }
                    }
                    if (isBorder) {
                        cell.Flags |= CellEdge;
                        SetCellCostInternal(cell);
                    }
                }
            }
            finally {
                enumerator.Dispose();
            }
            _threadLock.ExitWriteLock();
        }

        public void RunActionOnCells(System.Action<Point3, IPathfindingGridCell> del) {
            _threadLock.EnterReadLock();
            var enumerator = _cells.GetEnumerator();
            try {
                while (enumerator.MoveNext()) {
                    del(enumerator.Current.Key, enumerator.Current.Value);
                }
            }
            finally {
                enumerator.Dispose();
            }
            _threadLock.ExitReadLock();
        }
    }
}
