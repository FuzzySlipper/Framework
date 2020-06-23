using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;

namespace PixelComrades {
    public interface IWorldControl {
        bool WorldControlActive { get; }
        void OnControlUse();
        string OnControlHovered(bool status);
    }

    public interface IMenuControl {
        bool Active { get; }
        void ToggleActive();
        void SetSceneStatus(bool status);
        void SetStatus(bool status);
    }

    public class WorldControlMonitor : MonoBehaviour {

        private static WorldControlMonitor _main;

        [SerializeField] private LayerMask _worldControlsMask = new LayerMask();
        [SerializeField] private float _rayDistance = 15f;
        [SerializeField] private float _enemyDistance = 25f;
        [SerializeField] private float _clearControlTime = 1f;
        [SerializeField] private float _clickTimeoutTime = 0.65f;
        [SerializeField] private bool _debug = false;

        private Camera _cam;
        private IWorldControl _current = null;
        private GameObject _currentGameObject;
        private Ray _mouseRay;
        private float _currentRayDistance;
        private float _currentEnemyRayDistance;
        private RaycastHit[] _hits = new RaycastHit[10];
        private bool _foundControl;
        private static UnscaledTimer _clickTimeout = new UnscaledTimer(0.5f);
        //private List<IMenuControl> _openMenu = new List<IMenuControl>();
        private TriggerableUnscaledTimer _clearTimer = new TriggerableUnscaledTimer();
        private List<IWorldControl> _tempControls = new List<IWorldControl>(25);

        public static System.Action AlternativeUse;
        public static IWorldControl Current { get { return _main != null ? _main._current : null; } }
        public static GameObject CurrentGameObject { get { return _main != null ? _main._currentGameObject : null; } }
        public static Camera Cam { get { return _main != null && _main._cam != null ? _main._cam : Player.Cam; } }

        public static void SetCamera(Camera cam) {
            if (_main == null) {
                return;
            }
            _main._cam = cam;
        }

        public static void OverrideDistance(float newDistance) {
            if (_main == null) {
                return;
            }
            _main._currentRayDistance = newDistance;
            _main._currentEnemyRayDistance = newDistance*1.5f;
        }

        public static void ResetDistance() {
            if (_main == null) {
                return;
            }
            _main._currentRayDistance = _main._rayDistance;
            _main._currentEnemyRayDistance = _main._enemyDistance;
        }

        public static bool Use() {
            if (_clickTimeout.IsActive || _main == null) {
                return false;
            }
            _clickTimeout.StartNewTime(_main._clickTimeoutTime);
            if ( _main._current != null) {
                _main._current.OnControlUse();
                return true;
            }
            if (AlternativeUse != null) {
                AlternativeUse();
                return true;
            }
            return false;
        }

        public static void ClearCurrent() {
            if (_main == null) {
                return;
            }
            _main.ClearCurrentInternal();
        }

        [Button("Update Current")]
        private void UpdateCurrent() {
            _currentEnemyRayDistance = _enemyDistance;
            _currentRayDistance = _rayDistance;
        }

        void Awake() {
            _main = this;
            _currentRayDistance = _rayDistance;
            _currentEnemyRayDistance = _enemyDistance;
        }

        public void ShowDebug() {
            _debug = !_debug;
        }

        void Update() {
            if (!Game.GameActive) {
                return;
            }
            //if (_debug) {
            //    DebugText.UpdatePermText("World Control", "Game Not Active");
            //}
            //if (GenericDraggableController.Dragging) {
            //    if (_debug) {
            //        DebugText.UpdatePermText("World Control", "Dragging");
            //    }
            //    return;
            //}
            _foundControl = false;
            //_eventData.position = Input.mousePosition;
            //EventSystem.current.RaycastAll(_eventData, _result);
            if (PlayerInputSystem.IsCursorOverUI) {
                SetCurrentNull();
                //if (_debug) {
                //    var go = PlayerInput.CurrentInput != null ? PlayerInput.CurrentInput.GameObjectUnderPointer() : null;
                //    DebugText.UpdatePermText("World Control", string.Format("Over UI: {0}", go != null ? go.name :
                //        EventSystem.current.currentSelectedGameObject != null ? EventSystem.current.currentSelectedGameObject.name : " null ?"));
                //}
                return;
            }
            _mouseRay = Cam.ScreenPointToRay(Input.mousePosition);
            Entity currentActor = null;
            var cnt = Physics.RaycastNonAlloc(_mouseRay, _hits, _currentEnemyRayDistance, _worldControlsMask);
            _hits.SortByDistanceAsc(cnt);
            for (int i = 0; i < cnt; i++) {
                var hit = _hits[i];
                if (!_foundControl) {
                    if (hit.transform.gameObject == _currentGameObject) {
                        _foundControl = true;
                        continue;
                    }
                    if (hit.distance <= _currentRayDistance) {
                        hit.transform.GetComponentsInChildren(_tempControls);
                        for (int j = 0; j < _tempControls.Count; j++) {
                            if (_tempControls[j] == null) {
                                continue;
                            }
                            if (_tempControls[j].WorldControlActive) {
                                SetNewCurrent(_tempControls[j], hit.transform.gameObject);
                                _foundControl = true;
                                break;
                            }
                        }
                    }
                }
                if (currentActor == null) {
                    currentActor = UnityToEntityBridge.GetEntity(hit.collider);
                }
                if (LayerMasks.Environment.ContainsLayer(hit.transform.gameObject.layer)) {
                    if (_debug) {
                        DebugText.UpdatePermText("World Control", "Environment " + cnt);
                    }
                    break;
                }
            }

            if (currentActor != null) {
                if (_debug) {
                    DebugText.UpdatePermText("World Control", currentActor.Id.ToString());
                }
            }
            //RaycastHit hit;
            //if (Physics.Raycast(_mouseRay, out hit, _enemyDistance, _worldControlsMask)) {
            //    if (hit.transform.gameObject == _currentGameObject) {
            //        _foundControl = true;
            //    }
            //    else {
            //        if (hit.distance <= _rayDistance) {
            //            var newCurrent = hit.transform.GetComponent<IWorldControl>();
            //            if (newCurrent != null) {
            //                SetNewCurrent(newCurrent, hit.transform.gameObject);
            //                _foundControl = true;
            //            }
            //        }
            //        else {
            //            if (currentActor == null) {
            //                Actor.Actors.TryGetValue(hit.collider, out currentActor);
            //            }
            //        }
            //    }
            //}
            UICenterTarget.SetTargetActor(currentActor.GetTemplate<CharacterTemplate>());
            if (!_foundControl || currentActor != null) {
                SetCurrentNull();
            }
            if (_debug) {
                DebugText.UpdatePermText("World Control", string.Format("Current: {0}", _currentGameObject != null ? _currentGameObject.name : "None"));
            }
            //if (_current == null && AlternativeUse == null) {
            //    var wasActive = GenericDraggableController.HasTarget;
            //    if (GenericDraggableController.main.CanDrag()) {
            //        UICenterTarget.SetText("Drag");
            //    }
            //    else if (wasActive){
            //        UICenterTarget.Clear();
            //    }
            //}
            //else {
            //    GenericDraggableController.ClearTarget();
            //}
        }

        private void SetNewCurrent(IWorldControl newCurrent, GameObject newGo) {
            _foundControl = true;
            _clearTimer.Triggered = false;
            if (_current == newCurrent) {
                return;
            }
            if (_current != null) {
                UICenterTarget.Clear();
                _current.OnControlHovered(false);
            }
            _currentGameObject = newGo;
            _current = newCurrent;
            UICenterTarget.SetText(_current.OnControlHovered(true));
        }

        private void SetCurrentNull() {
            if (!_clearTimer.Triggered) {
                _clearTimer.StartNewTime(_clearControlTime);
                return;
            }
            if (_clearTimer.Triggered && _clearTimer.IsActive) {
                return;
            }
            ClearCurrentInternal();
        }

        private void ClearCurrentInternal() {
            if (_current != null) {
                UICenterTarget.Clear();
                _current.OnControlHovered(false);
            }
            _current = null;
            _currentGameObject = null;
        }
    }
}