using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static class SystemManager {

        private static List<ISystemUpdate> _everyUpdate = new List<ISystemUpdate>();
        private static List<ISystemFixedUpdate> _fixedUpdate = new List<ISystemFixedUpdate>();
        private static List<ITurnUpdate> _turnUpdates = new List<ITurnUpdate>();

        public static void Add(ISystemUpdate update) {
            //CheckAddUpdate(update);
            _everyUpdate.Add(update);
        }

        public static void AddFixed(ISystemFixedUpdate update) {
            _fixedUpdate.Add(update);
        }

        public static void AddTurn(ITurnUpdate update) {
            _turnUpdates.Add(update);
        }

        public static void Remove(ISystemUpdate update) {
            RemoveUpdate(update);
        }

        public static void Remove(ISystemFixedUpdate update) {
            _fixedUpdate.Remove(update);
        }

        public static void RemoveTurn(ITurnUpdate update) {
            _turnUpdates.Remove(update);
        }

        public static void TurnUpdate(bool fullTurn) {
            for (int i = 0; i < _turnUpdates.Count; i++) {
                _turnUpdates[i].TurnUpdate(fullTurn);
            }
        }

        public static void CheckForDuplicates() {
            for (int i = 0; i < _everyUpdate.Count; i++) {
                var update = _everyUpdate[i];
                for (int t = 0; t < _everyUpdate.Count; t++) {
                    if (t == i) {
                        continue;
                    }
                    var otherUpdate = _everyUpdate[t];
                    if (update == otherUpdate) {
                        var unityUpdate = update as UnityEngine.Component;
                        var otherUnity = otherUpdate as UnityEngine.Component;
                        if (unityUpdate != null && otherUnity != null) {
                            Debug.LogErrorFormat("Update {0} / {1} is in list twice", unityUpdate.transform.name, otherUnity.transform.name);
                        }
                        else {
                            Debug.LogErrorFormat("Update {0} / {1} is in list twice", update.ToString(), otherUpdate.ToString());
                        }
                        break;
                    }
                }
            }
            for (int i = 0; i < _turnUpdates.Count; i++) {
                var update = _turnUpdates[i];
                for (int t = 0; t < _turnUpdates.Count; t++) {
                    if (t == i) {
                        continue;
                    }
                    var otherUpdate = _turnUpdates[t];
                    if (update == otherUpdate) {
                        var unityUpdate = update as UnityEngine.Component;
                        var otherUnity = otherUpdate as UnityEngine.Component;
                        if (unityUpdate != null && otherUnity != null) {
                            Debug.LogErrorFormat("Turn {0} / {1} is in list twice", unityUpdate.transform.name, otherUnity.transform.name);
                        }
                        else {
                            Debug.LogErrorFormat("Turn {0} / {1} is in list twice", update.ToString(), otherUpdate.ToString());
                        }
                        break;
                    }
                }
            }
        }

        private static void CheckAddUpdate(ISystemUpdate newUpdate) {
            if (!_everyUpdate.Contains(newUpdate)) {
                _everyUpdate.Add(newUpdate);
            }
        }

        private static void RemoveUpdate(ISystemUpdate oldUpdate) {
            _everyUpdate.Remove(oldUpdate);
        }

        public static void SystemUpdate() {
            for (int i = _everyUpdate.Count - 1; i >= 0; i--) {
                if (_everyUpdate[i] == null) {
                    _everyUpdate.RemoveAt(i);
                    continue;
                }
                if (Game.Paused && !_everyUpdate[i].Unscaled) {
                    continue;
                }
                _everyUpdate[i].OnSystemUpdate(_everyUpdate[i].Unscaled ? TimeManager.DeltaUnscaled : TimeManager.DeltaTime);
            }
        }

        public static void FixedSystemUpdate(float delta) {
            if (Game.Paused) {
                return;
            }
            for (int i = 0; i < _fixedUpdate.Count; i++) {
                _fixedUpdate[i].OnFixedSystemUpdate(delta);
            }
        }
    }

}