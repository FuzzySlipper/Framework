using UnityEngine;

namespace PixelComrades {
    [CreateAssetMenu(menuName = "Assets/IntVariable")]
    public class IntVariable : ScriptableObject {

#if UNITY_EDITOR
        [Multiline] public string DeveloperDescription = "";
#endif
        [SerializeField] private int _value = 0;

        public int Value { get { return _value; } }
    }
}