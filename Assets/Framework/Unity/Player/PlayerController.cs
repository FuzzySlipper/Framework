using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class PlayerController : MonoBehaviour, ISystemUpdate {
        
        [SerializeField] private Transform _actorPivot = null;

        private IntValueHolder _currency = new IntValueHolder();
        private ValueHolder<bool> _moveEnabled = new ValueHolder<bool>(true);
        private bool _isMoving = false;

        public Entity Entity { get; protected set; }
        public ValueHolder<bool> MoveEnabledHolder { get { return _moveEnabled; } }
        public Transform Tr { get; private set; }
        public bool IsMoving { get { return _isMoving; } set { _isMoving = value; } }
        public Transform ActorPivot { get { return _actorPivot; } }
        public virtual bool Unscaled { get { return false; } }
        public virtual bool Slowed { get; protected set; }
        public virtual bool CanMove {
            get {
                return _moveEnabled.Value;
            }
        }

        protected virtual void Awake() {
            Player.Tr = transform;
            Tr = transform;
            Player.Currency = _currency;
            MessageKit.addObserver(Messages.LoadingFinished, EnablePlayer);
            Entity = Entity.New("PlayerController");
            Entity.Add(new LabelComponent("PlayerController"));
            Entity.Tr = Tr;
            Player.MainEntity = Entity;
        }

        public virtual void OnSystemUpdate(float dt) {
            if (!Game.GameActive) {
                return;
            }
        }

        public virtual void NewGame() {
            _currency.ChangeValue(100);
            MessageKit.post(Messages.PlayerNewGame);
        }

        public virtual void SetVitalMax() {}

        public void Teleport(Vector3 location, Quaternion rotation) {
            Tr.rotation = rotation;
            CameraMouseLook.main.ChangeRotation(rotation.eulerAngles.y);
            Teleport(location);
        }

        public virtual void Teleport(Vector3 location) {
            
        }

        public virtual void DisableMove(string id) {
            _moveEnabled.AddValue(false, id);
        }

        public virtual void RemoveDisabledMove(string id) {
            _moveEnabled.RemoveValue(id);
        }

        public virtual void AddExperience(float amount) {}
        public virtual void SetExperience(float amount) {}

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
