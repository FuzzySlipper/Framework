using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class WeaponModel : MonoBehaviour, IWeaponModel {

        [SerializeField] private Transform _spawn = null;
        [SerializeField] private Transform _base = null;
        [SerializeField] private MeleeWeaponTrail _weaponTrail = null;
        [SerializeField] private TransformState _basePositioning = new TransformState();
        [SerializeField] private HandPose _handPose = null;
        [SerializeField] private MusclePose _idlePose = null;

        public Transform Tr { get { return _base; } }
        public Transform Spawn { get { return _spawn; } }
        public MusclePose IdlePose { get { return _idlePose; } }
        
        public void SetFx(bool status) {
            if (_weaponTrail != null) {
                //_weaponTrail.gameObject.SetActive(status);
                _weaponTrail.SetActive(status);
            }
        }

        [Button]
        public void SetPosition() {
            _basePositioning.Set(_base);
        }

        [Button]
        public void Setup() {
            _basePositioning.Restore(_base);
            _handPose.RestoreMain();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            if (_spawn != null) {
                DebugExtension.DrawArrow(_spawn.position, _spawn.forward * 0.2f, Color.red);
            }
        }
#endif
    }
}
