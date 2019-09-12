using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class FirstPersonAnimatorComponent : IComponent {
        private CachedUnityComponent<PlayerWeaponAnimator> _component;
        public PlayerWeaponAnimator Animator { get { return _component.Component; } }

        public FirstPersonAnimatorComponent(PlayerWeaponAnimator animator) {
            _component = new CachedUnityComponent<PlayerWeaponAnimator>(animator);
        }

        public FirstPersonAnimatorComponent(SerializationInfo info, StreamingContext context) {
            _component = info.GetValue(nameof(_component), _component);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_component), _component);
        }
    }
}
