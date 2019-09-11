using UnityEngine;
using System.Collections;
namespace PixelComrades {
    public class ActionMoveConfig : ScriptableObject {

        [SerializeField] private float _length = 1f;
        [SerializeField] private EasingTypes _easing = EasingTypes.SinusoidalInOut;
        [SerializeField] private bool _rotateOnMove = false;
        [SerializeField] private EasingTypes _rotationEasing = EasingTypes.SinusoidalOut;
        [SerializeField] private float _rotationLength = 0.3f;

        public float Length { get { return _length; } }
        public EasingTypes Easing { get { return _easing; } }
        public bool RotateOnMove { get { return _rotateOnMove; } }
        public float RotationLength { get { return _rotationLength; } }
        public EasingTypes RotationEasing { get { return _rotationEasing; } }
    }
}