using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;

namespace PixelComrades {
    [CustomEditor(typeof(SpriteAnimation), true), CanEditMultipleObjects]
    public class SpriteAnimationEditor : OdinEditor {

        private TextureDrawType _selectedTextureDrawType = TextureDrawType.Transparent;
        private Material _normalMat;
        private SpriteAnimationPreview _timeControl;

        protected override void OnEnable() {
            base.OnEnable();
            _normalMat = new Material(Shader.Find("Sprites/Default"));
            _timeControl = new SpriteAnimationPreview((SpriteAnimation)target);
        }

        private float _distance = 0.2f;
        private float _quality = 0.35f;
        private SpriteAnimation _copyAnimation;

        public override void OnInspectorGUI() {
            var script = (SpriteAnimation)target;
            if (GUILayout.Button("Edit Animations")) {
                SpriteAnimationWindow.ShowWindow();
            }
            if (GUILayout.Button("Generate Colliders")) {
                if (targets.Length > 1) {
                    for (int i = 0; i < targets.Length; i++) {
                        SpriteMeshUtilities.GenerateColliders(targets[i] as SpriteAnimation, _distance, _quality);
                        EditorUtility.SetDirty(targets[i]);
                    }
                }
                else {
                    SpriteMeshUtilities.GenerateColliders(script, _distance, _quality);
                    EditorUtility.SetDirty(script);
                }
            }
            if (GUILayout.Button("Edit Critical Colliders")) {
                SpriteColliderCriticalWindow.ShowCriticalWindow().Set(script);
            }
            if (_copyAnimation != null && GUILayout.Button("Clone To This Animation")) {
                var cnt = _copyAnimation.LengthSprites;
                if (cnt != script.LengthSprites) {
                    Debug.LogErrorFormat("{0} has {1} sprites different from our sprite cnt {2}", _copyAnimation.name, cnt, script.LengthSprites);
                }
                else {
                    var colliders = _copyAnimation.Colliders;
                    script.Colliders = new SavedSpriteCollider[colliders.Length];
                    script.Colliders.AddRange(colliders);
                }
            }
            base.OnInspectorGUI();
            EditorGUILayout.LabelField(string.Format("Last Modified {0}", script.LastModified));
            EditorGUILayout.LabelField(string.Format("Length: {0}", script.LengthTime.ToString("F1")));
            EditorGUILayout.LabelField(string.Format("FrameTime: {0}", script.FrameTime.ToString("F1")));
            EditorGUILayout.LabelField(string.Format("Animation Length: {0}", script.LengthFrames));
            _distance = EditorGUILayout.FloatField("Collider Size", _distance);
            _quality = EditorGUILayout.FloatField("Collider Quality", _quality);
            _copyAnimation = (SpriteAnimation) EditorGUILayout.ObjectField("Clone Source", _copyAnimation, typeof(SpriteAnimation), false);
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

    public class SpriteCopy : EditorWindow {
        private Object _copyFrom;
        private Object _copyTo;

        // Creates a new option in "Windows"
        [MenuItem("Window/Copy Spritesheet pivots and slices")]
        private static void Init() {
            // Get existing open window or if none, make a new one:
            SpriteCopy window = (SpriteCopy) GetWindow(typeof(SpriteCopy));
            window.Show();
        }

        private void OnGUI() {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Copy from:", EditorStyles.boldLabel);
            _copyFrom = EditorGUILayout.ObjectField(_copyFrom, typeof(Texture2D), false, GUILayout.Width(220));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Copy to:", EditorStyles.boldLabel);
            _copyTo = EditorGUILayout.ObjectField(_copyTo, typeof(Texture2D), false, GUILayout.Width(220));
            GUILayout.EndHorizontal();
            GUILayout.Space(25f);
            if (GUILayout.Button("Copy pivots and slices")) {
                CopyPivotsAndSlices(_copyTo);
            }
            if (GUILayout.Button("Copy pivots to selection")) {
                for (int i = 0; i < Selection.objects.Length; i++) {
                    CopyPivotsAndSlices(Selection.objects[i]);
                }
            }
        }

        private void CopyPivotsAndSlices(Object copyTo) {
            if (!_copyFrom || !copyTo) {
                Debug.Log("Missing one object");
                return;
            }
            if (_copyFrom.GetType() != typeof(Texture2D) || copyTo.GetType() != typeof(Texture2D)) {
                Debug.Log("Cant convert from: " + _copyFrom.GetType() + "to: " + copyTo.GetType() + ". Needs two Texture2D objects!");
                return;
            }
            string copyFromPath = AssetDatabase.GetAssetPath(_copyFrom);
            TextureImporter ti1 = AssetImporter.GetAtPath(copyFromPath) as TextureImporter;
            ti1.isReadable = true;
            string copyToPath = AssetDatabase.GetAssetPath(copyTo);
            TextureImporter ti2 = AssetImporter.GetAtPath(copyToPath) as TextureImporter;
            ti2.isReadable = true;
            ti2.spriteImportMode = SpriteImportMode.Multiple;
            List<SpriteMetaData> newData = new List<SpriteMetaData>();
            Debug.Log("Amount of slices found: " + ti1.spritesheet.Length);
            for (int i = 0; i < ti1.spritesheet.Length; i++) {
                SpriteMetaData d = ti1.spritesheet[i];
                d.name = copyTo.name + "_" + i;
                newData.Add(d);
            }
            ti2.spritesheet = newData.ToArray();
            AssetDatabase.ImportAsset(copyToPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
        }
    }
}