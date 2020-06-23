using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {

    [ExecuteInEditMode]
    public class MuscleController : MonoBehaviour {

        [SerializeField] private Avatar _avatar = null;
        [SerializeField] private bool _disabled = false;
        [SerializeField] private bool _isMain = false;

        public float LeftHand;
        public float RightHand;
        private Dictionary<int, float> _modifiedDictionary = new Dictionary<int, float>();

        private HumanPose _pose;
        private HumanPoseHandler _hph = null;
        public HumanPoseHandler HumanPoseHandler { get => _hph; }
        public HumanPose HumanPose { get => _pose; }
        public Dictionary<int, float> ModifiedDictionary { get => _modifiedDictionary; }
        public static MuscleController Main { get; private set; }


        void OnEnable() {
            if (_isMain) {
                Main = this;
            }
        }

        public void Init() {
            if (_avatar == null) {
                return;
            }
            _hph = new HumanPoseHandler(_avatar, transform);
            _pose = new HumanPose();
            _hph.GetHumanPose(ref _pose);
            for (int i = 0; i < _pose.muscles.Length; i++) {
                _pose.muscles[i] = 0;
            }
        }

        [Button]
        public void UpdateBody() {
            _hph.GetHumanPose(ref _pose);
            _pose.bodyPosition = Vector3.zero;
            _pose.bodyRotation = Quaternion.identity;
            _hph.SetHumanPose(ref _pose);
        }

        [Button]
        public void SetMain() {
            Main = this;
        }

        public void ResetModified() {
            _modifiedDictionary.Clear();
        }

        public void SetPose() {
            if (_hph == null) {
                Init();
            }
            _hph.SetHumanPose(ref _pose);
        }

        public void SetChanged(int index, float value) {
            _pose.muscles[index] = value;
            if (_modifiedDictionary.ContainsKey(index)) {
                _modifiedDictionary[index] = value;
            }
            else {
                _modifiedDictionary.Add(index, value);
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos() {
            if (Application.isPlaying || _disabled) {
                return;
            }
            if (_hph == null) {
                Init();
            }
            if (_hph == null) {
                return;
            }
            _pose.bodyPosition = Vector3.zero;
            _pose.bodyRotation = Quaternion.identity;
            _hph.SetHumanPose(ref _pose);
        }
#endif
    }
}
