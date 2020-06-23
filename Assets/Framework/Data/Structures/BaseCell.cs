using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class BaseCell {

        private BaseCell[] _neighbors = new BaseCell[DirectionsExtensions.DiagonalLength3D];
        

        public System.Action<bool> OnPlayerStatus;

        public BaseCell this[DirectionsEight index] {
            get { return _neighbors[(int)index]; }
            set { _neighbors[(int)index] = value; }
        }
        public Entity Occupied;
        public int Length { get { return _neighbors.Length; } }
        public BaseCell[] Neighbors { get { return _neighbors; } }
        public bool Walkable { get; protected set; }
        public Point3 WorldPosition { get; protected set; }
        public abstract Vector3 WorldPositionV3 { get; }
        public abstract Vector3 WorldBottomV3 { get; }
        
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


        public virtual void Clear() {
            _neighbors = new BaseCell[DirectionsExtensions.DiagonalLength3D];
        }

        public bool BlocksVision(Directions dir) {
            return BlocksVision(dir.ToDirectionEight());
        }

        public virtual bool BlocksVision(DirectionsEight dir) {
            if (!CanExit(dir)) {
                return true;
            }
            return false;
        }

        public virtual bool CanExit(DirectionsEight dir) {
            if (_neighbors[(int) dir] == null || !_neighbors[(int) dir].Walkable) {
                return false;
            }
            return true;
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

        public DirectionsEight GetTravelDir(BaseCell cell) {
            for (int i = 0; i < _neighbors.Length; i++) {
                if (_neighbors[i] == cell) {
                    var dir = (DirectionsEight) i;
                    return dir;
                }
            }
            return DirectionsEight.Top;
        }

        public bool CanReach(BaseCell cell) {
            for (int i = 0; i < _neighbors.Length; i++) {
                if (_neighbors[i] == cell) {
                    var dir = (DirectionsEight) i;
                    return CanExit(dir);
                }
            }
            return false;
        }
    }
}
