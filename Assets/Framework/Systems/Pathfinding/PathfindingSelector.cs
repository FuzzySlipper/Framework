#define AStarPathfinding
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if AStarPathfinding
using Pathfinding;

namespace PixelComrades {
    public class PathfindingSelector : MonoBehaviour {

        [SerializeField] private Vector3 _offset = new Vector3(0, 0.25f, 0);
        [SerializeField] private Camera _cam = null;
        [SerializeField] private bool _onlyOnInput = true;

        private RaycastHit[] _hits = new RaycastHit[10];
        private NNConstraint _constraint;
        private Ray _mouseRay;
        void Awake() {
            _constraint = NNConstraint.None;
            _constraint.constrainWalkability = true;
            _constraint.walkable = true;
        }

        void Update() {
            if (CellGridGraph.Current != null) {
                //&& !PlayerInput.IsCursorOverUI
                if (_onlyOnInput && !Input.GetMouseButtonDown(0)) {
                    return;
                }
                _mouseRay = _cam != null ? _cam.ScreenPointToRay(Input.mousePosition) : WorldControlMonitor.Cam.ScreenPointToRay(Input.mousePosition);
                var cnt = Physics.RaycastNonAlloc(_mouseRay, _hits, 1200, LayerMasks.Floor);
                _hits.SortByDistanceAsc(cnt);
                if (cnt > 0) {
                    var info = CellGridGraph.Current.GetNearest(_hits[0].point, _constraint);
                    if (info.node != null) {
                        transform.position = (Vector3)info.node.position + _offset;
                    }
                }
            }
        }
    }
}
#endif