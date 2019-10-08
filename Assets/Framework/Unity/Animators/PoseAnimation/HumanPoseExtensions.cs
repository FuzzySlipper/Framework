using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    [System.Serializable]
    public class SavedMuscle {
        public int MuscleIndex;
        public float Value;

        public SavedMuscle() {
        }

        public SavedMuscle(int muscleIndex, float value) {
            MuscleIndex = muscleIndex;
            Value = value;
        }
    }

    [System.Serializable]
    public class SavedMuscleInstance {
        public int MuscleIndex;
        public float Target;
        public float Start;

        public SavedMuscleInstance() {}

        public SavedMuscleInstance(SavedMuscle muscle) {
            MuscleIndex = muscle.MuscleIndex;
            Target = muscle.Value;
        }

        public SavedMuscleInstance(SavedMuscle muscle, float start) {
            MuscleIndex = muscle.MuscleIndex;
            Target = muscle.Value;
            Start = start;
        }

        public void Set(SavedMuscle muscle, float start) {
            MuscleIndex = muscle.MuscleIndex;
            Target = muscle.Value;
            Start = start;
        }
    }

    [System.Serializable]
    public class PositionFrame {
        public int FrameIndex;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    [System.Serializable]
    public class LerpFrame {
        public int FrameIndex;
        public Vector3 Position;
        public Quaternion Rotation;
        public float Time;
    }

    public static class HumanPoseExtensions {
        public static int[] LeftHandMuscles = new[] {
            55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74
        };
        public static int[] RightHandMuscles = new[] {
            75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94
        };
        public static int[] RightArmMuscles = new[] {
            46, 48, 49, 50, 51, 52, 53, 54
        };
        public static int[] LeftArmMuscles = new[] {
            37, 39, 40, 41, 42, 43, 44, 45
        };
        public static string[] RightFingerTransforms = new string[] {
            "index_01_r", "index_02_r", "index_03_r",
            "middle_01_r", "middle_02_r", "middle_03_r",
            "pinky_01_r", "pinky_02_r", "pinky_03_r",
            "ring_01_r", "ring_02_r", "ring_03_r",
            "thumb_01_r", "thumb_02_r", "thumb_03_r",
        };
        public static string[] LeftFingerTransforms = new string[] {
            "index_01_l", "index_02_l", "index_03_l",
            "middle_01_l", "middle_02_l", "middle_03_l",
            "pinky_01_l", "pinky_02_l", "pinky_03_l",
            "ring_01_l", "ring_02_l", "ring_03_l",
            "thumb_01_l", "thumb_02_l", "thumb_03_l",
        };
        public const string RightHandTransform = "hand_r";
        public const string LeftHandTransform = "hand_l";
    }
}
