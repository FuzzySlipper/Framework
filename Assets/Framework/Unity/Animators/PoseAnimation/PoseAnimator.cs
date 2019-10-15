using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;

namespace PixelComrades {
    [ExecuteInEditMode]
    public class PoseAnimator : EntityIdentifier {

        [SerializeField] private Avatar _avatar = null;
        [SerializeField] private bool _isMain = true;
        [SerializeField] private Transform _primaryPivot = null;
        [SerializeField] private Transform _secondaryPivot = null;
        [SerializeField] private MusclePose _defaultPose = null;
        
        private HumanPose _pose;
        private HumanPoseHandler _hph = null;
        public Transform PrimaryPivot { get => _primaryPivot; }
        public Transform SecondaryPivot { get => _secondaryPivot; }
        public HumanPose HumanPose { get => _pose; }
        public Avatar Avatar { get => _avatar; }
        public MusclePose DefaultPose { get => _defaultPose; }
        
        private static PoseAnimator _main;
        public static PoseAnimator Main {
            get {
                if (_main == null) {
                    var animators = GameObject.FindObjectsOfType<PoseAnimator>();
                    for (int i = 0; i < animators.Length; i++) {
                        if (animators[i]._isMain) {
                            _main = animators[i];
                            break;
                        }
                    }
                }
                return _main;
            }
        }

        void Awake() {
            if (_isMain) {
                _main = this;
            }
            Init();
        }

        public void Init() {
            _hph = new HumanPoseHandler(_avatar, transform);
            _pose = new HumanPose();
            _hph.GetHumanPose(ref _pose);
            for (int i = 0; i < _pose.muscles.Length; i++) {
                _pose.muscles[i] = 0;
            }
            ResetPose();
            SetPose(_defaultPose);
        }

        [Button]
        public void ResetPose() {
            for (int i = 0; i < HumanPose.muscles.Length; i++) {
                HumanPose.muscles[i] = 0;
            }
        }

        public void UpdatePose() {
            if (_hph == null) {
                Init();
            }
            _hph.GetHumanPose(ref _pose);
        }

        public void RefreshPose() {
            _pose.bodyPosition = Vector3.zero;
            _pose.bodyRotation = Quaternion.identity;
            _hph.SetHumanPose(ref _pose);
        }

        private void SetPose(MusclePose pose) {
            UpdatePose();
            for (int i = 0; i < pose.Pose.Count; i++) {
                var muscle = pose.Pose[i];
                _pose.muscles[muscle.MuscleIndex] = muscle.Value;
            }
            RefreshPose();
        }

#if UNITY_EDITOR
        void OnDrawGizmos() {
            if (_primaryPivot == null || _secondaryPivot == null) {
                return;
            }
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.ConeHandleCap(999, PrimaryPivot.position, PrimaryPivot.rotation, 0.025f, EventType.Repaint);
            UnityEditor.Handles.ConeHandleCap(999, SecondaryPivot.position, SecondaryPivot.rotation, 0.025f, EventType.Repaint);
        }
#endif
    }
}
