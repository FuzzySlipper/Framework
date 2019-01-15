using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEditor;

namespace PixelComrades {
    [CustomEditor(typeof(SpriteAnimation), true)]
    public class SpriteAnimationEditor : Editor {

        private TextureDrawType _selectedTextureDrawType = TextureDrawType.Transparent;
        private Material _normalMat;
        private SpriteAnimationPreview _timeControl;

        void OnEnable() {
            _normalMat = new Material(Shader.Find("Sprites/Default"));
            _timeControl = new SpriteAnimationPreview((SpriteAnimation)target);
        }


        public override void OnInspectorGUI() {
            var script = (SpriteAnimation)target;
            if (GUILayout.Button("Edit Animations")) {
                SpriteAnimatorWindow.ShowWindow();
            }
            EditorGUILayout.LabelField(string.Format("Last Modified {0}", script.LastModified));
            EditorGUILayout.LabelField(string.Format("Length: {0}", script.LengthTime.ToString("F1")));
            EditorGUILayout.LabelField(string.Format("FrameTime: {0}", script.FrameTime.ToString("F1")));
            EditorGUILayout.LabelField(string.Format("Animation Length: {0}", script.LengthFrames));
            base.OnInspectorGUI();
        }

        public override void OnPreviewSettings() {
            var hasSprites = _timeControl != null && _timeControl.Animation.LengthFrames > 0;

            if (hasSprites) {
                DrawTextureDrawType();
                DrawPrevSpriteButton();
                DrawPlayButton();
                DrawNextSpriteButton();
                DrawSpeedSlider();

                if (!_timeControl.IsPlaying) return;

                foreach (var activeEditor in ActiveEditorTracker.sharedTracker.activeEditors) {
                    activeEditor.Repaint();
                }
            }
            else {
                base.OnPreviewSettings();
            }
        }

        public override bool HasPreviewGUI() {
            return true;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background) {
            if (r.width <= 1f && r.height <= 1f) {
                GUI.Box(r, "Tiny");
                return;
            }
            var hasSprites = _timeControl != null && _timeControl.Animation.LengthFrames > 0;
            if (hasSprites) {
                var texture = _timeControl.GetCurrentPreviewTexture(r);
                switch (_selectedTextureDrawType) {
                    case TextureDrawType.Normal:
                        EditorGUI.DrawPreviewTexture(r, texture, _normalMat, ScaleMode.ScaleToFit);
                        break;
                    case TextureDrawType.Alpha:
                        EditorGUI.DrawTextureAlpha(r, texture, ScaleMode.ScaleToFit);
                        break;
                    case TextureDrawType.Transparent:
                        EditorGUI.DrawTextureTransparent(r, texture, ScaleMode.ScaleToFit);
                        break;
                }
                GUI.Label(r, texture.name, Styles.grayLabel);
            }
            else {
                GUI.Box(r, "No Sprite Data");
                //base.OnPreviewGUI(r, background);
            }
        }


        private void DrawTextureDrawType() {
            _selectedTextureDrawType = (TextureDrawType)EditorGUILayout.EnumPopup(_selectedTextureDrawType, new GUIStyle("preDropDown"), GUILayout.Width(100));
        }

        private void DrawPlayButton() {

            var buttonContent = _timeControl.IsPlaying ? Contents.pauseButtonContent : Contents.playButtonContent;

            EditorGUI.BeginChangeCheck();

            var isPlaying = GUILayout.Toggle(_timeControl.IsPlaying, buttonContent, Styles.previewButtonSettingsStyle);

            if (EditorGUI.EndChangeCheck()) {
                if (isPlaying)
                    _timeControl.Play();
                else
                    _timeControl.Pause();
            }
        }
        private void DrawPrevSpriteButton() {
            if (GUILayout.Button(Contents.prevButtonContent, Styles.previewButtonSettingsStyle)) {
                _timeControl.Pause();
                _timeControl.PrevSprite();
            }
        }
        private void DrawNextSpriteButton() {
            if (GUILayout.Button(Contents.nextButtonContent, Styles.previewButtonSettingsStyle)) {
                _timeControl.Pause();
                _timeControl.NextSprite();
            }
        }

        private void DrawSpeedSlider() {

            if (GUILayout.Button(Contents.speedScale, Styles.preLabel)) _timeControl.Speed = 1;
            _timeControl.Speed = GUILayout.HorizontalSlider(_timeControl.Speed, 0, 5, Styles.preSlider, Styles.preSliderThumb);
            GUILayout.Label(_timeControl.Speed.ToString("0.00"), Styles.preLabel, GUILayout.Width(40));
        }

        class Styles {
            public static GUIStyle previewButtonSettingsStyle = new GUIStyle("preButton");
            public static GUIStyle preSlider = new GUIStyle("preSlider");
            public static GUIStyle preSliderThumb = new GUIStyle("preSliderThumb");
            public static GUIStyle preLabel = new GUIStyle("preLabel");
            public static GUIStyle grayLabel = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.gray } };
        }

        class Contents {
            public static GUIContent playButtonContent = EditorGUIUtility.IconContent("PlayButton");
            public static GUIContent pauseButtonContent = EditorGUIUtility.IconContent("PauseButton");
            public static GUIContent prevButtonContent = EditorGUIUtility.IconContent("Animation.PrevKey");
            public static GUIContent nextButtonContent = EditorGUIUtility.IconContent("Animation.NextKey");
            public static GUIContent speedScale = EditorGUIUtility.IconContent("SpeedScale");
        }

        public enum TextureDrawType {
            Normal,
            Alpha,
            Transparent
        }
    }
}