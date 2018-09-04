using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class DisableAfterPause : MonoBehaviour {

        [SerializeField] private float _pause = 1f;

        void Awake () {

        }

        void Start() {
            TimeManager.PauseFor(_pause, true, () => {
                gameObject.SetActive(false);
            });
        }
    }
}
