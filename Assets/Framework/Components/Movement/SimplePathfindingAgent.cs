using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
        
    [System.Serializable]
	public sealed class SimplePathfindingAgent : IComponent, IDisposable, IReceive<ChangePositionEvent> {

        public Point3 CurrentPos;
        public PathfindingStatus CurrentStatus = PathfindingStatus.Created;
        public float MovementLerp;
        public bool IsOversized = false;

        public Vector3 PreviousTarget { get; private set; }
        public Vector3 CurrentTarget { get; private set; }
        public Point3 GridTarget { get; private set; }
        public Point3 End { get; private set; }
        public int CurrentIndex { get; private set; }
        public float RepathTime { get; private set; }
        public bool Redirected { get; private set; }
        public List<Point3> CurrentNodePath { get => _currentNodePath; }
        public bool IsLastIndex { get { return CurrentIndex == _currentNodePath.Count - 1; } }
        public bool CanRepath { get { return TimeManager.Time > RepathTime; } }

        private PathfindingRequest _currentRequest;
        private Point3 _redirectedPoint;
        private CachedComponent<MoveSpeed> _moveSpeed;
        private List<Point3> _currentNodePath = new List<Point3>();
        private IPathfindingGrid _grid;
        private float _repathRate;

        public SimplePathfindingAgent(IPathfindingGrid grid, float repathRate) {
            _grid = grid;
            _repathRate = repathRate;
        }

        public SimplePathfindingAgent(SerializationInfo info, StreamingContext context) {
            _repathRate = info.GetValue(nameof(_repathRate), _repathRate);
            _grid = World.Get<PathfindingSystem>().Grid;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_repathRate), _repathRate);
        }

        public void Dispose() {
            for (int i = 0; i < _currentNodePath.Count; i++) {
                _grid.SetAgentCurrentPath(_currentNodePath[i], this.GetEntity(), false);
            }
            _currentNodePath.Clear();
            if (_currentRequest != null) {
                _currentRequest.Dispose();
            }
            _grid.SetStationaryAgent(CurrentPos, this.GetEntity(), false);
            _currentRequest = null;
            _moveSpeed = null;
            _grid = null;
        }

        public void CancelPath() {
            Stop();
            CurrentStatus = PathfindingStatus.NoPath;
            MovementLerp = 0;
            for (int i = 0; i < _currentNodePath.Count; i++) {
                _grid.SetAgentCurrentPath(_currentNodePath[i], this.GetEntity(), false);
            }
            _currentNodePath.Clear();
        }

        public void Stop() {
            var entity = this.GetEntity();
            if (entity.Tags.Contain(EntityTags.Moving)) {
                entity.Tags.Remove(EntityTags.Moving);
            }
        }

        public void SetMoving() {
            CurrentStatus = PathfindingStatus.Moving;
            var entity = this.GetEntity();
            if (!entity.Tags.Contain(EntityTags.Moving)) {
                entity.Tags.Add(EntityTags.Moving);
            }
        }

        public void ReachedDestination() {
            CancelPath();
            _grid.SetStationaryAgent(CurrentPos, this.GetEntity(), true);
        }

        public bool TryEnterNextNode() {
            if (!_grid.CanAgentEnter(this.GetEntity(), GridTarget, IsLastIndex)) {
                CurrentStatus = PathfindingStatus.WaitingOnNode;
                Stop();
                return false;
            }
            SetMoving();
            return true;
        }

        public void SetPosition(Point3 pos) {
            CurrentPos = pos;
            var entity = this.GetEntity();
            if (entity.Tr != null) {
                entity.Tr.position = pos.toVector3() + new Vector3(0, -(Game.MapCellSize * 0.5f), 0);
            }
        }

        public void SearchPath() {
            RepathTime = Mathf.Infinity;
            CurrentStatus = PathfindingStatus.WaitingOnPath;
            _currentRequest = PathfindingRequest.Create(_grid, this.GetEntity(), CurrentPos, End, PathCompleted, IsOversized, _currentNodePath);
        }

        public void SetEnd(Point3 pos) {
            End = pos;
            SearchPath();
        }

        public void AdvancePath() {
            CurrentIndex++;
            PreviousTarget = CurrentTarget;
            CurrentTarget = _currentNodePath[CurrentIndex].toVector3() + new Vector3(0, -(Game.MapCellSize * 0.5f), 0);
            GridTarget = _currentNodePath[CurrentIndex];
            MovementLerp = 0f;
        }

        private void FinishPath() {
            if (_currentNodePath.Count <= 1) {
                return;
            }
            var moveOffset = new Vector3(0, -(Game.MapCellSize * 0.5f), 0);
            CurrentIndex = 1;
            PreviousTarget = _currentNodePath[0].toVector3() + moveOffset;
            CurrentTarget = _currentNodePath[1].toVector3() + moveOffset;
            MovementLerp = 0f;
            var entity = this.GetEntity();
            if (Vector3.Distance(PreviousTarget, entity.Tr.position) > 0.1f) {
                MovementLerp = Vector3.Distance(entity.Tr.position, CurrentTarget) * (_moveSpeed.Value?.Speed ?? 1);
                PreviousTarget = entity.Tr.position;
            }
            GridTarget = _currentNodePath[1];
            CurrentStatus = PathfindingStatus.PathReceived;
        }

        private void SetRepathTimer(bool isError = false) {
            RepathTime = TimeManager.Time + ((_repathRate + Game.Random.Next(20, 50) * 0.01f) * (isError ? 5 : 1));
        }

        public float GetCurrentDistance() {
            var a = _currentNodePath.SafeAccess(CurrentIndex -1);
            var b = _currentNodePath.SafeAccess(CurrentIndex);
            //TODO: make this account for grid sizes
            if (a.x != b.x && a[AstarP3Pathfinder.TravelAxis] != b[AstarP3Pathfinder.TravelAxis]) {
                // The tiles are diagonal. The distance between their centers is sqrt(2) which is 1.414...
                return 1.41421f;
            }
            return 1;
        }

        private void PathCompleted(PathfindingResult result, List<Point3> path) {
            if (_currentRequest != null) {
                _currentRequest.Dispose();
            }
            _currentRequest = null;
            var entity = this.GetEntity();
            if (entity == null) {
                return;
            }
            switch (result) {
                case PathfindingResult.Successful:
                case PathfindingResult.Redirected:
                    if (Redirected && result == PathfindingResult.Redirected && _redirectedPoint == path.LastElement() && CurrentPos == _redirectedPoint) {
                        SetRepathTimer(true);
                        CurrentStatus = PathfindingStatus.NoPath;
                        return;
                    }
                    SetRepathTimer(false);
                    _currentNodePath = path;
                    Redirected = result == PathfindingResult.Redirected;
                    if (Redirected) {
                        _redirectedPoint = _currentNodePath.LastElement();
                    }
                    FinishPath();
                    break;
                default:
                    SetRepathTimer(true);
                    CurrentStatus = PathfindingStatus.InvalidPath;
                    entity.Post(new StatusUpdate(result.ToString()));
                    break;
            }
        }

        public void Handle(ChangePositionEvent arg) {
            SetPosition(arg.Position.toPoint3());
        }
    }
}
