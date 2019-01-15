using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PixelComrades {
    public class PathfindingThread {
        /// <summary>
        ///     The amount of time, in milliseconds, that is spent idling when no new request is detected. 0 means no sleep time,
        ///     ever.
        ///     Lower values mean that path responses will be faster, often the very next frame. However, it will use more of the
        ///     CPU time when there are no requests.
        ///     Higher values mean that the responses will be slower, but the CPU isn't used as much when there are no requests.
        ///     Default value is 5, which still allows for next-frame responses at 60fps.
        /// </summary>
        private const int IDLE_SLEEP = 5;

        public Queue<PathfindingRequest> Queue = new Queue<PathfindingRequest>();

        public float ApproximateWork { get; private set; }
        public long LatestTime { get; private set; }
        public long TimeThisSecond { get; private set; }
        public Thread Thread { get; private set; }
        public int ThreadNumber { get;}
        public IPathfinder Pathfinder { get; }

        private Stopwatch _secondWatch;
        private Stopwatch _watch;
        private PathfindingSystem _system;

        public PathfindingThread(PathfindingSystem pathfindingSystem, IPathfinder pathfinder, int number) {
            ThreadNumber = number;
            Pathfinder = pathfinder;
            _system = pathfindingSystem;
            _watch = new Stopwatch();
            _secondWatch = new Stopwatch();
        }

        public bool Run { get; private set; }

        public void RunThread(object n) {
            int number = (int) n;
            _secondWatch.Start();
            while (Run) {
                try {
                    if (_secondWatch.ElapsedMilliseconds >= 1000) {
                        _secondWatch.Reset();
                        _secondWatch.Start();
                        ApproximateWork = Mathf.Clamp01(TimeThisSecond / 1000f);
                        TimeThisSecond = 0;
                    }
                    int count = Queue.Count;
                    if (count == 0) {
                        Thread.Sleep(IDLE_SLEEP);
                    }
                    else {
                        // Lock to prevent simultaneous read and write.
                        PathfindingRequest request;
                        lock (_system.QueueLock) {
                            request = Queue.Dequeue();
                        }
                        if (request == null) {
                            continue;
                        }
                        if (request.ReturnEvent == null) {
                            continue;
                        }
                        _watch.Reset();
                        _watch.Start();
                        var result = Pathfinder.Run(request);
                        _watch.Stop();
                        LatestTime = _watch.ElapsedMilliseconds;
                        TimeThisSecond += LatestTime;
                        _system.AddResponse(new PathReturn(request.ID, result, request.Path, request.ReturnEvent, request.Grid));
                    }
                }
                catch (Exception e) {
                    Debug.Log("Exception in pathfinding thread #" + number + "! Execution on this thread will attempt to continue as normal. See:");
                    Debug.LogError(e);
                }
            }
            _secondWatch.Stop();
        }

        public void StartThread() {
            if (Run) {
                return;
            }
            Run = true;
            Thread = new Thread(RunThread);
            Thread.Start(ThreadNumber);
        }

        public void StopThread() {
            Run = false;
        }
    }
}