using UnityEngine;
using UnityEngine.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public class ActionPrefab {
        public ActionState Event;
        public GameObject Prefab;
        public AudioClip Sound;
    }

    [System.Serializable]
    public class ActionFxData {
        public ActionState Event;
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
        public ActionState Event;
        public AudioClip Sound;
    }

    public interface IActionPrefab {
        void OnActionSpawn(ActionEvent state);
    }
}
