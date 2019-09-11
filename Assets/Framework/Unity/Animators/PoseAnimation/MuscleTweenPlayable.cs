using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Playables;

namespace PixelComrades {
    [System.Serializable]
    public class MuscleTweenBehaviour: PlayableBehaviour {

        public EasingTypes EaseType = EasingTypes.Linear;
        public List<SavedMuscleInstance> Pose = new List<SavedMuscleInstance>();
        public PoseAnimator Animator;
        public bool UpdateStart;

        private bool _started = false;
        private float _startTime;
        private float _duration;
        private Func<float, float, float, float> _easeFunc;

        public override void OnGraphStart(Playable playable) {
            base.OnGraphStart(playable);
            _started = false;
        }

        public override void OnGraphStop(Playable playable) {
            base.OnGraphStop(playable);
            _started = false;
        }

        public override void OnPlayableCreate(Playable playable) {
            base.OnPlayableCreate(playable);
            _started = false;
        }

        public override void PrepareFrame(Playable playable, FrameData info) {
            base.PrepareFrame(playable, info);
            if (!_started) {
                Start(playable, info);
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
            base.ProcessFrame(playable, info, playerData);
            if (!_started) {
                Setup(playable);
            }
            var percent =  _easeFunc(0, 1,(TimeManager.Time - _startTime) / _duration);
            for (int i = 0; i < Pose.Count; i++) {
                var muscle = Pose[i];
                Animator.HumanPose.muscles[muscle.MuscleIndex] = Mathf.Lerp(muscle.Start, muscle.Target, percent);
                //Animator.UpdateMuscle(muscle.MuscleIndex, Mathf.Lerp(muscle.Start, muscle.Target, percent));
            }
            Animator.RefreshPose();
        }

        private void Setup(Playable playable) {
            _started = true;
            _duration = (float) playable.GetDuration();
            _easeFunc = Easing.Function(EaseType);
            _startTime = TimeManager.Time;
            Animator = PoseAnimator.Main;
            Animator.UpdatePose();
            for (int i = 0; i < Pose.Count; i++) {
                Pose[i].Start = Animator.HumanPose.muscles[Pose[i].MuscleIndex];
            }
        }

        private void Start(Playable playable, FrameData info) {
            //if (info.frameId == 0){ 
            //if ((info.frameId == 0) || (info.deltaTime > 0)) {
                Setup(playable);
           // }
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info) {
            base.OnBehaviourPlay(playable, info);
            Start(playable, info);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info) {
            base.OnBehaviourPause(playable, info);
            _started = false;
        }
    }

    public class MuscleTweenPlayable : PlayableAsset {

        public EasingTypes EaseType = EasingTypes.Linear;
        public List<SavedMuscle> TargetPose = new List<SavedMuscle>();
        public List<SavedMuscle> StartPose = new List<SavedMuscle>();
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
            var playable = ScriptPlayable<MuscleTweenBehaviour>.Create(graph);
            playable.SetDuration(0.25);
            var behavior = playable.GetBehaviour();
            behavior.EaseType = EaseType;
            behavior.UpdateStart = StartPose.Count == TargetPose.Count;
            for (int i = 0; i < TargetPose.Count; i++) {
                behavior.Pose.Add(new SavedMuscleInstance(TargetPose[i]));
                if (StartPose.HasIndex(i)) {
                    behavior.Pose[i].Start = FindStartValue(TargetPose[i].MuscleIndex);
                }
            }
            return playable;
        }

        public float FindStartValue(int muscleIndex) {
            for (int s = 0; s < StartPose.Count; s++) {
                if (StartPose[s].MuscleIndex == muscleIndex) {
                    return StartPose[s].Value;
                }
            }
            return 0;
        }
        
        [Button]
        public void CopyHelperRightArm() {
            UpdateMuscleIndices(HumanPoseExtensions.RightArmMuscles, PoseAnimationHelper.PoseAnimator);
        }

        [Button]
        public void CopyHelperLeftArm() {
            UpdateMuscleIndices(HumanPoseExtensions.LeftArmMuscles, PoseAnimationHelper.PoseAnimator);
        }

        [Button]
        public void CopyHelperRightHand() {
            UpdateMuscleIndices(HumanPoseExtensions.RightHandMuscles, PoseAnimationHelper.PoseAnimator);
        }

        [Button]
        public void CopyHelperLeftHand() {
            UpdateMuscleIndices(HumanPoseExtensions.LeftHandMuscles, PoseAnimationHelper.PoseAnimator);
        }

        [Button]
        public void CopyHelperRightArmStart() {
            UpdateMuscleIndices(HumanPoseExtensions.RightArmMuscles, PoseAnimationHelper.PoseAnimator, true);
        }

        [Button]
        public void CopyHelperLeftArmStart() {
            UpdateMuscleIndices(HumanPoseExtensions.LeftArmMuscles, PoseAnimationHelper.PoseAnimator, true);
        }

        [Button]
        public void CopyHelperRightHandStart() {
            UpdateMuscleIndices(HumanPoseExtensions.RightHandMuscles, PoseAnimationHelper.PoseAnimator, true);
        }

        [Button]
        public void CopyHelperLeftHandStart() {
            UpdateMuscleIndices(HumanPoseExtensions.LeftHandMuscles, PoseAnimationHelper.PoseAnimator, true);
        }

        [Button]
        public void Clear() {
            TargetPose.Clear();
        }

        private void UpdateMuscleIndices(int[] indices, PoseAnimator animator, bool isStart = false) {
            animator.UpdatePose();
            var poseList = isStart ? StartPose: TargetPose;
            for (int m = 0; m < indices.Length; m++) {
                var muscleIndex = indices[m];
                var value = animator.HumanPose.muscles[muscleIndex];
                //animator.UpdateMuscle(muscleIndex, value);
                bool foundIndex = false;
                for (int s = 0; s < poseList.Count; s++) {
                    if (poseList[s].MuscleIndex == muscleIndex) {
                        poseList[s].Value = value;
                        foundIndex = true;
                        break;
                    }
                }
                if (!foundIndex) {
                    poseList.Add(new SavedMuscle(muscleIndex, value));
                }
            }
        }
    }
}
