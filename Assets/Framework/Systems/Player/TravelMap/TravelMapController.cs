using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class TravelMapController : PlayerController {

        private const int MinPlayerPriority = 50;

        private float _turnEnergyTick;
        private static float _turnEnergy;
        private bool _displayingText = false;
        private List<Vector3> _pathPositions = new List<Vector3>();
        private SimpleBufferedList<TurnMove> _playerMoves;
        private TurnSorter _turnSorter = new TurnSorter();
        private int _pathIndex;
        private List<MapEnemy> _enemies = new List<MapEnemy>();
        private Task _updateTask;
        private Task _enemyMove;
        private int _turn = 0;
        private TravelMapConfig _config;
        private RtsCameraConfig _camConfig;
        private RtsCameraComponent _rtsCamera;
        private PlayerCameraComponent _cam;
        private PlayerInputComponent _input;
        
        public bool DisplayingText { get => _displayingText; set => _displayingText = value; }
        public HexMoverUnit PlayerUnit { get => _config.PlayerUnit; }
        public Camera Cam { get => _config.Cam; }

        public TravelMapController(PlayerControllerConfig config) : base(config) {
            _config = (TravelMapConfig) config;
            _turnEnergyTick = 1f / _config.MaxMovesTurn;
            _playerMoves = new SimpleBufferedList<TurnMove>(15);
            _camConfig = _config.RtsCameraConfig;
            for (int i = 0; i < _config.TileGridRenderers.Length; i++) {
                _config.TileGridRenderers[i].enabled = false;
            }
        }

        public override void Enable() {
            base.Enable();
            if (_input != null) {
                PlayerInputSystem.Set(_input);
            }
            if (_rtsCamera != null) {
                _rtsCamera.Active = true;
            }
            CameraSystem.Set(_cam);
            _cam.Cam.enabled = true;
        }

        public override void Disable() {
            base.Disable();
            if (_input != null) {
                PlayerInputSystem.Remove(_input);
            }
            if (_rtsCamera != null) {
                _rtsCamera.Active = false;
            }
            CameraSystem.Remove(_cam);
            _cam.Cam.enabled = false;
        }

        public override void NewGame() {
            base.NewGame();
            MainEntity = Entity.New("TravelController");
            MainEntity.Add(new TransformComponent(Tr));
            MainEntity.Add(new LabelComponent("TravelController"));
            // entity.Add(new ImpactRendererComponent(UIPlayerSlot.GetSlot(0)));
            MainEntity.Add(new PlayerRaycastTargeting());
            _input = MainEntity.Add(new PlayerInputComponent(new TravelMapInput(LazySceneReferences.main.PlayerInput, this)));
            _cam = MainEntity.Add(new PlayerCameraComponent(_config.PlayerUnit.Tr, _config.Cam));
            _rtsCamera = MainEntity.Add(new RtsCameraComponent(_config.Cam, _camConfig));
            _rtsCamera.FollowTr = _config.PlayerUnit.Tr;
            _rtsCamera.Active = false;
            UnityToEntityBridge.RegisterToEntity(Tr.gameObject, MainEntity);
            RtsCameraSystem.Set(_rtsCamera);
        }

        public override void SetActive(bool active) {
            base.SetActive(active); 
            UIMap.main.SetMinimapStatus(!active);
            //UIDropWorldPanel.Enabled = !status;
            _config.Cam.gameObject.SetActive(active);
            _config.Cam.enabled = active;
            for (int i = 0; i < _config.TileGridRenderers.Length; i++) {
                _config.TileGridRenderers[i].enabled = active;
            }
            if (active) {
                Game.PauseAndUnlockCursor("RiftSpace");
                UIPlayerComponents.DisableGameplayUi("RiftSpace");
            }
            else {
                Game.RemovePauseAndLockCursor("RiftSpace");
                UIPlayerComponents.RemoveDisableGameplayUi("RiftSpace");
                if (_displayingText) {
                    UICenterTarget.Clear();
                    _displayingText = false;
                }
            }
            for (int i = 0; i < _config.ActiveObjects.Length; i++) {
                _config.ActiveObjects[i].SetActive(active);
            }
        }

        public override void SystemUpdate(float dt) {
            base.SystemUpdate(dt);
            for (int i = 0; i < _enemies.Count; i++) {
                var enemy = _enemies[i];
                if (enemy.InTransition) {
                    enemy.UpdateVisibility();
                }
            }
        }

        public void AddMove(float turnEnergyNeeded, IEnumerator mover, float priority, string nm) {
            _playerMoves.Add(new TurnMove(_turnEnergy + turnEnergyNeeded, mover, priority, nm));
        }

        public void SetupMap() {
            for (int i = 0; i < _enemies.Count; i++) {
                ItemPool.Despawn(_enemies[i].Unit.gameObject);
            }
            _enemies.Clear();
            for (int i = 0; i < (int) RiftFactions.Maelstrom; i++) {
                var faction = (RiftFactions) i;
                for (int e = 0; e < _config.EnemiesPerFaction; e++) {
                    ItemPool.Spawn(
                        _config.EnemyMoverPrefab,
                        entity => {
                            var enemy = new MapEnemy(entity.GetComponent<HexMoverUnit>(), faction);
                            entity.name = faction.ToString() + _enemies.Count;
                            WhileLoopLimiter.ResetInstance();
                            if (!HexMapGenerator.RegionDict.TryGetValue((int) faction, out var cellList)) {
                                Debug.LogErrorFormat("No cells for " + faction);
                                ItemPool.Despawn(enemy.Unit.gameObject);
                                return;
                            }
                            while (WhileLoopLimiter.InstanceAdvance()) {
                                var cell = cellList.RandomElement();
                                if (!cell.IsUnderwater && !cell.HasFeature && cell.Unit == null) {
                                    enemy.Unit.Teleport(cell);
                                    break;
                                }
                            }
                            FindEnemyPath(enemy);
                            _enemies.Add(enemy);
                        });
                }
            }
        }

        public void Teleport(HexCell cell) {
            _config.PlayerUnit.Teleport(cell);
            _config.PlayerUnit.ClearPath();
        }
        
        public void Travel(List<HexCell> path) {
            if (path == null || path.Count == 0) {
                return;
            }
            _config.PlayerUnit.SetPath(path);
            if (_config.PathRenderer != null) {
                _pathPositions.Clear();
                for (int i = 0; i < path.Count; i++) {
                    _pathPositions.Add(path[i].Position + _config.LineOffset);
                }
                _config.PathRenderer.positionCount = _pathPositions.Count;
                _config.PathRenderer.SetPositions(_pathPositions.ToArray());
            }
            RecalculateTurn();
            if (_updateTask != null) {
                _updateTask.Cancel();
            }
            _updateTask = TimeManager.StartUnscaled(ProcessTurn());
        }

        private void RecalculateTurn() {
            _playerMoves.Clear();
            if (_config.PlayerUnit.CurrentPath == null || _config.PlayerUnit.CurrentPath.Count <= 1) {
                return;
            }
            float playerIncrement = 1f / _config.PlayerUnit.MovesPerTurn;
            for (int i = 1; i < _config.PlayerUnit.CurrentPath.Count + 1; i++) {
                AddMove(i * playerIncrement, _config.PlayerUnit.TravelHex(), MinPlayerPriority - i, "Player" + i);
            }
            for (int i = 0; i < _enemies.Count; i++) {
                var enemy = _enemies[i];
                FindEnemyPath(enemy);
            }
            _playerMoves.Update();
        }

        private void FindEnemyPath(MapEnemy enemy) {
            enemy.Unit.ClearPath();
            if (enemy.Unit.Location != null && enemy.Unit.Location.IsVisible) {
                if (!enemy.CanSeePlayer) {
                    FloatingText.Message(enemy.Faction + " Saw Player", enemy.Unit.Tr.position);
                }
                enemy.LastSawPlayerTurn = _turn;
                enemy.CanSeePlayer = true;
            }
            else {
                enemy.CanSeePlayer = false;
            }
            HexCell target = null;
            if (enemy.LastSawPlayerTurn < _turn + _config.MaxChasePlayer) {
                target = _config.PlayerUnit.Location;
            }
            else {
                var surrounding = HexGridPathfinding.Current.GetNodesInRange(enemy.Unit.Location, enemy.Unit.MovesPerTurn, enemy.Unit);
                if (surrounding != null && surrounding.Count > 1) {
                    target = surrounding.RandomElement().Value;
                }
                HexGridPathfinding.Current.Clear(surrounding);
            }
            if (target == null) {
                return;
            }
            enemy.Unit.SetPath(HexGridPathfinding.Current.GetPathEndNotWalkable(enemy.Unit.Location, target, enemy.Unit));
            if (enemy.Unit.CurrentPath != null) {
                enemy.UpdateNextEnergy();
            }
        }
        
        private IEnumerator ProcessTurn() {
            while (_playerMoves.Count > 0) {
                _turnEnergy += _turnEnergyTick;
                _turn = Mathf.FloorToInt(_turnEnergy);
                _playerMoves.Update();
                _playerMoves.Sort(_turnSorter);
                for (int i = 0; i < _playerMoves.Count; i++) {
                    if (_playerMoves[i].TurnEnergyNeeded <= _turnEnergy) {
                        yield return _playerMoves[i].Mover;
                        CheckNpcVisibility();
                        _playerMoves.Remove(i);
                    }
                }
                _playerMoves.Update();
                if (_enemyMove != null) {
                    continue;
                }
                bool enemyMove = false;
                for (int i = 0; i < _enemies.Count; i++) {
                    var enemy = _enemies[i];
                    if (enemy.NextEnergy <= _turnEnergy) {
                        enemyMove = true;
                        enemy.StartMove();
                    }
                }
                if (enemyMove) {
                    _enemyMove = TimeManager.StartUnscaled(ProcessEnemyTurn());
                }
                yield return null;
            }
            if (_config.PlayerUnit.Location.HasFeature) {
                _config.PlayerUnit.Location.Feature.OnEnter();
            }
        }

        private void CheckNpcVisibility() {
            for (int i = 0; i < _enemies.Count; i++) {
                var enemy = _enemies[i];
                if (enemy.Unit.Location == null) {
                    continue;
                }
                if (enemy.IsVisible && enemy.Unit.Location.IsVisible) {
                    continue;
                }
                if (!enemy.IsVisible && !enemy.Unit.Location.IsVisible) {
                    continue;
                }
                enemy.ChangeVisibility(_config.EnemyVisibilitySpeed);
            }
        }

        private IEnumerator ProcessEnemyTurn() {
            yield return null;
            while (true) {
                var isActive = false;
                for (int i = 0; i < _enemies.Count; i++) {
                    var enemy = _enemies[i];
                    if (!enemy.IsMoving) {
                        continue;
                    }
                    enemy.Unit.UpdateMove();
                    if (!enemy.Unit.MoveActive) {
                        enemy.FinishMove();
                        if (!enemy.Unit.HasPath) {
                            FindEnemyPath(enemy);
                        }
                        else {
                            enemy.UpdateNextEnergy();
                        }
                    }
                    else {
                        isActive = true;
                    }
                }
                CheckNpcVisibility();
                yield return null;
                if (!isActive) {
                    break;
                }
            }
            _enemyMove = null;
        }
        
        [System.Serializable]
        public struct TurnMove {
            public float TurnEnergyNeeded;
            public IEnumerator Mover;
            public float Priority;
            public string DebugName;

            public TurnMove(float turnEnergyNeeded, IEnumerator mover, float priority, string debugName) {
                TurnEnergyNeeded = turnEnergyNeeded;
                Mover = mover;
                Priority = priority;
                DebugName = debugName;
            }
        }

        [System.Serializable]
        public class MapEnemy {
            public HexMoverUnit Unit;
            public RiftFactions Faction;
            public bool CanSeePlayer = false;
            public int LastSawPlayerTurn = int.MaxValue;
            public float NextEnergy = float.MaxValue;
            public bool IsMoving { get; private set; }
            public VisibleStatus Visibility = VisibleStatus.NotVisible;
            public TweenFloat VisibleLerp = new TweenFloat(0,1, 0.5f, EasingTypes.SinusoidalInOut, true);
            
            public enum VisibleStatus {
                NotVisible,
                BecomingVisible,
                Visible,
                Disappearing
            }
            
            public bool IsVisible { get { return Visibility == VisibleStatus.Visible || Visibility == VisibleStatus.BecomingVisible; } }
            public bool InTransition { get { return Visibility == VisibleStatus.BecomingVisible || Visibility == VisibleStatus.Disappearing; } }
            
            public MapEnemy(HexMoverUnit unit, RiftFactions faction) {
                Unit = unit;
                Faction = faction;
            }

            public void UpdateNextEnergy() {
                NextEnergy = _turnEnergy + (1f / Unit.MovesPerTurn);
            }

            public void ChangeVisibility(float speed) {
                VisibleLerp.Restart(0,1, speed);
                Visibility = Unit.Location.IsVisible ? VisibleStatus.BecomingVisible : VisibleStatus.Disappearing;
            }

            private static Color _blackTransparent = new Color(0, 0, 0, 0);

            public void UpdateVisibility() {
                Unit.SpriteRenderer.color = Color.Lerp(_blackTransparent, Color.white, VisibleLerp.Get());
                if (!VisibleLerp.Active) {
                    Visibility = Visibility == VisibleStatus.BecomingVisible ? VisibleStatus.Visible : VisibleStatus.NotVisible;

                }
            }

            public void StartMove() {
                IsMoving = true;
                Unit.SetupNextMove();
            }

            public void FinishMove() {
                Unit.FinishMove();
                IsMoving = false;
            }
        }

        private class TurnSorter : Comparer<TurnMove> {
            public override int Compare(TurnMove x, TurnMove y) {
                return -1 * x.Priority.CompareTo(y.Priority);
            }
        }
    }
}
