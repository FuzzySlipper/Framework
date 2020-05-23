using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class PlayerController : MonoBehaviour, ISystemUpdate {
        
        [SerializeField] private Transform _actorPivot = null;

        private ValueHolder<bool> _moveEnabled = new ValueHolder<bool>(true);
        private bool _isMoving = false;

        public Entity Entity { get; protected set; }
        public ValueHolder<bool> MoveEnabledHolder { get { return _moveEnabled; } }
        public Transform Tr { get; private set; }
        public bool IsMoving { get { return _isMoving; } set { _isMoving = value; } }
        public Transform ActorPivot { get { return _actorPivot; } }
        public virtual Point3 GridPosition { get { return (Tr.position + Vector3.up).ToMapGridP3(); } }
        public virtual bool Unscaled { get { return false; } }
        public virtual bool Slowed { get; protected set; }
        public virtual bool CanMove {
            get {
                return _moveEnabled.Value;
            }
        }

        protected virtual void Awake() {
            SetupControllerDefaults();
            SetupGenericControllerEntity();
        }

        protected void SetupGenericControllerEntity() {
            SetupControllerEntity(Entity.New("PlayerController"));
            Entity.Add(new LabelComponent("PlayerController"));
        }

        protected void SetupControllerDefaults() {
            Player.Tr = transform;
            Tr = transform;
            MessageKit.addObserver(Messages.LoadingFinished, EnablePlayer);
        }

        protected virtual void SetupControllerEntity(Entity entity) {
            Entity = entity;
            entity.Add(new TransformComponent(Tr));
            Player.MainEntity = entity;
        }

        public virtual void OnSystemUpdate(float dt) {
            if (!Game.GameActive) {
                return;
            }
        }

        public virtual void NewGame() {
            Player.DefaultCurrencyHolder.ChangeValue(100);
            MessageKit.post(Messages.PlayerNewGame);
        }

        public virtual void SetVitalMax() {}

        public virtual void Teleport(Vector3 location, Quaternion rotation) {
            Tr.rotation = rotation;
            Tr.position = FindFloorPoint(location);
            MessageKit<float>.post(Messages.PlayerViewRotated, rotation.eulerAngles.y);
            MessageKit.post(Messages.PlayerTeleported);
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
            MessageKit.post(Messages.PlayerDead);
            Game.SetGameActive(false);
        }

        private void EnablePlayer() {
            _moveEnabled.Clear();
            //if (GameOptions.MouseLook) {
            //    TimeManager.PauseFor(0.5f, true, () => {
            //        Cursor.lockState = CursorLockMode.Locked;
            //    });
            //}
        }

    }
}
