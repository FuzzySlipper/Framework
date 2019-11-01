using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RootMotion.FinalIK;
using Sirenix.OdinInspector;

namespace PixelComrades {
    [ExecuteInEditMode]
    public class AnimationIkPoser : MonoBehaviour {
        
        [SerializeField] private Transform _rightHandTarget = null;
        [SerializeField] private Transform _leftHandTarget = null;
        [SerializeField] private Transform _rightShoulderTarget = null;
        [SerializeField] private Transform _leftShoulderTarget = null;

        [SerializeField] private string _currentLabel = "";
        [SerializeField, Range(-2, 2)] private float _rightHandOpen = 1;
        [SerializeField, Range(-2, 2)] private float _leftHandOpen = 1;

        [SerializeField] private IkPoses _db = null;
        [SerializeField] private PoseAnimator _poseAnimator = null;
        [SerializeField] private FullBodyBipedIK _ik = null;

        [SerializeField] private bool _autoUpdate = true;
        
        private float _lastRightHand = -2;
        private float _lastLeftHand = -2;
        
        
        public PoseAnimator PoseAnimator { get => _poseAnimator; }
        public Transform RightHandTarget { get => _rightHandTarget; set => _rightHandTarget = value; }
        public Transform LeftHandTarget { get => _leftHandTarget; set => _leftHandTarget = value; }
        public Transform RightShoulderTarget { get => _rightShoulderTarget; set => _rightShoulderTarget = value; }
        public Transform LeftShoulderTarget { get => _leftShoulderTarget; set => _leftShoulderTarget = value; }
        public string CurrentLabel { get => _currentLabel; set => _currentLabel = value; }
        public bool AutoUpdate { get => _autoUpdate; set => _autoUpdate = value; }

        void Update() {
            if (!_autoUpdate) {
                return;
            }
            if (_poseAnimator != null) {
                _poseAnimator.UpdatePose();
                SetHand(true);
                SetHand(false);
                _poseAnimator.RefreshPose();
            }
            if (_ik != null) {
                _ik.UpdateSolverExternal();
            }
//            if (_poseAnimator != null) {
//                _poseAnimator.RefreshPose();
//            }
        }

        public void UpdateIk() {
            if (_ik != null) {
                _ik.UpdateSolverExternal();
            }
        }

        public void SetHand(bool isRight) {
            var sourceValue = (isRight ? _rightHandOpen : _leftHandOpen);
            var lastValue = isRight ? _lastRightHand : _lastLeftHand;
            if (Mathf.Abs(sourceValue - lastValue) > 0.001f) {
                UpdateHandIndices(isRight, sourceValue);
                if (isRight) {
                    _lastRightHand = sourceValue;
                }
                else {
                    _lastLeftHand = sourceValue;
                }
            }
        }

        public void SetHands(float value) {
            _lastLeftHand = _lastRightHand = _rightHandOpen = _leftHandOpen = value;
            UpdateHandIndices(true, value);
            UpdateHandIndices(false, value);
        }

        private void UpdateHandIndices(bool isRight, float value) {
            var indices = isRight ? HumanPoseExtensions.RightHandMuscles : HumanPoseExtensions.LeftHandMuscles;
            for (int i = 0; i < indices.Length; i++) {
                _poseAnimator.HumanPose.muscles[indices[i]] = value;
            }
        }

        [Button]
        public void StorePose() {
            if (_db == null) {
                return;
            }
            _db.AddPose(new SavedIkPose() {
                Label = _currentLabel,
                RightHandPos = _rightHandTarget.localPosition,
                RightHandRot = _rightHandTarget.localRotation,
                RightShoulderPos = _rightShoulderTarget.localPosition,
                RightShoulderRot = _rightShoulderTarget.localRotation,
                LeftHandPos = _leftHandTarget.localPosition,
                LeftHandRot = _leftHandTarget.localRotation,
                LeftShoulderPos = _leftShoulderTarget.localPosition,
                LeftShoulderRot = _leftShoulderTarget.localRotation,
                RightHandOpenClose = _rightHandOpen,
                LeftHandOpenClose = _leftHandOpen
            });
        }

        [Button]
        public void RestoreCurrent() {
            if (_db == null) {
                return;
            }
            var pose = _db.GetPose(_currentLabel);
            if (pose != null) {
                Restore(pose);
            }
        }

        public void Restore(SavedIkPose ikPose) {
            _rightHandTarget.localPosition = ikPose.RightHandPos;
            _rightHandTarget.localRotation = ikPose.RightHandRot;
            _rightShoulderTarget.localPosition = ikPose.RightShoulderPos;
            _rightShoulderTarget.localRotation = ikPose.RightShoulderRot;
            _leftHandTarget.localPosition = ikPose.LeftHandPos;
            _leftHandTarget.localRotation = ikPose.LeftHandRot;
            _leftShoulderTarget.localPosition = ikPose.LeftShoulderPos;
            _leftShoulderTarget.localRotation = ikPose.LeftShoulderRot;
            _leftHandOpen = ikPose.LeftHandOpenClose;
            _rightHandOpen = ikPose.RightHandOpenClose;
            _currentLabel = ikPose.Label;
            SetHand(true);
            SetHand(false);
        }
    }

    [System.Serializable]
    public sealed class AnimationIkPoserComponent : IComponent {

        public AnimationIkPoser Poser;

        public AnimationIkPoserComponent(AnimationIkPoser poser) {
            Poser = poser;
        }

        public AnimationIkPoserComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
