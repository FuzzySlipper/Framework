using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if AStarPathfinding
using Pathfinding;
using UnityEngine.EventSystems;

namespace PixelComrades {
    public class CombatModeInput : MonoBehaviour {

        [SerializeField] private LayerMask _mask = new LayerMask();
        [SerializeField] private float _rayDistance = 15f;
        [SerializeField] private RtsCamera _cam = null;

        private RaycastHit[] _hits = new RaycastHit[10];
        private Ray _mouseRay;
        private NNConstraint _constraint;

        void Awake() {
            _constraint = NNConstraint.None;
            _constraint.constrainWalkability = true;
            _constraint.walkable = true;
        }

        void Update() {
            if (!Game.GameActive || PlayerInput.IsCursorOverUI) {
                return;
            }
            bool leftClick = CheckLeftClick();
            bool rightClick = CheckRightClick();
            if (!leftClick && !rightClick) {
                _cam.UpdateInput();
                return;
            }
            _mouseRay = WorldControlMonitor.Cam.ScreenPointToRay(Input.mousePosition);
            var cnt = Physics.RaycastNonAlloc(_mouseRay, _hits, _rayDistance, _mask);
            _hits.SortByDistanceAsc(cnt);
            for (int i = 0; i < cnt; i++) {
                var hit = _hits[i];
                Actor actor = Actor.Get(hit.collider);
                if (actor != null) {
                    if (leftClick) {
                        if (actor.Faction == Factions.Enemy && Player.SelectedActor != null) {
                            var attack = Player.SelectedActor.GetAttack(Player.SelectedActor.ActionDistance(UICenterTarget.CurrentActor));
                            if (!attack.TryStart(UICenterTarget.CurrentActor)) {
                                UIFloatingText.Spawn(attack.LastStatusUpdate, 2f, Player.SelectedActor.PartySlot.RectTr, Color.yellow);
                            }
                        }
                        //else {
                        //    var playActor = actor as PlayerActor;
                        //    if (playActor != null) {
                        //        Player.SelectedActor = playActor;
                        //    }
                        //}
                    }
                    if (rightClick) {
                        UIActionController.main.OpenRadial(actor);
                    }
                    continue;
                }
                if (LayerMasks.Environment.ContainsLayer(hit.transform.gameObject.layer)) {
                    if (Player.SelectedActor != null) {
                        var info = CellGridGraph.Current.GetNearest(hit.point, _constraint);
                        if (info.node != null) {
                            Player.SelectedActor.StateController.MoveTo((Vector3) info.node.position);
                        }
                    }
                }
            }
        }

        private bool CheckRightClick() {
            if (PlayerInput.IsCursorOverUI || !Input.GetMouseButtonDown(1)) {
                return false;
            }
            if (UISubMenu.Default.Active || UIRadialMenu.Active) {
                return false;
            }
            return true;
        }

        private bool CheckLeftClick() {
            if (PlayerInput.IsCursorOverUI || !Input.GetMouseButtonDown(0)) {
                return false;
            }
            if (UISubMenu.Default.Active || UIRadialMenu.Active) {
                return false;
            }
            if (UIDragDropHandler.CurrentData != null && UIDropWorldPanel.Active) {
                return false;
            }
            if (WorldControlMonitor.Use()) {
                return false;
            }
            return true;
        }
    }
}
#endif