using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace PixelComrades {
    
    [CustomEditor(typeof(WeaponPositionMarker))]
    public class WeaponPositionMarkerEditor : OdinEditor {}

    [CustomEditor(typeof(AnimationEventMarker))]
    public class AnimationEventMarkerEditor : OdinEditor {
    }

    [CustomEditor(typeof(MuscleController))]
    public class MuscleControllerEditor : OdinEditor {

        private MuscleController _script;

        private static FoldOut[] _foldOuts = new[] {
            new FoldOut("Spine", new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 47, 38 }),
            new FoldOut("Head", new[] {9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20}),
            new FoldOut("Legs", new[] {21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36})
        };

        private const float ChangeTolerance = 0.001f;
        private Header _leftArm;
        private Header _rightArm;
        private static FoldOut _leftHandDetail = null;
        private static FoldOut _rightHandDetail = null;
        private static string[] _muscleNames;
        private static int _min = -2;
        private static int _max = 2;
        private static int[] _frontBackLegs = new[] {21, 29};
        private static int[] _lowerLegStretch = new[] {24, 32};
        public override void OnInspectorGUI() {
            if (_leftHandDetail == null || _rightHandDetail == null || _leftArm == null ||
                _rightArm == null) {
                _leftHandDetail = new FoldOut("Left Hand", HumanPoseExtensions.LeftHandMuscles);
                _rightHandDetail = new FoldOut("Right Hand", HumanPoseExtensions.RightHandMuscles);
                _leftArm = new Header("Left Arm", HumanPoseExtensions.LeftArmMuscles);
                _rightArm = new Header("Right Arm", HumanPoseExtensions.RightArmMuscles);
            }
            _script = (MuscleController) target;
            if (_script.HumanPoseHandler == null) {
                _script.Init();
            }
            if (_script.HumanPoseHandler == null) {
                base.OnInspectorGUI();
                return;
            }
            EditorGUILayout.LabelField("Modified Count : " + _script.ModifiedDictionary.Count);
            //_script.UpdateBody();
            _muscleNames = HumanTrait.MuscleName;

            DisplayList(_rightArm.Label, _rightArm.Indices);
            DisplayHand(true);
            DisplayFoldOut(_rightHandDetail);

            DisplayList(_leftArm.Label, _leftArm.Indices);
            DisplayHand(false);
            DisplayFoldOut(_leftHandDetail);
            
            for (int i = 0; i < _foldOuts.Length; i++) {
                DisplayFoldOut(_foldOuts[i]);
            }
            if (GUILayout.Button("Reset Modified")) {
                _script.ResetModified();
            }
            if (GUILayout.Button("Reset Pose")) {
                for (int i = 0; i < _script.HumanPose.muscles.Length; i++) {
                    _script.HumanPose.muscles[i] = 0;
                }
                for (int i = 0; i < _frontBackLegs.Length; i++) {
                    _script.HumanPose.muscles[_frontBackLegs[i]] = 0.62f;
                    _script.HumanPose.muscles[_lowerLegStretch[i]] = 1f;
                }
                _script.SetPose();
                _script.ResetModified();
            }
            SceneView.RepaintAll();
            base.OnInspectorGUI();
        }

        public void DisplayHand(bool isRight) {
            EditorGUILayout.BeginHorizontal();
            var sourceValue = (isRight ? _script.RightHand : _script.LeftHand);
            var indices = isRight ? _rightHandDetail.Indices : _leftHandDetail.Indices;
            var value = EditorGUILayout.Slider(isRight ? "Right Hand" : "Left Hand", sourceValue, _min, _max);
            if (GUILayout.Button("Copy") && PoseAnimationHelper.PoseAnimator != null) {
                PoseAnimationHelper.PoseAnimator.UpdatePose();
                value = 0;
                for (int i = 0; i < indices.Length; i++) {
                    var idx = indices[i];
                    var helperValue = PoseAnimationHelper.PoseAnimator.HumanPose.muscles[idx];
                    value += helperValue;
                    _script.SetChanged(idx, helperValue);
                }
                value /= indices.Length;
            }
            if (Math.Abs(sourceValue - value) > 0.001f) {
                UpdateHand(value, isRight);
                UpdateList(value, indices);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void UpdateHand(float value, bool isRight) {
            if (isRight) {
                _script.RightHand = value;
            }
            else {
                _script.LeftHand = value;
            }
        }

        private void DisplayFoldOut(FoldOut foldOut) {
            foldOut.Active = EditorGUILayout.Foldout(foldOut.Active, foldOut.Label);
            if (foldOut.Active) {
                for (int i = 0; i < foldOut.Indices.Length; i++) {
                    DisplayMuscle(foldOut.Indices[i]);
                }
            }
        }

        private void DisplayList(string header, int[] indices) {
            EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
            for (int i = 0; i < indices.Length; i++) {
                DisplayMuscle(indices[i]);
            }
        }

        private void UpdateList(float value, int[] indices) {
            for (int i = 0; i < indices.Length; i++) {
                //_script.HumanPose.muscles[indices[i]] = value;
                _script.SetChanged(indices[i], value);
            }
        }

        private const float Increment = 0.01f;

        private void DisplayMuscle(int i) {
            //var min = HumanTrait.GetMuscleDefaultMin(i);
            //var max = HumanTrait.GetMuscleDefaultMax(i);
            EditorGUILayout.BeginHorizontal();
            var val = EditorGUILayout.Slider(_muscleNames[i], _script.HumanPose.muscles[i], _min, _max);
            if (GUILayout.Button("-")) {
                val -= Increment;
            }
            if (GUILayout.Button("+")) {
                val += Increment;
            }
            if (GUILayout.Button("Copy") && PoseAnimationHelper.PoseAnimator != null) {
                PoseAnimationHelper.PoseAnimator.UpdatePose();
                val = PoseAnimationHelper.PoseAnimator.HumanPose.muscles[i];
            }
            if (Math.Abs(val - _script.HumanPose.muscles[i]) > ChangeTolerance) {
                _script.SetChanged(i, val);
            }
            EditorGUILayout.EndHorizontal();
        }

        private class FoldOut {
            public string Label;
            public bool Active;
            public int[] Indices;

            public FoldOut(string label, int[] indices) {
                Label = label;
                Active = false;
                Indices = indices;
            }
        }

        private class Header {
            public string Label;
            public int[] Indices;

            public Header(string label, int[] indices) {
                Label = label;
                Indices = indices;
            }
        }
    }

}
