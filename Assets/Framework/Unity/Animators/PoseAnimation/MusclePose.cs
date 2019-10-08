using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class MusclePose : ScriptableObject {

        public List<SavedMuscle> Pose = new List<SavedMuscle>();

        public int Count { get { return Pose.Count; } }

        public void SetPose(PoseAnimator animator) {
            animator.UpdatePose();
            for (int i = 0; i < Pose.Count; i++) {
                animator.HumanPose.muscles[Pose[i].MuscleIndex] = Pose[i].Value;
            }
            animator.RefreshPose();

        }
        
        [Button]
        public void CopyMainModified() {
            var controller = MuscleController.Main;
            if (controller == null) {
                return;
            }
            Pose.Clear();
            foreach (var modifiedMuscle in controller.ModifiedDictionary) {
                Pose.Add(new SavedMuscle(modifiedMuscle.Key, modifiedMuscle.Value));
            }
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }

        [Button]
        public void RestoreToMain() {
            MuscleController.Main.UpdateBody();
            for (int i = 0; i < Pose.Count; i++) {
                MuscleController.Main.SetChanged(Pose[i].MuscleIndex,Pose[i].Value);
            }
            MuscleController.Main.SetPose();
        }

        [Button]
        public void CopyHelperToMain() {
            PoseAnimationHelper.PoseAnimator.UpdatePose();
            PoseAnimator.Main.UpdatePose();
            for (int i = 0; i < PoseAnimationHelper.PoseAnimator.HumanPose.muscles.Length; i++) {
                PoseAnimator.Main.HumanPose.muscles[i] = PoseAnimationHelper.PoseAnimator.HumanPose.muscles[i];
            }
            PoseAnimator.Main.RefreshPose();
        }

        public SavedMuscle GetMuscle(int muscleIndex) {
            for (int s = 0; s < Pose.Count; s++) {
                if (Pose[s].MuscleIndex == muscleIndex) {
                    return Pose[s];
                }
            }
            return null;
        }

        public bool HasPose(int muscleIndex) {
            for (int s = 0; s < Pose.Count; s++) {
                if (Pose[s].MuscleIndex == muscleIndex) {
                    return true;
                }
            }
            return false;
        }

        private void UpdateMuscleIndices(int[] indices, PoseAnimator animator) {
            animator.UpdatePose();
            for (int m = 0; m < indices.Length; m++) {
                var muscleIndex = indices[m];
                var value = animator.HumanPose.muscles[muscleIndex];
                //animator.UpdateMuscle(muscleIndex, value);
                var muscle = GetMuscle(muscleIndex);
                if (muscle == null) {
                    muscle = new SavedMuscle(muscleIndex, value);
                    Pose.Add(muscle);
                }
                else {
                    muscle.Value = value;
                }
            }
        }
    }
}
