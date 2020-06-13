using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class PlayerController {

        private ValueHolder<bool> _moveEnabled = new ValueHolder<bool>(true);
        private bool _isMoving = false;
        private Point3 _gridPosition;

        protected PlayerControllerConfig Config;
        public Entity MainEntity { get; protected set; }
        public ValueHolder<bool> MoveEnabledHolder { get { return _moveEnabled; } }
        public Transform Tr { get; private set; }
        public bool IsMoving { get { return _isMoving; } set { _isMoving = value; } }
        public bool Active { get; protected set; }
        public virtual Point3 GridPosition { get { return _gridPosition; } }
        public virtual bool CanMove {
            get {
                return _moveEnabled.Value;
            }
        }

        public PlayerController(PlayerControllerConfig config) {
            Config = config;
            Tr = config.MainTr;
        }

        public virtual void Enable() {
            MessageKit.addObserver(Messages.LoadingFinished, EnablePlayer);
        }

        public virtual void Disable() {
            MessageKit.removeObserver(Messages.LoadingFinished, EnablePlayer);
        }

        public virtual void NewGame() {
            _gridPosition = Point3.max;
        }

        public virtual void SetActive(bool active) {
            Active = active;
            Tr.gameObject.SetActive(active);
        }

        public virtual void SystemUpdate(float dt) {
            if (!Active) {
                return;
            }
            if (_gridPosition != (Tr.position + Vector3.up).ToMapGridP3()) {
                UpdateCell();
            }
        }

        protected virtual void UpdateCell() {
            if (!Game.GameActive) {
                return;
            }
            var position = Tr.position;
            _gridPosition = position.ToMapGridP3();
            MainEntity.Add(new GridPosition(_gridPosition));
            PlayerGameStats.MetersWalked += Game.MapCellSize;
            World.Get<PathfindingSystem>().UpdatePlayerPosition(position);
            MessageKit.post(Messages.PlayerReachedDestination);
        }

        public virtual bool TryOpenDoor(Door door) {
            return false;
        }

        public virtual void Teleport(Vector3 location, Quaternion rotation) {
            Teleport(location);
            Tr.rotation = rotation;
            MessageKit<float>.post(Messages.PlayerViewRotated, rotation.eulerAngles.y);
        }

        public virtual void Teleport(Vector3 location) {
            Tr.position = FindFloorPoint(location);
            MessageKit.post(Messages.PlayerTeleported);
        }

        protected Vector3 FindFloorPoint(Vector3 location) {
            var ray = new Ray(location + (Vector3.up * Game.MapCellSize), Vector3.down);
            if (Physics.Raycast(ray, out var hit, Game.MapCellSize * 3f, LayerMasks.Floor)) {
                return hit.point;
            }
            return location;
        }

        public virtual void DisableMove(string id) {
            _moveEnabled.AddValue(false, id);
        }

        public virtual void RemoveDisabledMove(string id) {
            _moveEnabled.RemoveValue(id);
        }

        protected virtual void OnDeath() {
            
        }

        private void EnablePlayer() {
            _moveEnabled.Clear();
        }
    }
}
