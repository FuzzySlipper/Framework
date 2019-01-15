using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace PixelComrades {
    public class ApplySelectedPrefabs {
        private delegate void ChangePrefab(GameObject go);
        private const int SelectionThresholdForProgressBar = 20;

        [MenuItem("GameObject/Apply Changes To Selected Prefabs", false, 100)]
        private static void ApplyPrefabs() {
            SearchPrefabConnections(ApplyToSelectedPrefabs);
        }

        [MenuItem("GameObject/Revert Changes Of Selected Prefabs", false, 100)]
        private static void ResetPrefabs() {
            SearchPrefabConnections(RevertToSelectedPrefabs);
        }

        [MenuItem("GameObject/Apply Changes To Selected Prefabs", true)]
        [MenuItem("GameObject/Revert Changes Of Selected Prefabs", true)]
        private static bool IsSceneObjectSelected() {
            return Selection.activeTransform != null;
        }

        //Look for connections
        private static void SearchPrefabConnections(ChangePrefab changePrefabAction) {
            GameObject[] selectedTransforms = Selection.gameObjects;
            int numberOfTransforms = selectedTransforms.Length;
            bool showProgressBar = numberOfTransforms >= SelectionThresholdForProgressBar;
            int changedObjectsCount = 0;
            //Iterate through all the selected gameobjects
            try {
                for (int i = 0; i < numberOfTransforms; i++) {
                    if (showProgressBar) {
                        EditorUtility.DisplayProgressBar("Update prefabs", "Updating prefabs (" + i + "/" + numberOfTransforms + ")",
                            (float)i / (float)numberOfTransforms);
                    }

                    var go = selectedTransforms[i];
                    var prefabType = PrefabUtility.GetPrefabType(go);
                    //Is the selected gameobject a prefab?
                    if (prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.DisconnectedPrefabInstance) {
                        var prefabRoot = PrefabUtility.FindRootGameObjectWithSameParentPrefab(go);
                        if (prefabRoot == null) {
                            continue;
                        }

                        changePrefabAction(prefabRoot);
                        changedObjectsCount++;
                    }
                }
            }
            finally {
                if (showProgressBar) {
                    EditorUtility.ClearProgressBar();
                }
                Debug.LogFormat("{0} Prefab(s) updated", changedObjectsCount);
            }
        }

        //Apply    
        private static void ApplyToSelectedPrefabs(GameObject go) {
            var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(go);
            if (prefabAsset == null) {
                return;
            }
            PrefabUtility.ReplacePrefab(go, prefabAsset, ReplacePrefabOptions.ConnectToPrefab);
        }

        //Revert
        private static void RevertToSelectedPrefabs(GameObject go) {
            PrefabUtility.ReconnectToLastPrefab(go);
            PrefabUtility.RevertPrefabInstance(go);
        }
    }
}
