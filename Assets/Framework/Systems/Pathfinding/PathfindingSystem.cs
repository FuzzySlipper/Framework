using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PathfindingSystem : SystemBase, IMainSystemUpdate {

        public static bool UseSimple = false;

        private GameOptions.CachedInt _playerWalkableRadius = new GameOptions.CachedInt("PathfindPlayerWalkableRadius");
        private GameOptions.CachedInt _playerOccupyRadius = new GameOptions.CachedInt("PathfindPlayerOccupiedRadius");
        private GameOptions.CachedInt _threadCount = new GameOptions.CachedInt("PathfindThreadCount");
        private GameOptions.CachedBool _useThreading = new GameOptions.CachedBool("PathfindUseThreading");

        public object QueueLock = new object(); // The lock for adding to and removing from the processing queue.
        public object ReturnLock = new object(); // The lock for adding to and removing from the return queue.

        private PathfindingThread[] _threads;
        private List<PathfindingRequest> _pending = new List<PathfindingRequest>();
        private List<PathReturn> _toReturn = new List<PathReturn>();
        private IPathfinder _nonThreadedPathfinder;
        private IPathfindingGrid _pathfindingGrid;
        private IPathfindingSource _pathfindingSource;

        public PathfindingThread[] Threads { get => _threads; }
        public int PendingCount { get; private set; }
        public int ReturnCount { get; private set; }
        public IPathfindingGrid Grid { get => _pathfindingGrid; }

        public PathfindingSystem() {
            if (!UseSimple) {
                _pathfindingSource = new AstarPathfinderSystemGrid();
                return;
            }
            AstarP3Pathfinder.SetAxis(2);
            _pathfindingGrid = new SimpleThreadSafeGrid();
            _pathfindingSource = _pathfindingGrid;
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                _nonThreadedPathfinder = new AstarP3Pathfinder();
                return;
            }
#endif
            if (Game.GameActive && _useThreading) {
                StartThreads();
            }
            else {
                _nonThreadedPathfinder = new AstarP3Pathfinder();
            }
        }

        public override void Dispose() {
            base.Dispose();
            if (_threads != null) {
                StopThreads();
            }
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            if (!Game.GameActive) {
                return;
            }
            if (_threads == null && _useThreading) {
                StartThreads();
            }
            lock (ReturnLock) {
                ReturnCount = _toReturn.Count;
                for (var i = 0; i < _toReturn.Count; i++) {
                    if (i >= _toReturn.Count) {
                        break;
                    }
                    var item = _toReturn[i];
                    if (item.Path != null) {
                        for (int p = 0; p < item.Path.Count; p++) {
                            item.Grid.SetAgentCurrentPath(item.Path[p], item.ID, true);
                        }
                    }
                    if (item.Callback != null) {
                        item.Callback.Invoke(item.Result, item.Path);
                    }
                }
                _toReturn.Clear();
            }
            lock (QueueLock) {
                PendingCount = _pending.Count;
                for (var i = 0; i < _pending.Count; i++) {
                    if (i >= _pending.Count) {
                        break;
                    }
                    var request = _pending[i];
                    if (request.Path != null) {
                        for (int p = 0; p < request.Path.Count; p++) {
                            request.Grid.SetAgentCurrentPath(request.Path[p], request.ID, false);
                        }
                    }
                    if (!_useThreading) {
                        var result = _nonThreadedPathfinder.Run(request);
                        AddResponse(new PathReturn(request.ID, result, request.Path, request.ReturnEvent, request.Grid));
                        continue;
                    }
                    int lowest = int.MaxValue;
                    PathfindingThread thread = null;
                    for (var index = 0; index < _threads.Length; index++) {
                        var tryThread = _threads[index];
                        if (tryThread.Queue.Count < lowest) {
                            lowest = tryThread.Queue.Count;
                            thread = tryThread;
                        }
                    }
                    if (thread != null) {
                        thread.Queue.Enqueue(request);
                    }
                }
                _pending.Clear();
            }       
        }

        public void Enqueue(PathfindingRequest request) {
            if (request == null) {
                return;
            }
            if (!_pending.Contains(request)) {
                _pending.Add(request);
            }
            else {
                Debug.LogError("That pathfinding request was already submitted.");
            }
        }

        public void AddResponse(PathReturn pending) {
            // Prevent this from being called when currently returning stuff.
            lock (ReturnLock) {
                _toReturn.Add(pending);
            }
        }

        public void StartThreads() {
            _threads = new PathfindingThread[_threadCount];
            for (int i = 0; i < _threads.Length; i++) {
                _threads[i] = new PathfindingThread(this, new AstarP3Pathfinder(), i);
            }
            for (int i = 0; i < _threads.Length; i++) {
                var t = _threads[i];
                t.StartThread();
            }
        }

        public void StopThreads() {
            if (_threads == null) {
                return;
            }
            for (int i = 0; i < _threads.Length; i++) {
                var t = _threads[i];
                t.StopThread();
            }
        }

        public void Clear() {
            _pathfindingSource.ClearAll();
        }

        public Vector3 FindOpenPosition(Vector3 currentV3Pos, int wanderRadius) {
            return _pathfindingSource.FindOpenPosition(currentV3Pos, wanderRadius);
        }

        public void UpdatePlayerPosition(Vector3 position) {
            _pathfindingSource.SetPlayerPosition(position.toPoint3ZeroY(), Player.MainEntity, _playerWalkableRadius, _playerOccupyRadius);
        }

        public bool IsWalkable(Vector3 pos, bool isOversized) {
            return _pathfindingSource.IsWalkable(pos, isOversized);
        }

        public void SetWalkable(Bounds bounds, bool status) {
            _pathfindingSource.SetWalkable(bounds, status);
        }

        public void Scan() {
            _pathfindingSource.Scan();
        }

        public void SetCellSize(float size) {
            _pathfindingSource.SetCellSize(size);
        }
    }
}
