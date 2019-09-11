using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class WorldControlRedirect : MonoBehaviour, IWorldControl {

        [SerializeField] private GameObject _target = null;

        private IWorldControl _targetInterface;
        public bool WorldControlActive { get { return _targetInterface != null && _targetInterface.WorldControlActive; } }
        void Awake() {
            _targetInterface = _target.GetComponent<IWorldControl>();
        }

        public void OnControlUse() {
            _targetInterface.OnControlUse();
        }
        public string OnControlHovered(bool status) {
            return _targetInterface.OnControlHovered(status);
        }
       
    }
}
