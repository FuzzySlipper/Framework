using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static class SystemManager {

        public static List<ISystemUpdate> EveryUpdate = new List<ISystemUpdate>();
        public static List<ISystemFixedUpdate> FixedUpdate = new List<ISystemFixedUpdate>();
        public static List<ITurnUpdate> TurnUpdates = new List<ITurnUpdate>();

        public static void Add(ISystemUpdate update) {
            //CheckAddUpdate(update);
            EveryUpdate.Add(update);
        }

        public static void AddFixed(ISystemFixedUpdate update) {
            FixedUpdate.Add(update);
        }

        public static void AddTurn(ITurnUpdate update) {
            TurnUpdates.Add(update);
        }

        public static void Remove(ISystemUpdate update) {
            RemoveUpdate(update);
        }

        public static void Remove(ISystemFixedUpdate update) {
            FixedUpdate.Remove(update);
        }

        public static void RemoveTurn(ITurnUpdate update) {
            TurnUpdates.Remove(update);
        }

        public static void TurnUpdate(bool fullTurn) {
            for (int i = 0; i < TurnUpdates.Count; i++) {
                TurnUpdates[i].TurnUpdate(fullTurn);
            }
        }

        public static void CheckForDuplicates() {
            for (int i = 0; i < EveryUpdate.Count; i++) {
                var update = EveryUpdate[i];
                for (int t = 0; t < EveryUpdate.Count; t++) {
                    if (t == i) {
                        continue;
                    }
                    var otherUpdate = EveryUpdate[t];
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
            for (int i = 0; i < TurnUpdates.Count; i++) {
                var update = TurnUpdates[i];
                for (int t = 0; t < TurnUpdates.Count; t++) {
                    if (t == i) {
                        continue;
                    }
                    var otherUpdate = TurnUpdates[t];
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
            if (!EveryUpdate.Contains(newUpdate)) {
                EveryUpdate.Add(newUpdate);
            }
        }

        private static void RemoveUpdate(ISystemUpdate oldUpdate) {
            EveryUpdate.Remove(oldUpdate);
        }

        public static void SystemUpdate() {
            for (int i = EveryUpdate.Count - 1; i >= 0; i--) {
                if (EveryUpdate[i] == null) {
                    EveryUpdate.RemoveAt(i);
                    continue;
                }
                if (Game.Paused && !EveryUpdate[i].Unscaled) {
                    continue;
                }
                EveryUpdate[i].OnSystemUpdate(EveryUpdate[i].Unscaled ? TimeManager.DeltaUnscaled : TimeManager.DeltaTime);
            }
        }

        public static void FixedSystemUpdate(float delta) {
            if (Game.Paused) {
                return;
            }
            for (int i = 0; i < FixedUpdate.Count; i++) {
                FixedUpdate[i].OnFixedSystemUpdate(delta);
            }
        }
    }

}