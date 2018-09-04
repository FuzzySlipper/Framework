using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CullingManager : SystemBase, IMainSystemUpdate {

        private static Dictionary<Point3, List<PrefabEntity>> _sectorObjects = new Dictionary<Point3, List<PrefabEntity>>();
        private static Dictionary<Point3, List<GameObject>> _sectorLevelObjects = new Dictionary<Point3, List<GameObject>>();

        private UnscaledTimer _updateTimer = new UnscaledTimer(1);
        private int _currentIndex = 0;
        
        private List<Point3>[] _list = new List<Point3>[2] {
            new List<Point3>(), new List<Point3>()
        };

        public List<Point3> CurrentList { get { return _list[_currentIndex]; } }
        public List<Point3> PreviousList { get { return _list[_currentIndex == 0 ? 1 : 0]; } }

        public CullingManager() {
            MessageKit.addObserver(Messages.LevelClear, ClearList);
            MessageKit.addObserver(Messages.GameStarted, GameStarted);
        }

        public void OnSystemUpdate(float dt) {
            if (!Game.GameActive || _updateTimer.IsActive) {
                return;
            }
            _updateTimer.StartTimer();
            for (int i = 0; i < CurrentList.Count; i++) {
                var sectorPos = CurrentList[i];
                var list = GetList(sectorPos);
                for (int listIdx = list.Count - 1; listIdx >= 0; listIdx--) {
                    var entity = list[listIdx];
                    if (entity == null) {
                        list.RemoveAt(listIdx);
                        continue;
                    }
                    var pos = Game.WorldToSector(entity.transform.position);
                    if (pos != entity.SectorPosition) {
                        entity.SectorPosition = pos;
                        SetEntityList(entity);
                        list.RemoveAt(listIdx);
                    }
                }
            }
        }

        public void UpdatePlayerPosition() {
            var playPos = Game.WorldToSector(Player.Tr.position);
            if (!CurrentList.Contains(playPos)) {
                CurrentList.Add(playPos);
                SetSector(playPos, true);
            }
        }

        private void GameStarted() {
            //Player.Controller.ActorSenses.OnSensesUpdate += UpdateVisibility;
        }

        public List<PrefabEntity> GetList(Point3 p3) {
            List<PrefabEntity> list;
            if (!_sectorObjects.TryGetValue(p3, out list)) {
                list = new List<PrefabEntity>();
                _sectorObjects.Add(p3, list);
            }
            return list;
        }

        private void SetEntityList(PrefabEntity entity) {
            var list = GetList(entity.SectorPosition);
            list.Add(entity);
            var active = CurrentList.Contains(entity.SectorPosition);
            entity.SetActive(active);
        }

        public void Add(PrefabEntity entity) {
            entity.SectorPosition = Game.WorldToSector(entity.transform.position);
            SetEntityList(entity);
        }

        public void Add(Point3 pos, GameObject go) {
            GetLevelList(pos).Add(go);
            //go.SetActive(main.CurrentList.Contains(pos));
        }

        public void Remove(PrefabEntity entity) {
            List<PrefabEntity> list;
            if (!_sectorObjects.TryGetValue(entity.SectorPosition, out list)) {
                return;
            }
            list.Remove(entity);
            if (list.Count != 0) {
                return;
            }
            _sectorObjects.Remove(entity.SectorPosition);
        }

        private List<GameObject> GetLevelList(Point3 pos) {
            List<GameObject> list;
            if (!_sectorLevelObjects.TryGetValue(pos, out list)) {
                list = new List<GameObject>();
                _sectorLevelObjects.Add(pos, list);
            }
            return list;
        }

        private void ClearList() {
            _sectorObjects.Clear();
            _sectorLevelObjects.Clear();
            CurrentList.Clear();
            PreviousList.Clear();
        }

        private void UpdateVisibility() {
            if (!GameOptions.UseCulling) {
                return;
            }
            _currentIndex = _currentIndex == 0 ? 1 : 0;
            CurrentList.Clear();
            //for (int i = 0; i < Player.Controller.ActorSenses.CurrentList.Count; i++) {
            //    var pos = Player.Controller.ActorSenses.CurrentList[i];
            //    var sector = Game.WorldToSector(pos.WorldPositionV3);
            //    if (!CurrentList.Contains(sector)) {
            //        CurrentList.Add(sector);
            //    }
            //}
            SetList(true);
            SetList(false);
        }

        private void SetList(bool active) {
            var list = active ? CurrentList : PreviousList;
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
            List<PrefabEntity> list;
            if (_sectorObjects.TryGetValue(pos, out list)) {
                for (int i = list.Count - 1; i >= 0; i--) {
                    if (list[i] == null) {
                        list.RemoveAt(i);
                        continue;
                    }
                    list[i].SetActive(active);
                    //list[i].SetVisible(active);
                }
            }
            //var lvlList = GetLevelList(pos);
            //for (int i = 0; i < lvlList.Count; i++) {
            //    lvlList[i].SetActive(active);
            //}
        }

       
    }
}