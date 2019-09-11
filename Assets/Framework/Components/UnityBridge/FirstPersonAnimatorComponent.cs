using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FirstPersonAnimatorComponent : IComponent {
        public int Owner { get; set; }

        public PlayerWeaponAnimator Animator { get; }

        public FirstPersonAnimatorComponent(PlayerWeaponAnimator animator) {
            Animator = animator;
        }
    }
}
