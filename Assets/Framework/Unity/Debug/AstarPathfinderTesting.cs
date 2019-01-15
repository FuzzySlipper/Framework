using UnityEngine;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using PixelComrades.DungeonCrawler;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class AstarPathfinderTesting : MonoBehaviour {

        [SerializeField] private GameObject _debugGameObject = null;
        [SerializeField] private Transform _moveTarget = null;
        [SerializeField] private bool _useDebug = true;
        [Range(0, 500)] private int _characterCount = 15;
        [SerializeField] private GameObject _characterPrefab;
        [SerializeField] private float _speedMod = 1;
        [SerializeField] private float _movementSpeed = 5f;
        
        private List<Entity> _entities = new List<Entity>();
        private Point3 _lastPositionP3 = Point3.max;


        void Awake() {
            SetupPathfinding();
            ToggleDoors();
            Game.SetGameActive(true);
        }

        void Update() {
            UpdateRegularPathfinding();
        }

        [Button("Clear")]
        private void Clear() {
            World.Get<PathfindingSystem>().DestroyGrid();
        }

        private void OnGUI() {
            _speedMod = GUILayout.HorizontalSlider(_speedMod, 0.05f, 2f);
            if (GUILayout.Button("Spawn " + _characterCount + " characters")) {
                SpawnTestAgents();
            }
            if (GUILayout.Button("Spawn " + _characterCount + " NPCs")) {
                SpawnNPCs();
            }
            if (GUILayout.Button("Clear all characters")) {
                foreach (var item in _entities) {
                    item.Destroy();
                    Destroy(item.Tr.gameObject);
                }
                World.Get<PathfindingSystem>().Grid.ClearAll();
            }
            //if (GUILayout.Button("Everyone wander")) {
            //    _simpleGrid.Clear();
            //    for (int i = 0; i < _nodeList.Count; i++) {
            //        _nodeList[i].SetEnd(FindWander(_nodeList[i].CurrentPos));
            //    }
            //}
        }

        [Button("Setup Simple Pathfinding")]
        private void SetupPathfinding() {
            GlobalLevelController.SetupCells();
            var grid = (SimpleThreadSafeGrid) World.Get<PathfindingSystem>().Grid;
            GlobalLevelController.SetupPathfinding();
        }

        private void SpawnTestAgents() {
            var grid = (SimpleThreadSafeGrid) World.Get<PathfindingSystem>().Grid;
            for (int i = 0; i < _characterCount; i++) {
                var spawned = Instantiate(_characterPrefab);
                var entity = Entity.New("TestPathfind " + _entities.Count);
                entity.Tr = spawned.transform;
                entity.Add(new MoveSpeed(_movementSpeed));
                entity.Add(new MoveTarget());
                var agent = World.Get<PathfinderMoverSystem>().SetupPathfindEntity(entity);
                entity.Add(new PathfindingDebugging(spawned.GetComponentInChildren<LineRenderer>(), spawned.GetComponentInChildren<TextMesh>()));
                var pos = grid.GetOpenWalkablePosition();
                agent.SetPosition(pos);
                grid.SetStationaryAgent(pos, agent.Owner, true);
                _entities.Add(entity);
                entity.Post(new SetMoveTarget(null, World.Get<PathfindingSystem>().FindWander(entity.Tr.position, 15), null));
            }
        }

        private void SpawnNPCs() {
            var grid = (SimpleThreadSafeGrid) World.Get<PathfindingSystem>().Grid;
            for (int i = 0; i < _characterCount; i++) {
                var entity = NpcEntityFactory.GetRandom();
                entity.Add(new MoveTarget());
                if (_useDebug && _debugGameObject != null) {
                    var spawned = Instantiate(_debugGameObject);
                    spawned.transform.SetParent(entity.Tr);
                    spawned.transform.localPosition = new Vector3(0, 2, 0);
                    entity.Add(new PathfindingDebugging(spawned.GetComponentInChildren<LineRenderer>(), spawned.GetComponentInChildren<TextMesh>()));
                }
                var pos = grid.GetOpenWalkablePosition();
                entity.Get<SimplePathfindingAgent>().SetPosition(pos);
                grid.SetStationaryAgent(pos, entity, true);
                _entities.Add(entity);
                entity.Post(new SetMoveTarget(null, World.Get<PathfindingSystem>().FindWander(entity.Tr.position, 15), null));
            }
        }

        private void UpdateRegularPathfinding() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                ToggleDoors();
            }
            var cell = new Point3(_moveTarget.position);
            cell.y = 0;
            if (cell != _lastPositionP3) {
                _lastPositionP3 = cell;
                World.Get<PathfindingSystem>().Grid.ClearLocks();
                for (int i = 0; i < _entities.Count; i++) {
                    _entities[i].Post(new SetMoveTarget(null, _lastPositionP3.toVector3(), null));
                }
            }
        }

        private void ToggleDoors() {
            var doors = LevelBuilder.main.GetComponentsInChildren<Door>();
            for (int i = 0; i < doors.Length; i++) {
                doors[i].OverrideLock(false);
                doors[i].ToggleDoor(Vector3.zero);
            }
        }

        public void RunActionOnCells(System.Action<LevelCell> del) {
            var enumerator = GlobalLevelController.Cells.GetEnumerator();
            try {
                while (enumerator.MoveNext()) {
                    var cell = enumerator.Current.Value;
                    del(cell);
                }
            }
            finally {
                enumerator.Dispose();
            }
        }

#if UNITY_EDITOR

        private Point3[] _spiralPoints;

        [SerializeField] private Transform _targetParent = null;
        [SerializeField] private Transform _blockedParent = null;

        [Button("Clear Spiral")]
        private void ClearSpiral() {
            _spiralPoints = null;
        }

        [Button("Init GameData")]
        private void InitGameData() {
            GameData.Init();
        }

        [Button("Create Spiral")]
        private void CreateSpiral() {
            _spiralPoints = null;
            _spiralPoints = new Point3[25];
            for (int i = 0; i < _spiralPoints.Length; i++) {
                _spiralPoints[i] = GridExtension.GridSpiralP3(i);
            }
        }

        private List<DebugPathfind> _pathfinderTest;
        [SerializeField] private Vector3 _target = new Vector3(0, 0, 10);

        private class DebugPathfind {
            public Vector3 Position;
            public Color Color;
            public float StarCost;
            public float EndCost;
            public float TotalCost { get { return StarCost + EndCost; } }

            public DebugPathfind(Vector3 position, Color color, float starCost, float endCost) {
                Position = position;
                Color = color;
                StarCost = starCost;
                EndCost = endCost;
            }
        }

        [Button("Clear PathfindTest")]
        private void ClearPathfind() {
            _pathfinderTest = null;
        }

        [Button("Create PathfindTest")]
        private void CreatePathfindTest() {
            SetupPathfinding();
            _pathfinderTest = new List<DebugPathfind>();
            var pathfinder = new AstarP3Pathfinder();
            AstarP3Pathfinder.SetAxis(2);
            var grid = World.Get<PathfindingSystem>().Grid;
            var nodePath = new List<Point3>();
            var start = transform.position.toPoint3();
            var result = pathfinder.Run(PathfindingRequest.Create(grid, 0, start, (transform.position + _target).toPoint3(), null, false, nodePath));
            if (nodePath.Count == 0) {
                Debug.Log(pathfinder.KeyedDict.Count + " " + result);
                grid.ClearAll();
                return;
            }
            var startCost = pathfinder.KeyedDict[nodePath[0]].TotalCost;
            var endNode = pathfinder.KeyedDict[nodePath.LastElement()];
            endNode.EndCost = 0;
            endNode.StartCost = Vector3.Distance(nodePath[0].toVector3(), endNode.Value.toVector3());
            float maxCost = startCost;
            foreach (var p3Node in pathfinder.KeyedDict) {
                if (p3Node.Value.TotalCost > maxCost) {
                    maxCost = p3Node.Value.TotalCost;
                }
            }
            foreach (var p3Node in pathfinder.KeyedDict) {
                Color nodeColor;
                if (nodePath.Contains(p3Node.Value.Value)) {
                    nodeColor = Color.green;
                }
                else {
                    nodeColor = Color.Lerp(Color.white, Color.red, Mathf.InverseLerp(startCost, maxCost, p3Node.Value.TotalCost));
                }
                var node = new DebugPathfind(p3Node.Value.Value.toVector3(), nodeColor, p3Node.Value.StartCost, p3Node.Value.EndCost);
                _pathfinderTest.Add(node);
            }
            grid.ClearAll();
        }

        [Button("Create Obstacles")]
        private void CreateObstacles() {
            AstarP3Pathfinder.SetAxis(2);
            SetupPathfinding();
            var grid = World.Get<PathfindingSystem>().Grid;
            if (_targetParent != null) {
                foreach (Transform child in _targetParent) {
                    grid.SetAgentCurrentPath(child.transform.position.toPoint3ZeroY(), 5, true);
                }
            }
            if (_blockedParent != null) {
                foreach (Transform child in _blockedParent) {
                    grid.SetStationaryAgent(child.transform.position.toPoint3ZeroY(), 5, true);
                }
            }
        }
    
        [Button("Create Complicated PathfindTest")]
        private void PathfindingComplicatedTest() {
            CreateObstacles();
            _pathfinderTest = new List<DebugPathfind>();
            var pathfinder = new AstarP3Pathfinder();
            var grid = World.Get<PathfindingSystem>().Grid;
            var nodePath = new List<Point3>();
            var start = transform.position.toPoint3();
            var result = pathfinder.Run(PathfindingRequest.Create(grid, 0, start, (transform.position + _target).toPoint3(), null, false, nodePath));
            if (nodePath.Count == 0) {
                Debug.Log(pathfinder.KeyedDict.Count + " " + result);
                grid.ClearAll();
                return;
            }
            var startCost = pathfinder.KeyedDict[nodePath[0]].TotalCost;
            var endNode = pathfinder.KeyedDict[nodePath.LastElement()];
            endNode.EndCost = 0;
            endNode.StartCost = Vector3.Distance(nodePath[0].toVector3(), endNode.Value.toVector3());
            float maxCost = startCost;
            foreach (var p3Node in pathfinder.KeyedDict) {
                if (p3Node.Value.TotalCost > maxCost) {
                    maxCost = p3Node.Value.TotalCost;
                }
            }
            foreach (var p3Node in pathfinder.KeyedDict) {
                Color nodeColor;
                if (nodePath.Contains(p3Node.Value.Value)) {
                    nodeColor = Color.green;
                }
                else {
                    nodeColor = Color.Lerp(Color.white, Color.red, Mathf.InverseLerp(startCost, maxCost, p3Node.Value.TotalCost));
                }
                var node = new DebugPathfind(p3Node.Value.Value.toVector3(), nodeColor, p3Node.Value.StartCost, p3Node.Value.EndCost);
                _pathfinderTest.Add(node);
            }
            Debug.LogFormat("Test took {0} nodes for path {1}", pathfinder.KeyedDict.Count, nodePath.Count);
            grid.ClearAll();
        }

        private GameOptions.CachedFloat _defaultCost = new GameOptions.CachedFloat("PathfindGridDefaultCost");
        private GameOptions.CachedFloat _occupiedCost = new GameOptions.CachedFloat("PathfindGridOccupiedCost");
        private GameOptions.CachedFloat _playerCost = new GameOptions.CachedFloat("PathfindGridPlayerCost");

        [SerializeField] private float _magCheck = 1.1f;
        [SerializeField] private float _axisDiscount = 0.1f;
        [SerializeField] private float _simpleEndCode = 1.1f;

        [Button]
        public void UpdateCost() {
            //AstarP3Pathfinder.P3Node.MagnitudeAdjustment = _magCheck;
            AstarP3Pathfinder.P3Node.SimpleEndCostMulti = _simpleEndCode;
            //AstarP3Pathfinder.P3Node.OnAxisDiscount = _axisDiscount;
        }

        void OnDrawGizmos() {
            Gizmos.DrawSphere((transform.position + _target), 0.5f);
            if (_pathfinderTest != null) {
                for (int i = 0; i < _pathfinderTest.Count; i++) {
                    var node = _pathfinderTest[i];
                    UnityEditor.Handles.color = node.Color;
                    Gizmos.color = node.Color;
                    Gizmos.DrawWireCube(node.Position, Vector3.one * 0.9f);
                    UnityEditor.Handles.Label(node.Position, node.TotalCost.ToString("F1") ); //string.Format("{0:F1}/{1:F1}", node.StarCost, node.EndCost));
                }
            }
            if (_spiralPoints != null) {
                var center = new Point3(transform.position);
                for (int i = 0; i < _spiralPoints.Length; i++) {
                    var pos = center + _spiralPoints[i];
                    UnityEditor.Handles.Label(pos.toVector3(), i.ToString());
                    Gizmos.DrawWireCube(pos.toVector3(), Vector3.one);
                }
            }
            var simpleGrid = World.Get<PathfindingSystem>().Grid;
            if (simpleGrid == null || simpleGrid.CellsCount == 0) {
                return;
            }
            RunActionOnCells(
                c => {
                    var p3 = new Point3(c.WorldPositionV3);
                    var size = Game.MapCellSize / 2;
                    for (int x = -size; x <= size; x++) {
                        for (int z = -size; z <= size; z++) {
                            var pos = new Point3(p3.x + x, p3.y, p3.z + z);
                            Color color;
                            if (!simpleGrid.IsWalkable(pos, false)) {
                                color = Color.red;
                            }
                            else {
                                color = Color.Lerp(Color.green, Color.yellow, Mathf.InverseLerp(_defaultCost, _occupiedCost, simpleGrid.GetTraversalCost(pos)));
                            }
                            //else if (simpleGrid.Targets.ContainsKey(pos)) {
                            //    color = Color.blue;
                            //}
                            Gizmos.color = color;
                            Gizmos.DrawWireCube(pos.toVector3(), Vector3.one * 0.9f);
                            //UnityEditor.Handles.Label(pos.toVector3(), string.Format("{0}{2}{1}", pos, simpleGrid.GetTraversalCost(pos).ToString("F1"), System.Environment.NewLine));
                        }
                    }
                });
            //for (int i = 0; i < _cells.Count; i++) {
            //    var cell = _cells[i];
            //    var pos = cell.Position;
            //    var walkable = cell.Walkable;
            //    Gizmos.color = walkable ? Color.green : Color.red;
            //    Gizmos.DrawWireCube(pos.toVector3() + (Vector3.one * _drawOffset), Vector3.one);
            //    //UnityEditor.Handles.Label(pos.toVector3() + (Vector3.one * _drawOffset), string.Format("O:{0}{3}P:{1}{3}W:{2}", cell.Origin, cell.Position, cell.CheckDir, System.Environment.NewLine));
            //}
        }

#endif
    }
}