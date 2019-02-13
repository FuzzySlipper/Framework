using UnityEngine;

namespace PixelComrades {

    [System.Serializable]
    public class GameDataRequired {
        public StringVariable Variable;
        public int Required = 1;

        public bool HasData() {
            if (Variable == null) {
                return true;
            }
            return Game.GetDataInt(Variable) >= Required;
        }
    }

    [CreateAssetMenu(menuName = "Assets/StringVariable")]
    public class StringVariable : ScriptableObject {

#if UNITY_EDITOR
        [Multiline] public string DeveloperDescription = "";
#endif
        public string Value { get { return name; } }

        public static implicit operator string(StringVariable reference) {
            return reference.Value;
        }
    }
}