using UnityEngine;
namespace PixelComrades {
    [System.Serializable]
    public class ActionPrefab {
        public ActionStateEvents Event;
        public GameObject Prefab;
        public AudioClip Sound;
    }

    [System.Serializable]
    public class ActionSound {
        public ActionStateEvents Event;
        public AudioClip Sound;
    }

    public interface IActionPrefab {
        void OnActionSpawn(ActionStateEvent state);
    }
}
