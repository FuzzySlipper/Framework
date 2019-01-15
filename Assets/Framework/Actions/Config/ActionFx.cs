using UnityEngine;

namespace PixelComrades {
    [CreateAssetMenu(menuName =  "Assets/ActionFx")]
    public class ActionFx : ScriptableObject {

        [SerializeField] private ActionPrefab[] _actionPrefabs = new ActionPrefab[1];

        public ActionPrefab[] ActionPrefabs { get { return _actionPrefabs; } }

        public void TriggerEvent(ActionStateEvent actionEvent) {
            for (int i = 0; i < _actionPrefabs.Length; i++) {
                if (_actionPrefabs[i].Event == actionEvent.State) {
                    if (_actionPrefabs[i].Prefab != null) {
                        var spawn = ItemPool.SpawnScenePrefab(_actionPrefabs[i].Prefab, actionEvent.Position, actionEvent.Rotation);
                        CheckObjectForListener(spawn, actionEvent);
                    }
                    if (_actionPrefabs[i].Sound != null) {
                        AudioPool.PlayClip(_actionPrefabs[i].Sound, actionEvent.Position, 0.5f);
                    }
                }
            }
        }

        private void CheckObjectForListener(GameObject newObject, ActionStateEvent stateEvent) {
            var actionListener = newObject.GetComponent<IActionPrefab>();
            if (actionListener != null) {
                actionListener.OnActionSpawn(stateEvent);
            }
        }
    }
}