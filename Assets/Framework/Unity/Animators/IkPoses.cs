using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    [CreateAssetMenu]
    public sealed class IkPoses : ScriptableObject {
        
        [SerializeField] private List<SavedIkPose> _poses = new List<SavedIkPose>();

        public SavedIkPose GetPose(string label) {
            for (int i = 0; i < _poses.Count; i++) {
                if (_poses[i].Label.CompareCaseInsensitive(label)) {
                    return _poses[i];
                }
            }
            return null;
        }

        public ValueDropdownList<string> SignalsList() {
            var signals = new ValueDropdownList<string>();
            signals.Add("");
            for (int i = 0; i < _poses.Count; i++) {
                signals.Add(_poses[i].Label);
            }
            return signals;
        }

        public List<string> StringSignalsList() {
            var signals = new List<string>();
            signals.Add("");
            for (int i = 0; i < _poses.Count; i++) {
                signals.Add(_poses[i].Label);
            }
            return signals;
        }

        public void AddPose(SavedIkPose ikPose) {
            for (int i = 0; i < _poses.Count; i++) {
                if (_poses[i].Label.CompareCaseInsensitive(ikPose.Label)) {
                    _poses[i] = ikPose;
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(this);
#endif
                    return;
                }
            }
            _poses.Add(ikPose);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        
    }

    [System.Serializable]
    public class SavedIkPose {
        public Vector3 RightHandPos;
        public Vector3 LeftHandPos;
        public Vector3 RightShoulderPos;
        public Vector3 LeftShoulderPos;
        public Quaternion RightHandRot;
        public Quaternion LeftHandRot;
        public Quaternion RightShoulderRot;
        public Quaternion LeftShoulderRot;
        public float RightHandOpenClose;
        public float LeftHandOpenClose;
        public string Label;
    }
}
