using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class HandPose : ScriptableObject {

        public List<SavedMuscle> Pose = new List<SavedMuscle>();
        public void SetPose(PoseAnimator animator) {
            animator.UpdatePose();
            for (int i = 0; i < Pose.Count; i++) {
                animator.HumanPose.muscles[Pose[i].MuscleIndex] = Pose[i].Value;
            }
            animator.RefreshPose();
        }
        
        [Button]
        public void CopyMainRightHand() {
            UpdateMuscleIndices(HumanPoseExtensions.RightHandMuscles, PoseAnimator.Main);
        }

        [Button]
        public void CopyMainLeftHand() {
            UpdateMuscleIndices(HumanPoseExtensions.LeftHandMuscles, PoseAnimator.Main);
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
        public void Clear() {
            Pose.Clear();
        }

        [Button]
        public void RestoreMain() {
            SetPose(PoseAnimator.Main);
        }

        private void UpdateMuscleIndices(int[] indices, PoseAnimator animator) {
            animator.UpdatePose();
            for (int m = 0; m < indices.Length; m++) {
                var muscleIndex = indices[m];
                var value = animator.HumanPose.muscles[muscleIndex];
                //animator.UpdateMuscle(muscleIndex, value);
                bool foundIndex = false;
                for (int s = 0; s < Pose.Count; s++) {
                    if (Pose[s].MuscleIndex == muscleIndex) {
                        Pose[s].Value = value;
                        foundIndex = true;
                        break;
                    }
                }
                if (!foundIndex) {
                    Pose.Add(new SavedMuscle(muscleIndex, value));
                }
            }
        }
    }
}
