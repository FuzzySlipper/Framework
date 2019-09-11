using UnityEngine;
using UnityEngine.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public class ActionPrefab {
        public ActionStateEvents Event;
        public GameObject Prefab;
        public AudioClip Sound;
    }

    [System.Serializable]
    public class ActionFxData {
        public ActionStateEvents Event;
        public AudioClip Sound;
        public SpriteParticle Particle;
        public bool Parent = false;
    }

    [System.Serializable]
    public struct SpriteParticle {
        public SpriteAnimation Animation;
        public Color Color;
        [Range(0,5)] public float Glow;
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
