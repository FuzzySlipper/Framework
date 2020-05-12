using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelComrades.DungeonCrawler;

namespace PixelComrades {
    public class LevelCell {
        
        private Door[] _doors = new Door[DirectionsExtensions.DiagonalLength];
        private BlockBorder[] _borders = new BlockBorder[DirectionsExtensions.DiagonalLength];
        private LevelCell[] _neighbors = new LevelCell[DirectionsExtensions.DiagonalLength];


        public LevelCell(Point3 pos) {
            WorldPosition = pos;
            Explored = false;
        }

        public LevelCell(BlockCell block) {
            WorldPosition = block.Position;
            Explored = false;
            Walkable = block.Walkable;
            for (int i = 0; i < block.WallTypes.Length; i++) {
                switch (block.WallTypes[i]) {
                    case BlockCell.WallType.Blocked:
                        _borders[i] = BlockBorder.Impassable;
                        break;
                    case BlockCell.WallType.Doorway:
                    case BlockCell.WallType.LockedDoorway:
                        _borders[i] = BlockBorder.Doorway;
                        break;
                    case BlockCell.WallType.Wall:
                        _borders[i] = BlockBorder.InsideWall;
                        break;
                    case BlockCell.WallType.None:
                        _borders[i] = BlockBorder.None;
                        break;
                }
            }
        }

        public System.Action<bool> OnPlayerStatus;

        public LevelCell this[DirectionsEight index] {
            get { return _neighbors[(int)index]; }
            set { _neighbors[(int)index] = value; }
        }
        public Entity Occupied;
        public int Length { get { return _neighbors.Length; } }
        public LevelCell[] Neighbors { get { return _neighbors; } }
        public bool Walkable { get; protected set; }
        public Point3 WorldPosition { get; protected set; }
        public Vector3 WorldPositionV3 { get { return Game.GridToWorld(WorldPosition); } }
        public Vector3 WorldBottomV3 { get { return Game.GridToWorld(WorldPosition) - new Vector3(0, Game.MapCellSize * 0.5f, 0); } }
        public Vector3 WorldBottomCorner { get { return Game.GridToWorld(WorldPosition) - new Vector3(Game.MapCellSize * 0.5f, Game.MapCellSize * 0.5f, Game.MapCellSize * 0.5f); } }
        public Vector3 WorldTopCorner { get { return Game.GridToWorld(WorldPosition) + new Vector3(Game.MapCellSize * 0.5f, Game.MapCellSize * 0.5f, Game.MapCellSize * 0.5f); } }
        public BlockBorder[] Borders { get { return _borders; } }
        
        public bool Explored { get; set; }
        public bool IsVisible { get; set; }
        

        public bool IsOccupied() {
            return Occupied != null;
        }

        public bool CanExit(Directions dir) {
            return CanExit(dir.ToDirectionEight());
        }

        public bool CanPass(DirectionsEight dir) {
            if (!CanExit(dir)) {
                return false;
            }
            return true;
        }

        public void Clear() {
            _neighbors = new LevelCell[DirectionsExtensions.DiagonalLength3D];
            OnPlayerStatus = null;
            _doors = new Door[DirectionsExtensions.DiagonalLength3D];
        }

        public bool BlocksVision(Directions dir) {
            return BlocksVision(dir.ToDirectionEight());
        }

        public virtual bool BlocksVision(DirectionsEight dir) {
            if (!CanExit(dir)) {
                return true;
            }
            var door = GetDoor(dir);
            return door != null && door.BlocksVision;
        }

        public virtual bool CanExit(DirectionsEight dir) {
            if (_neighbors[(int) dir] == null || !_neighbors[(int) dir].Walkable) {
                return false;
            }
            var door = GetDoor(dir);
            return door == null || door.Opened;
        }

      
        public void PlayerEntered() {
            if (OnPlayerStatus != null) {
                OnPlayerStatus(true);
            }
        }

        public void PlayerLeft() {
            if (OnPlayerStatus != null) {
                OnPlayerStatus(false);
            }
        }

        public DirectionsEight GetTravelDir(LevelCell cell) {
            for (int i = 0; i < _neighbors.Length; i++) {
                if (_neighbors[i] == cell) {
                    var dir = (DirectionsEight) i;
                    return dir;
                }
            }
            return DirectionsEight.Top;
        }

        public bool CanReach(LevelCell cell) {
            for (int i = 0; i < _neighbors.Length; i++) {
                if (_neighbors[i] == cell) {
                    var dir = (DirectionsEight) i;
                    return CanExit(dir);
                }
            }
            return false;
        }
        public BlockBorder GetBorder(DirectionsEight dir) {
            return _borders[(int) dir];
        }

        public int DoorCount {
            get {
                int cnt = 0;
                for (int i = 0; i < _doors.Length; i++) {
                    if (_doors[i] != null) {
                        cnt++;
                    }
                }
                return cnt;
            }
        }

        

        //public void AddElement(LevelElement element) {
        //    //I'm removing this for now to set manual
        //    //if (element.BlocksPath()) {
        //    //    Walkable = false;
        //    //}
        //}

        public Door GetDoor(Directions dir) {
            return GetDoor(dir.ToDirectionEight());
        }

        public Door GetDoor(DirectionsEight dir) {
            var door = _doors[(int) dir];
            if (door != null) {
                return door;
            }
            if (!dir.IsCardinal()) {
                return null;
            }
            var adjacent = dir.Adjacent();
            for (int i = 0; i < adjacent.Length; i++) {
                door = _doors[(int) adjacent[i]];
                if (door != null) {
                    return door;
                }
            }
            return null;
        }

        public void SetDoor(Door door, DirectionsEight dir) {
            _doors[(int) dir] = door;
        }

        public bool CanReach(LevelCell cell, bool ignoreDoor) {
            for (int i = 0; i < Neighbors.Length; i++) {
                if (Neighbors[i] == cell) {
                    var dir = (DirectionsEight) i;
                    return CanExit(dir, ignoreDoor);
                }
            }
            return false;
        }

        public bool CanExit(DirectionsEight dir, bool ignoreDoor) {
            if (this[dir] == null || !this[dir].Walkable) {
                return false;
            }
            if (!_borders[(int) dir].CanExit()) {
                return false;
            }
            if (ignoreDoor) {
                return true;
            }
            var door = GetDoor(dir);
            return door == null || door.Opened;
        }


        public bool IsOpen(DirectionsEight dir) {
            var door = GetDoor(dir);
            if (door != null && !door.Opened) {
                return false;
            }
            return _borders[(int) dir] == BlockBorder.None || _borders[(int) dir] == BlockBorder.Impassable;
        }
    }

    public enum BlockBorder {
        None,
        OutsideWall,
        InsideWall,
        Impassable,
        Doorway,
    }
}
