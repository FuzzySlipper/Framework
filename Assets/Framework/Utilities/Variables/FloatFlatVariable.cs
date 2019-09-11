using UnityEngine;

namespace PixelComrades {
    public abstract class FloatVariable : ScriptableObject {
#if UNITY_EDITOR
        [Multiline] public string DeveloperDescription = "";
#endif
        public abstract float Value { get; }
    }

    [CreateAssetMenu(menuName = "Assets/FloatFlat")]
    public class FloatFlatVariable : FloatVariable {

        [SerializeField] private float _value = 0;

        public override float Value { get { return _value; } }
    }
}