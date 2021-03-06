using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CullingSystem : SystemBase, IMainSystemUpdate {

        private GameOptions.CachedFloat _sectorSize = new GameOptions.CachedFloat("CullingSize");
        private GameOptions.CachedBool _useCulling = new GameOptions.CachedBool("UseCulling");

        private Dictionary<Point3, List<PrefabEntity>> _sectorObjects = new Dictionary<Point3, List<PrefabEntity>>();
        private Dictionary<Point3, List<GameObject>> _levelObjects = new Dictionary<Point3, List<GameObject>>();
        private List<PrefabEntity> _entities = new List<PrefabEntity>();
        private UnscaledTimer _updateTimer = new UnscaledTimer(1);
        private BufferedList<Point3>[] _list;
        private Point3 _playerPosition = Point3.max;
        private int _currentIndex = 0;
        private BufferedList<Point3> CurrentList { get { return _list[_currentIndex]; } }
        private BufferedList<Point3> PreviousList { get { return _list[_currentIndex == 0 ? 1 : 0]; } }

        public CullingSystem() {
            MessageKit.addObserver(Messages.LevelClear, ClearList);
            MessageKit.addObserver(Messages.PlayerReachedDestination, UpdatePlayerPosition);
            MessageKit.addObserver(Messages.PlayerTeleported, UpdatePlayerPosition);
            _list = new[] {
                new BufferedList<Point3>(),
                new BufferedList<Point3>(),
            };
        }

        private bool IsActive {
            get {
                if (!Game.GameStarted || !Game.GameActive || !_useCulling) {
                    return false;
                }
                return true;
            }
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            if (!IsActive || _updateTimer.IsActive) {
                return;
            }
            _updateTimer.StartTimer();
            for (int i = _entities.Count - 1; i >= 0; i--) {
                if (_entities[i] == null || _entities[i].Pooled) {
                    _entities.RemoveAt(i);
                    continue;
                }
                var entity = _entities[i];
                var pos = entity.transform.position.WorldToGenericGrid(_sectorSize);
                if (pos == entity.SectorPosition) {
                    continue;
                }
                GetList(entity.SectorPosition).Remove(entity);
                entity.SectorPosition = pos;
                SetEntityList(entity);
            }
        }

        private void UpdatePlayerPosition() {
            //TODO: this needs to be more global
            if (!IsActive) {
                return;
            }
            var playPos = Player.Tr.position.WorldToGenericGrid(_sectorSize);
            if (playPos == _playerPosition) {
                return;
            }
            _playerPosition = playPos;
            _currentIndex = _currentIndex == 0 ? 1 : 0;
            CurrentList.Clear();
            CurrentList.Add(_playerPosition);
            for (int i = 0; i < DirectionsExtensions.DiagonalLength; i++) {
                var pos = _playerPosition + ((DirectionsEight) i).ToPoint3();
                CurrentList.Add(pos);
            }
            SetList(true);
            SetList(false);
        }

        public List<PrefabEntity> GetList(Point3 p3) {
            if (!_sectorObjects.TryGetValue(p3, out var list)) {
                list = new List<PrefabEntity>();
                _sectorObjects.Add(p3, list);
            }
            return list;
        }

        private void SetEntityList(PrefabEntity entity) {
            var list = GetList(entity.SectorPosition);
            list.Add(entity);
            if (!IsActive) {
                return;
            }
            var active = CurrentList.Contains(entity.SectorPosition);
            entity.SetActive(active);
        }

        public void Add(PrefabEntity entity) {
            entity.SectorPosition = entity.transform.position.WorldToGenericGrid(_sectorSize);
            _entities.Add(entity);
            SetEntityList(entity);
        }

        public void Add(Point3 pos, GameObject go) {
            GetLevelList(pos).Add(go);
            go.SetActive(CurrentList.Contains(pos));
        }

        public void Remove(PrefabEntity entity) {
            _entities.Remove(entity);
            if (!_sectorObjects.TryGetValue(entity.SectorPosition, out var list)) {
                return;
            }
            list.Remove(entity);
            if (list.Count != 0) {
                return;
            }
            _sectorObjects.Remove(entity.SectorPosition);
        }

        private List<GameObject> GetLevelList(Point3 pos) {
            if (!_levelObjects.TryGetValue(pos, out var list)) {
                list = new List<GameObject>();
                _levelObjects.Add(pos, list);
            }
            return list;
        }

        private void ClearList() {
            _sectorObjects.Clear();
            _levelObjects.Clear();
            CurrentList.Clear();
            PreviousList.Clear();
        }

        private void SetList(bool active) {
            var list = active ? CurrentList: PreviousList;
            for (int i = 0; i < list.Count; i++) {
                var pos = list[i];
                if (active && !PreviousList.Contains(pos)) {
                    SetSector(pos, true);
                }
                else if (!active && !CurrentList.Contains(pos)) {
                    SetSector(pos, false);
                }
            }
        }
        
        private void SetSector(Point3 pos, bool active) {
            if (!_sectorObjects.TryGetValue(pos, out var list)) {
                return;
            }
            for (int i = list.Count - 1; i >= 0; i--) {
                if (list[i] == null) {
                    list.RemoveAt(i);
                    continue;
                }
                list[i].SetActive(active);
                //list[i].SetVisible(active);
            }
            if (!_levelObjects.TryGetValue(pos, out var lvlList)) {
                return;
            }
            for (int i = 0; i < lvlList.Count; i++) {
                lvlList[i].SetActive(active);
            }
        }
    }
}