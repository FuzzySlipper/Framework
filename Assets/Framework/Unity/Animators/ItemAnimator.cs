using UnityEngine;
using System.Collections;


namespace PixelComrades {
    public class ItemAnimator : MonoBehaviour {

        [SerializeField] private Animator _animator = null;

        public void PlayChargeAnimation() {
            _animator.SetTrigger("Charge");
        }

        public void PlayActivateAnimation() {
            _animator.SetTrigger("Activate");
        }

    }
}
