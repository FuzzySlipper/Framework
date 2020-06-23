//#define AStarPathfinding
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

#if AStarPathfinding
using Pathfinding;
#endif

namespace PixelComrades {
    public class PathfindingSelector : MonoBehaviour {

        [SerializeField] private Vector3 _offset = new Vector3(0, 0.25f, 0);
        [SerializeField] private Camera _cam = null;
        [SerializeField] private bool _onlyOnInput = true;
        [SerializeField] private bool _useUnityNavMesh = true;

        private RaycastHit[] _hits = new RaycastHit[10];
        private Ray _mouseRay;
#if AStarPathfinding
        private NNConstraint _constraint;
#endif
        void Awake() {
#if AStarPathfinding
            _constraint = NNConstraint.None;
            _constraint.constrainWalkability = true;
            _constraint.walkable = true;
#endif
        }

        void Update() {
            if (_onlyOnInput && !Input.GetMouseButtonDown(0)) {
                return;
            }
            _mouseRay = _cam != null ? _cam.ScreenPointToRay(Input.mousePosition) : WorldControlMonitor.Cam.ScreenPointToRay(Input.mousePosition);
            var cnt = Physics.RaycastNonAlloc(_mouseRay, _hits, 1200, LayerMasks.Floor);
            _hits.SortByDistanceAsc(cnt);
            if (cnt <= 0) {
                return;
            }
            if (_useUnityNavMesh) {
                for (int i = 0; i < cnt; i++) {
                    if (NavMesh.SamplePosition(_hits[i].point, out var hit, 500, NavMesh.AllAreas)) {
                        transform.position = hit.position + _offset;
                        return;
                    }
                }
                
            }
#if AStarPathfinding
            if (AstarPath.active != null) {
                var info = AstarPath.active.GetNearest(_hits[0].point, _constraint);
                if (info.node != null) {
                    transform.position = (Vector3) info.node.position + _offset;
                }
                return;
            }
#endif
            var pnt = _hits[0].point;
            if (World.Get<PathfindingSystem>().IsWalkable(pnt, false)) {
                transform.position = pnt + _offset;
            }
        }
    }
}
