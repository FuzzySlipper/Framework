using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class PoseAnimatorComponent : IComponent {

        private Avatar _avatar;
        
        private CachedTransform _cachedTr = new CachedTransform();
        private HumanPose _pose;
        private HumanPoseHandler _hph = null;
        
        public HumanPose HumanPose { get => _pose; }
        public MusclePose DefaultPose { get; }

        public PoseAnimatorComponent(Avatar avatar, MusclePose defaultPose, Transform tr) {
            _avatar = avatar;
            _cachedTr.Set(tr);
            DefaultPose = defaultPose;
        }

        public PoseAnimatorComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }

        public void Init() {
            _hph = new HumanPoseHandler(_avatar, _cachedTr.Tr);
            _pose = new HumanPose();
            _hph.GetHumanPose(ref _pose);
            for (int i = 0; i < _pose.muscles.Length; i++) {
                _pose.muscles[i] = 0;
            }
            ResetPose();
        }

        public void RefreshPose() {
            _pose.bodyPosition = Vector3.zero;
            _pose.bodyRotation = Quaternion.identity;
            _hph.SetHumanPose(ref _pose);
        }

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

        public void SetPose(MusclePose pose) {
            UpdatePose();
            for (int i = 0; i < pose.Pose.Count; i++) {
                var muscle = pose.Pose[i];
                _pose.muscles[muscle.MuscleIndex] = muscle.Value;
            }
            RefreshPose();
        }
    }
}
