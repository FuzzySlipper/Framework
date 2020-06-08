using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PlayerGridController : PlayerController {

        private LevelCell _lastCell;
        private LevelCell _currentCell;
        private TweenV3 _tweenMove = new TweenV3();
        private TweenQuaternion _tweenRotate = new TweenQuaternion();
        private TweenState _moveState;
        private TweenState _rotateState;
        private PlayerGridControllerConfig _config;

        public override Point3 GridPosition { get { return _currentCell != null ? _currentCell.WorldPosition : (Tr.position + Vector3.up).ToMapGridP3(); } }
        public LevelCell Cell { get { return _currentCell != null ? _currentCell : World.Get<MapSystem>().GetCell(Tr.position
        .ToMapGridP3()) as LevelCell;
         } }
        public Point3 ForwardGridPosition { get { return GridPosition + Tr.forward.toPoint3(); } }
        //public override bool CanMove {
        //    get {
        //        for (int i = 0; i < Party.Length; i++) {
        //            if (Party[i] != null && !Party[i].IsDead && !Party[i].Stats.GetVital(Stats.Recovery).IsMax) {
        //                return false;
        //            }
        //        }
        //        return base.CanMove;
        //    }
        //}

        public PlayerGridController(PlayerControllerConfig config) : base(config) {
            _config = (PlayerGridControllerConfig) config;
            _moveState = new TweenState(
                _tweenMove,
                () => { Tr.position = _tweenMove.Get(); },
                FinishMove);
            _rotateState = new TweenState(
                _tweenRotate,
                () => {
                    Tr.localRotation = _tweenRotate.Get();
                    if (!GameOptions.MouseLook) {
                        CameraMouseLook.main.Pivot.localRotation = _tweenRotate.Get();
                    }
                },
                FinishRotate);
        }

        public override void Enable() {
            base.Enable();
            MessageKit.addObserver(Messages.LevelLoadingFinished, LoadFinished);
        }

        public override void Disable() {
            base.Disable();
            MessageKit.removeObserver(Messages.LevelLoadingFinished, LoadFinished);
        }

        public override void NewGame() {
            base.NewGame();
            _lastCell = _currentCell = null;
        }

        public override void SystemUpdate(float dt) {
            base.SystemUpdate(dt);
            CheckRotation();
            if (_moveState.Active) {
                _moveState.Update();
            }
            if (_rotateState.Active) {
                _rotateState.Update();
            }
            
        }

        private int _diffDir = 0;
        [SerializeField] private int _minFramesForDirChange = 15;

        private void CheckRotation() {
            if (PlayerCamera.AltCam != null) {
                return;
            }
            var newDir = Player.Cam.transform.parent.localRotation.eulerAngles.EulerToDirectionEight(true);
            var currDir = Tr.localRotation.eulerAngles.EulerToDirectionEight(true);
            if (currDir != newDir) {
                _diffDir++;
                if (_diffDir > _minFramesForDirChange) {
                    SnapRotate(newDir.ToEulerRot());
                    //PlayerControllerSystem.Current.RotateActorTo(newDir.ToEulerRot(), true, true);
                    _diffDir = 0;
                }
            }
            else {
                _diffDir = 0;
            }
        }


        private void LoadFinished() {
            _lastCell = null;
            _currentCell = null;
        }

        private void FinishMove() {
            RemoveDisabledMove(MoveId);
            UpdateCell();
            for (int i = 0; i < PlayerPartySystem.Party.Length; i++) {
                if (PlayerPartySystem.Party[i] != null && !PlayerPartySystem.Party[i].IsDead && !PlayerPartySystem.Party[i].Stats.GetVital(Stats.Recovery).IsMax) {
                    var vital = PlayerPartySystem.Party[i].Entity.FindStat<VitalStat>(Stats.Recovery);
                    if (vital != null && vital.Current > 0) {
                        vital.Current = Mathf.Max(0, vital.Current * 0.5f);
                    }
                }
            }
            MessageKit.post(Messages.PlayerReachedDestination);
            //HealPerStep();
        }

        private void HealPerStep() {
            for (int i = 0; i < PlayerPartySystem.Party.Length; i++) {
                var player = PlayerPartySystem.Party[i];
                if (player.IsDead) {
                    continue;
                }
                var percent = Player.Supplies.Value <= 0 ? 0.2f : 0.3f;
                player.Stats.DoRecovery(percent);
            }
        }

        private void FinishRotate() {
            RemoveDisabledMove(RotateId);
            //Sensor.UpdateSenses();
            MessageKit.post(Messages.PlayerRotated);
        }
        
        public override void Teleport(Vector3 location, Quaternion rotation) {
            base.Teleport(location, rotation);
            UpdateCell();
        }

        public override void Teleport(Vector3 location) {
            base.Teleport(location);
            UpdateCell();
        }

        public void RotateActorTo(Directions dir) {
            RotateActorTo(Tr.localRotation * Quaternion.Euler(dir.ToEuler()));
        }

        public override bool TryOpenDoor(Door door) {
            if (door.Locked) {
                for (int i = 0; i < PlayerPartySystem.Party.Length; i++) {
                    if (RpgSettings.CanOpenLock(PlayerPartySystem.Party[i].Entity, door.Keyhole.PickDifficulty)) {
                        return true;
                    }
                }
                return false;
            }
            door.ToggleDoor(Tr.position);
            return true;
        }
        
        public void RotateActorTo(Quaternion rotation, bool quickRotate = false, bool overrideMove = false) {
            if (_rotateState.Active) {
                return;
            }
            if (!overrideMove && (!CanMove || _moveState.Active)) {
                return;
            }
            _tweenRotate.Restart(Tr.localRotation, rotation,
                _config.MoveConfig.RotationLength * (quickRotate ? 0.25f : 1),
                _config.MoveConfig.RotationEasing);
            _rotateState.Restart();
            DisableMove(RotateId);
        }

        public void SnapRotate(Quaternion rotation) {
            if (_rotateState.Active) {
                return;
            }
            Tr.localRotation = rotation;
            //Sensor.UpdateSenses();
            MessageKit.post(Messages.PlayerRotated);
        }

        public void TryMove(Point3 dir) {
            if (!CanMove || dir == Point3.zero || _moveState.Active) {
                return;
            }
            var gridPos = GridPosition;
            var movePos = gridPos + Game.LocalGridRotated(dir, Tr);
            if (movePos == gridPos) {
                return;
            }
            var cell = World.Get<MapSystem>().GetCell(movePos) as LevelCell;
            TryMove(cell);
        }

        public void MoveToLastCell() {
            TryMove(_lastCell);
        }

        public void TryMove(LevelCell cell) {
            if (cell == null || Cell == null || cell.IsOccupied()) {
                return;
            }
            var movePos = cell.WorldPosition;
            if (!Cell.CanReach(cell, false)) {
                return;
            }
            SetMove(movePos);
        }

        private void SetMove(Point3 targetP3) {
            _tweenMove.Restart(Tr.position, FindMovePos(targetP3), _config.MoveConfig.Length, _config.MoveConfig.Easing, false);
            DisableMove(MoveId);
            _moveState.Restart();
            MessageKit.post(Messages.PlayerMoving);
        }

        private Vector3 FindMovePos(Point3 targetP3) {
            var target = targetP3.MapGridToWorldV3() + new Vector3(0, Game.MapCellSize * -0.5f, 0);
            RaycastHit hit;
            var ray = new Ray(target, -Tr.up);
            if (Physics.Raycast(ray, out hit, 6, LayerMasks.Floor)) {
                Debug.DrawLine(ray.origin, hit.point, Color.blue, 3f);
                target = hit.point;
            }
            if (_config.EnvOffset > 0) {
                var edge = targetP3.MapGridToWorldV3() + (GridPosition.MapGridToWorldV3() - targetP3.MapGridToWorldV3()).normalized * Game.MapCellSize * 0.5f;
                if (Physics.Linecast(edge, target, out hit, LayerMasks.WallsEnvironment)) {
                    Debug.DrawLine(Tr.position, hit.point, Color.yellow, 3f);
                    target = hit.point + (Tr.position - hit.point).normalized * _config.EnvOffset;
                }
            }
            return target;
        }

        protected override void UpdateCell() {
            base.UpdateCell();
            _lastCell = _currentCell;
            _currentCell = World.Get<MapSystem>().GetCell(GridPosition) as LevelCell;
            if (_lastCell != null) {
                _lastCell.PlayerLeft();
            }
            if (_currentCell != null) {
                _currentCell.PlayerEntered();
            }
            PlayerGameStats.MetersWalked += Game.MapCellSize;
            World.Get<PathfindingSystem>().UpdatePlayerPosition(Tr.position);
        }
        protected override void OnDeath() {
            base.OnDeath();
            if (CombatArenaSystem.Active) {
                World.Get<CombatArenaSystem>().Defeated();
            }
        }

        private const string MoveId = "Moving";
        private const string RotateId = "Rotating";
    }

    public class TweenState {
        public TweenState(Tweener tween, System.Action update, System.Action finish) {
            _tween = tween;
            _onUpdate = update;
            _onFinish = finish;
        }

        private Tweener _tween;
        private bool _active = false;
        private System.Action _onUpdate;
        private System.Action _onFinish;
        
        public bool Active { get { return _active; } }

        public void Restart() {
            _active = true;
        }

        public void Update() {
            if (!_active) {
                return;
            }
            _onUpdate();
            if (!_tween.Active) {
                _active = false;
                _onFinish();
            }
        }

    }
}
