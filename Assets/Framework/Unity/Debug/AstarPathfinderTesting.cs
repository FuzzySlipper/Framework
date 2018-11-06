using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using PixelComrades.DungeonCrawler;

namespace PixelComrades {
    public class AstarPathfinderTesting : MonoBehaviour {

        [SerializeField] private int _amountToTest = 15;
        [SerializeField] private Transform _moveTarget = null;

        private List<Entity> _entities = new List<Entity>();
        private GraphNode _lastPosition;

        void Awake() {
            NodeFilter<PathfindMoverNode>.New(PathfindMoverNode.GetTypes());
            World.Get<AstarMoverSystem>();
            Game.SetGameActive(true);
            TimeManager.StartUnscaled(LoadTest());
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.D)) {
                ToggleDoors();
            }
            var node = AstarPath.active.GetNearest(_moveTarget.position);
            if (node.node == _lastPosition) {
                return;
            }
            _lastPosition = node.node;
            for (int i = 0; i < _entities.Count; i++) {
                var entityNode = AstarPath.active.GetNearest(_entities[i].GetPosition());
                if (entityNode.node == _lastPosition || entityNode.node.ContainsConnection(_lastPosition)) {
                    continue;
                }
                _entities[i].Post(new SetTarget(_moveTarget, null));
            }
        }

        private void ToggleDoors() {
            var doors = GlobalLevelController.Loaded.GetComponentsInChildren<Door>();
            for (int i = 0; i < doors.Length; i++) {
                doors[i].OverrideLock(false);
                doors[i].ToggleDoor(Vector3.zero);
            }
        }

        private IEnumerator LoadTest() {
            yield return 0.1f;
            ToggleDoors();
            int loaded = 0;
            while (loaded < _amountToTest) {
                var npc = NpcEntityFactory.GetRandom();
                _entities.Add(npc);
                var tr = npc.Get<TransformComponent>().Tr;
                //var coll = tr.GetComponent<Collider>();
                //var astarHolder = Instantiate(_astarPrefab);
                LevelCellHolder localCell = null;
                while (localCell == null) {
                    var cell = GlobalLevelController.Loaded.Cells.RandomElement();
                    if (cell.Walkable) {
                        localCell = cell;
                    }
                }
                //var seeker = astarHolder.GetComponent<Seeker>();
                //tr.SetParentResetPos(astarHolder.transform);
                npc.Add(new MoveTarget());
                tr.transform.position = World.Get<CellMapSystem>().GetLevelCell(localCell.OriginalPos).WorldBottomV3;
                npc.Add(new AstarPathfinderData(tr.gameObject.AddComponent<Seeker>()));
                npc.Stats.SetMax();
                //npc.Tags.Add(EntityTags.Moving);
                loaded++;
                yield return null;
            }
        }
    }
}
