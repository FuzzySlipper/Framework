using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PixelComrades {
    public partial class SpriteAnimatorWindow : EditorWindow {

        private static readonly float TIMELINE_HEIGHT = 200;
        private static readonly float INFO_PANEL_WIDTH = 260;
        private static readonly float CHECKERBOARD_SCALE = 32.0f;
        private static Texture2D s_textureCheckerboard;
        private float _animTime;

        // When true, the anim plays automatically when animation is selected. Set to false when users manually stops an animation
        [SerializeField] private bool _autoPlay = false;

        [SerializeField] private SpriteAnimation _clip;

        // List of copied frames or events
        [SerializeField] private List<AnimFrame> _copiedFrames;

        // Used to repaint while drag and dropping into editor to show position indicator
        private bool _dragDropHovering = true;

        private eDragState _dragState = eDragState.None;
        private double _editorTimePrev;

        // When new event is created it's focused so you can immediately type the name
        [SerializeField] private List<AnimFrame> _frames;

        [SerializeField] private bool _playing;
        [SerializeField] private bool _previewloop = true;
        private Vector2 _previewOffset = Vector2.zero;
        private bool _previewResetScale; // When true, the preview will scale to best fit next update
        private float _previewScale = 1.0f;

        private float _previewSpeedScale = 1.0f;

        // List of selected frames or events
        private List<AnimFrame> _selectedFrames = new List<AnimFrame>();
        private float _timelineAnimWidth = 1;

        // Timeline view's offset from left (in pixels)
        private float _timelineOffset = -TIMELINE_OFFSET_MIN;
        // Unit per second on timeline
        private float _timelineScale = 1000;
        [SerializeField] private bool _uiImage;

        // Used to clear selection when hit play (avoids selection references being broken)
        private bool _wasPlaying;

        private Texture2D _eventTexture = null;

        public Texture2D EventTexture {
            get {
                if (_eventTexture == null) {
                    var size = 50;
                    var border = 10;
                    _eventTexture = new Texture2D(size, size, TextureFormat.ARGB32, false);
                    for (int x = 0; x < size; x++) {
                        for (int y = 0; y < size; y++) {
                            bool isBorder = x < border || y < border || x > size - border || y > size - border;
                            _eventTexture.SetPixel(x,y, isBorder ? Color.white : Color.clear);
                        }
                    }
                    _eventTexture.Apply();
                }
                return _eventTexture;
            }
        }

        public SpriteAnimatorWindow() {
            EditorApplication.update += Update;
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        // Class used for laying out events
        private class AnimEventLayoutData {
            public float end;
            public int heightOffset;
            public bool selected;
            public float start;
            public string text;
            public float textWidth;
            public AnimFrame frame;
        }

        // Class used internally to store info about a frame
        [Serializable] private class AnimFrame {
            public float RealLength;
            public float LengthMulti;
            public Sprite Sprite;
            public float Time;

            public AnimationFrame.EventType Event = AnimationFrame.EventType.None;
            public string EventName;
            public Vector2 EventPosition;
            public float EventDataFloat;
            public string EventDataString;
            public Object EventDataObject;
            public float EndTime { get { return Time + RealLength; } }
        }

        // Static list of content (built in unity icons and things)
        private class Contents {
            public static readonly GUIContent PLAY = EditorGUIUtility.IconContent("PlayButton");
            public static readonly GUIContent PAUSE = EditorGUIUtility.IconContent("PauseButton");
            public static readonly GUIContent PREV = EditorGUIUtility.IconContent("Animation.PrevKey");
            public static readonly GUIContent NEXT = EditorGUIUtility.IconContent("Animation.NextKey");
            public static readonly GUIContent SPEEDSCALE = EditorGUIUtility.IconContent("SpeedScale");
            public static readonly GUIContent ZOOM = EditorGUIUtility.IconContent("d_ViewToolZoom");
            public static readonly GUIContent LOOP_OFF = EditorGUIUtility.IconContent("d_RotateTool");
            public static readonly GUIContent LOOP_ON = EditorGUIUtility.IconContent("d_RotateTool On");
            public static readonly GUIContent PLAY_HEAD = EditorGUIUtility.IconContent("me_playhead");
            public static readonly GUIContent EVENT_MARKER = EditorGUIUtility.IconContent("d_Animation.EventMarker");
            public static readonly GUIContent ANIM_MARKER = EditorGUIUtility.IconContent("blendKey");
        }
        
        private enum eAnimSpriteType {
            Sprite,
            UIImage
        }

        // Static list of styles
        private class Styles {
            public static readonly GUIStyle PREVIEW_BUTTON = new GUIStyle("preButton");

            public static readonly GUIStyle PREVIEW_BUTTON_LOOP = new GUIStyle(PREVIEW_BUTTON) {
                padding = new RectOffset(0, 0, 2, 0)
            };

            public static readonly GUIStyle PREVIEW_SLIDER = new GUIStyle("preSlider");
            public static readonly GUIStyle PREVIEW_SLIDER_THUMB = new GUIStyle("preSliderThumb");
            public static readonly GUIStyle PREVIEW_LABEL_BOLD = new GUIStyle("preLabel");

            public static readonly GUIStyle PREVIEW_LABEL_SPEED = new GUIStyle("preLabel") {
                fontStyle = FontStyle.Normal,
                normal = {
                    textColor = Color.gray
                }
            };

            public static readonly GUIStyle TIMELINE_KEYFRAME_BG = new GUIStyle("AnimationKeyframeBackground");
#if UNITY_5_3 || UNITY_5_4
			public static readonly GUIStyle TIMELINE_ANIM_BG = new GUIStyle("AnimationCurveEditorBackground");
		#else
            public static readonly GUIStyle TIMELINE_ANIM_BG = new GUIStyle("CurveEditorBackground");
#endif
            public static readonly GUIStyle TIMELINE_BOTTOMBAR_BG = new GUIStyle("ProjectBrowserBottomBarBg");

            public static readonly GUIStyle TIMELINE_EVENT_TEXT = EditorStyles.miniLabel;
            public static readonly GUIStyle TIMELINE_EVENT_TICK = new GUIStyle();

            public static readonly GUIStyle TIMELINE_EVENT_TOGGLE = new GUIStyle(EditorStyles.toggle) {
                font = EditorStyles.miniLabel.font,
                fontSize = EditorStyles.miniLabel.fontSize,
                padding = new RectOffset(15, 0, 3, 0)
            };

            public static readonly GUIStyle INFOPANEL_LABEL_RIGHTALIGN = new GUIStyle(EditorStyles.label) {
                alignment = TextAnchor.MiddleRight
            };

        }

        /// Saves changes in the internal _frames to the actual animation clip
        private void ApplyChanges() {
            Undo.RecordObject(_clip, "Animation Change");
            _clip.Frames = new AnimationFrame[_frames.Count];
            for (int i = 0; i < _frames.Count; i++) {
                _clip.Frames[i] = new AnimationFrame() {
                    Length = _frames[i].LengthMulti,
                    SpriteIndex = i,
                    Event = _frames[i].Event,
                    EventName = _frames[i].EventName,
                    EventPosition = _frames[i].EventPosition,
                    EventDataFloat = _frames[i].EventDataFloat,
                    EventDataString = _frames[i].EventDataString,
                    EventDataObject = _frames[i].EventDataObject,
                };
            }
            _clip.LastModified = System.DateTime.Now.ToString("G");
            EditorUtility.SetDirty(_clip);
        }

        /// Duplicates the selected frames or events, selecting new items
        private bool CopySelected() {
            return CopySelected(ref _copiedFrames, _selectedFrames);
        }

        private bool CopySelected<T>(ref List<T> list, List<T> selectedList) where T : class, new() {
            if (selectedList.Count == 0)
                return false;

            list = selectedList.ConvertAll(item => Utils.Clone(item));
            return true;
        }

        /// Delete all selected frames or events
        private void DeleteSelected() {
            if (_selectedFrames.Count > 0) {
                _frames.RemoveAll(item => _selectedFrames.Contains(item));
                RecalcFrameTimes();
            }
            ClearSelection();
            Repaint();
            ApplyChanges();
        }

        private static void DrawLine(Vector2 from, Vector2 to, Color color, float width = 0) {
            if ((to - from).sqrMagnitude <= float.Epsilon)
                return;

            from.x = Mathf.FloorToInt(from.x);
            from.y = Mathf.FloorToInt(from.y);
            to.x = Mathf.FloorToInt(to.x);
            to.y = Mathf.FloorToInt(to.y);

            var savedColor = Handles.color;
            Handles.color = color;

            if (width > 1.0f)
                Handles.DrawAAPolyLine(width, from, to);
            else
                Handles.DrawLine(from, to);

            Handles.color = savedColor;
        }

        private static void DrawRect(Rect rect, Color backgroundColor) {
            EditorGUI.DrawRect(rect, backgroundColor);
        }

        private static void DrawRect(Rect rect, Color backgroundColor, Color borderColor, float borderWidth = 1) {
            // draw background
            EditorGUI.DrawRect(rect, backgroundColor);

            // Draw border
            rect.width = rect.width - borderWidth;
            rect.height = rect.height - borderWidth;
            DrawLine(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax), borderColor, borderWidth);
            DrawLine(new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMax, rect.yMax), borderColor, borderWidth);
            DrawLine(new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.yMin), borderColor, borderWidth);
            DrawLine(new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMin, rect.yMin), borderColor, borderWidth);
        }

        /// Duplicates the selected frames or events, selecting new items
        private bool DuplicateSelected() {
            if (DuplicateSelected(_frames, ref _selectedFrames)) {
                RecalcFrameTimes();
                Repaint();
                ApplyChanges();
                return true;
            }
            return false;
        }

        /// Templated function that duplicates the selected frames or events, selecting new items
        private bool DuplicateSelected<T>(List<T> list, ref List<T> selectedList) where T : class, new() {
            if (list.Count == 0 || selectedList.Count == 0)
                return false;

            var lastSelected = selectedList[selectedList.Count - 1];
            var index = list.FindLastIndex(item => item == lastSelected) + 1;

            // Clone all items
            var duplicatedItems = selectedList.ConvertAll(item => Utils.Clone(item));

            // Add the duplicated frames
            list.InsertRange(index, duplicatedItems);

            // Select the newly created frames
            ClearSelection();
            selectedList = duplicatedItems;

            return true;
        }

        private float GetAnimLength() {
            if (_frames != null && _frames.Count > 0) {
                var lastFrame = _frames[_frames.Count - 1];
                return lastFrame.Time + lastFrame.RealLength;
            }
            return 0;
        }

        // Returns a usable texture that looks like a high-contrast checker board.
        private static Texture2D GetCheckerboardTexture() {
            if (s_textureCheckerboard == null) {
                s_textureCheckerboard = new Texture2D(2, 2);
                s_textureCheckerboard.name = "[Generated] Checkerboard Texture";
                s_textureCheckerboard.hideFlags = HideFlags.DontSave;
                s_textureCheckerboard.filterMode = FilterMode.Point;
                s_textureCheckerboard.wrapMode = TextureWrapMode.Repeat;

                var c0 = new Color(0.4f, 0.4f, 0.4f, 1.0f);
                var c1 = new Color(0.278f, 0.278f, 0.278f, 1.0f);
                s_textureCheckerboard.SetPixel(0, 0, c0);
                s_textureCheckerboard.SetPixel(1, 1, c0);
                s_textureCheckerboard.SetPixel(0, 1, c1);
                s_textureCheckerboard.SetPixel(1, 0, c1);
                s_textureCheckerboard.Apply();
            }
            return s_textureCheckerboard;
        }

        private int GetCurrentFrame() {
            if (_frames == null || _frames.Count == 0)
                return -1;
            var frame = _frames.FindIndex(item => item.Time > _animTime);
            if (frame < 0)
                frame = _frames.Count;
            frame--;
            return frame;
        }

        private AnimFrame GetFrameAtTime(float time) {
            if (_frames == null || _frames.Count == 0)
                return null;
            var frame = _frames.FindIndex(item => item.Time > time);
            if (frame <= 0 || frame > _frames.Count)
                frame = _frames.Count;
            frame--;
            return _frames[frame];
        }

        private float GetFrameTime() {
            return 1.0f/_clip.FramesPerSecond;
        }

        private Sprite GetSpriteAtTime(float time) {
            var frame = GetFrameAtTime(time);
            return frame != null ? frame.Sprite : null;
        }

        private bool HasValidEvent(float time) {
            var frame = GetFrameAtTime(time);
            return frame.Event != AnimationFrame.EventType.None;
        }

        private Vector2 GetEventPosition(float time) {
            return GetFrameAtTime(time).EventPosition;
        }

        /// Add frames at a specific position
        private void InsertFrames(Sprite[] sprites, int atPos) {
            var frameLength = GetFrameTime();

            if (_frames.Count > 0) {
                // Find previous frame's length to use for inserted frames
                frameLength = _frames[atPos == 0 || atPos >= _frames.Count ? 0 : atPos - 1].RealLength;
            }

            var newFrames = Array.ConvertAll(sprites, sprite => {
                return new AnimFrame {
                    Sprite = sprite,
                    RealLength = frameLength
                };
            });

            atPos = Mathf.Clamp(atPos, 0, _frames.Count);
            _frames.InsertRange(atPos, newFrames);

            RecalcFrameTimes();
            Repaint();
            ApplyChanges();
        }

        private void LayoutFrameSprite(Rect rect, Sprite sprite, float scale, Vector2 offset, bool useTextureRect, bool clipToRect) {
            var spriteRectOriginal = useTextureRect ? sprite.textureRect : sprite.rect;
            var texCoords = new Rect(spriteRectOriginal.x/sprite.texture.width,
                spriteRectOriginal.y/sprite.texture.height, spriteRectOriginal.width/sprite.texture.width,
                spriteRectOriginal.height/sprite.texture.height);

            var spriteRect = new Rect(Vector2.zero, spriteRectOriginal.size*scale);
            spriteRect.center = rect.center + offset;
            //Rect clickRect;
            if (clipToRect) {
                // If the sprite doesn't fit in the rect, it needs to be cropped, and have it's uv's scaled to compensate (This is way more complicated than it should be!)
                var croppedRectOffset = new Vector2(MathEx.Max(spriteRect.xMin, rect.xMin),
                    MathEx.Max(spriteRect.yMin, rect.yMin));
                var croppedRectSize =
                    new Vector2(MathEx.Min(spriteRect.xMax, rect.xMax), MathEx.Min(spriteRect.yMax, rect.yMax)) -
                    croppedRectOffset;
                var croppedRect = new Rect(croppedRectOffset, croppedRectSize);
                texCoords.x += (croppedRect.xMin - spriteRect.xMin)/spriteRect.width*texCoords.width;
                texCoords.y += (spriteRect.yMax - croppedRect.yMax)/spriteRect.height*texCoords.height;
                texCoords.width *= 1.0f - (spriteRect.width - croppedRect.width)/spriteRect.width;
                texCoords.height *= 1.0f - (spriteRect.height - croppedRect.height)/spriteRect.height;

                // Draw the texture
                GUI.DrawTextureWithTexCoords(croppedRect, sprite.texture, texCoords, true);
                //clickRect = croppedRect;
                if (HasValidEvent(_animTime)) {
                    var rectSize = 25;
                    var eventPos = GetEventPosition(_animTime);
                    var eventRect = new Rect(croppedRect.center + new Vector2(-eventPos.x * (croppedRect.width/2)- rectSize, -eventPos.y * (croppedRect.height/2)- rectSize), Vector2.one * rectSize);
                    GUI.DrawTexture(eventRect, EventTexture);
                }
            }
            else {
                // Draw the texture
                GUI.DrawTextureWithTexCoords(spriteRect, sprite.texture, texCoords, true);
                //clickRect = spriteRect;
            }
            
            //GUI.DrawTexture(Texture2D.whiteTexture);
            //if (Event.current.isMouse && Event.current.button == 0 && Event.current.OnMouseDown(clickRect, 0, false)) {
            //if (Event.current.OnMouseDown(clickRect, 0, false)) {
            //    var mousePos = GUIUtility.ScreenToGUIPoint(Event.current.mousePosition);
            //    //var guiPos = GUIUtility.ScreenToGUIRect(clickRect);
            //    var pos = new Vector2(mousePos.x - clickRect.x, mousePos.y - clickRect.y);
            //    Debug.LogFormat("{0} {1} {2} {3} {4}", mousePos, clickRect.position, clickRect.center, pos, new Vector2(pos.x / spriteRectOriginal.width, pos.y / spriteRectOriginal.height));
            //}
        }

        private void LayoutPreview(Rect rect) {
            //
            // Draw checkerboard
            //
            var checkboardCoords = new Rect(Vector2.zero, rect.size/(CHECKERBOARD_SCALE*_previewScale));
            checkboardCoords.center = new Vector2(-_previewOffset.x, _previewOffset.y)/
                                      (CHECKERBOARD_SCALE*_previewScale);
            GUI.DrawTextureWithTexCoords(rect, GetCheckerboardTexture(), checkboardCoords, false);

            //
            // Draw sprite
            //
            Sprite sprite = null;
            if (_frames.Count > 0) {
                sprite = GetSpriteAtTime(_animTime);
            }

            if (sprite != null) {
                // Can't display packed sprites at the moment, so don't bother trying
                if (sprite.packed && Application.isPlaying) {
                    EditorGUI.LabelField(rect, "Disabled in Play Mode", new GUIStyle(EditorStyles.boldLabel) {
                        alignment = TextAnchor.MiddleCenter,
                        normal = {
                            textColor = Color.white
                        }
                    });
                    return;
                }

                LayoutFrameSprite(rect, sprite, _previewScale, _previewOffset, false, true);
            }

            //
            // Handle layout events
            //
            var e = Event.current;
            if (rect.Contains(e.mousePosition)) {
                if (e.type == EventType.ScrollWheel) {
                    var scale = 1000.0f;
                    while (_previewScale/scale < 1.0f || _previewScale/scale > 10.0f) {
                        scale /= 10.0f;
                    }
                    _previewScale -= e.delta.y*scale*0.05f;
                    _previewScale = Mathf.Clamp(_previewScale, 0.1f, 100.0f);
                    Repaint();
                    e.Use();
                }
                else if (e.type == EventType.MouseDrag) {
                    if (e.button == 1 || e.button == 2) {
                        if (sprite != null) {
                            _previewOffset += e.delta;
                            Repaint();
                            e.Use();
                        }
                    }
                }
            }
        }

        private void LayoutToolbarAnimName() {
            GUILayout.Space(10);
            if (GUILayout.Button(_clip.name, new GUIStyle(Styles.PREVIEW_BUTTON) {
                stretchWidth = true,
                alignment = TextAnchor.MiddleLeft
            })) {
                Selection.activeObject = _clip;
                EditorGUIUtility.PingObject(_clip);
            }
        }

        private void LayoutToolbarLoop() {
            _previewloop = GUILayout.Toggle(_previewloop, _previewloop ? Contents.LOOP_ON : Contents.LOOP_OFF,
                Styles.PREVIEW_BUTTON_LOOP, GUILayout.Width(25));
        }

        private void LayoutToolbarNextFrame() {
            if (GUILayout.Button(Contents.NEXT, Styles.PREVIEW_BUTTON, GUILayout.Width(25))) {
                if (_frames.Count <= 1)
                    return;

                _playing = false;
                var frame = Mathf.Clamp(GetCurrentFrame() + 1, 0, _frames.Count - 1);
                _animTime = _frames[frame].Time;
            }
        }

        private void LayoutToolbarPlay() {
            EditorGUI.BeginChangeCheck();
            _playing = GUILayout.Toggle(_playing, _playing ? Contents.PAUSE : Contents.PLAY, Styles.PREVIEW_BUTTON,
                GUILayout.Width(40));
            if (EditorGUI.EndChangeCheck()) {
                // Set the auto play variable. Anims will auto play when selected unless user has manually stopped an anim.
                _autoPlay = _playing;

                if (_playing) {
                    // Clicked play

                    // If anim is at end, restart
                    if (_animTime >= GetAnimLength()) {
                        _animTime = 0;
                    }
                }
            }
        }

        private void LayoutToolbarPrevFrame() {
            if (GUILayout.Button(Contents.PREV, Styles.PREVIEW_BUTTON, GUILayout.Width(25))) {
                if (_frames.Count <= 1)
                    return;

                _playing = false;
                var frame = Mathf.Clamp(GetCurrentFrame() - 1, 0, _frames.Count - 1);
                _animTime = _frames[frame].Time;
            }
        }

        private void LayoutToolbarScaleSlider() {
            // When press the zoom button, if scale isn't 1, set it to 1, otherwise, scale to it (so it toggles baseically)
            if (GUILayout.Button(Contents.ZOOM, Styles.PREVIEW_LABEL_BOLD, GUILayout.Width(30))) {
                if (_previewScale == 1)
                    _previewResetScale = true;
                else
                    _previewScale = 1;
            }
            _previewScale = GUILayout.HorizontalSlider(_previewScale, 0.1f, 5, Styles.PREVIEW_SLIDER,
                Styles.PREVIEW_SLIDER_THUMB, GUILayout.Width(50));
            GUILayout.Label(_previewScale.ToString("0.0"), Styles.PREVIEW_LABEL_SPEED, GUILayout.Width(40));
        }

        private void LayoutToolbarSpeedSlider() {
            if (GUILayout.Button(Contents.SPEEDSCALE, Styles.PREVIEW_LABEL_BOLD, GUILayout.Width(30)))
                _previewSpeedScale = 1;
            _previewSpeedScale = GUILayout.HorizontalSlider(_previewSpeedScale, 0, 4, Styles.PREVIEW_SLIDER,
                Styles.PREVIEW_SLIDER_THUMB, GUILayout.Width(50));
            GUILayout.Label(_previewSpeedScale.ToString("0.00"), Styles.PREVIEW_LABEL_SPEED, GUILayout.Width(40));
        }

        /// Moves any currently selected frames to after the specified index
        private void MoveSelectedFrames(int toIndex) {
            // Sort selected items by time so they can be moved in correct order
            _selectedFrames.Sort((a, b) => a.Time.CompareTo(b.Time));

            var insertAtEnd = toIndex >= _frames.Count;

            // Insert all items (remove from list, and re-add in correct position.
            foreach (var frame in _selectedFrames) {
                if (insertAtEnd) {
                    if (_frames[_frames.Count - 1] != frame) {
                        _frames.Remove(frame);
                        _frames.Add(frame);
                    }
                }
                else {
                    var insertBeforeFrame = _frames[toIndex];
                    if (insertBeforeFrame != frame) {
                        _frames.Remove(frame);
                        toIndex = _frames.FindIndex(item => item == insertBeforeFrame);
                        _frames.Insert(toIndex, frame);
                    }
                    toIndex++;
                }
            }
            RecalcFrameTimes();
            Repaint();
            ApplyChanges();
        }

        private void OnClipChange(bool resetPreview = true) {
            if (_clip == null)
                return;
            _frames = new List<AnimFrame>();
            float time = 0;
            for (int i = 0; i < _clip.Frames.Length; i++) {
                _frames.Add(new AnimFrame() {
                    Sprite = _clip.GetSpriteFrame(i),
                    Event = _clip.Frames[i].Event,
                    LengthMulti = _clip.Frames[i].Length,
                    RealLength = MathEx.Max(GetFrameTime(), SnapTimeToFrameRate(_clip.Frames[i].Length)),
                    EventName = _clip.Frames[i].EventName,
                    EventPosition = _clip.Frames[i].EventPosition,
                    EventDataFloat = _clip.Frames[i].EventDataFloat,
                    EventDataString = _clip.Frames[i].EventDataString,
                    EventDataObject = _clip.Frames[i].EventDataObject,
                });
                _frames.LastElement().Time = time;
                time += _clip.FrameTime*_frames.LastElement().RealLength;
            }
            RecalcFrameLengths();

            _framesReorderableList.list = _frames;
            if (resetPreview) {
                _previewResetScale = true;
                //_previewloop = _clip.Looping;
                _animTime = 0;
                _playing = _autoPlay;
                _scrollPosition = Vector2.zero;
                //_selectedEvents.Clear();
                _selectedFrames.Clear();
                _previewOffset = Vector2.zero;
                _timelineOffset = -TIMELINE_OFFSET_MIN;
            }
            Repaint();
        }

        private void OnEnable() {
            _editorTimePrev = EditorApplication.timeSinceStartup;

            InitialiseFramesReorderableList();

            OnSelectionChange();
        }

        private void OnFocus() {
            OnSelectionChange();
        }

        private void OnGUI() {
            GUI.SetNextControlName("none");
            // If no sprite selected, show editor	
            if (_clip == null || _frames == null) {
                GUILayout.Space(10);
                GUILayout.Label("No animation selected", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            
            GUILayout.BeginHorizontal(Styles.PREVIEW_BUTTON); // EditorStyles.toolbar );
            {
                LayoutToolbarPlay();
                LayoutToolbarPrevFrame();
                LayoutToolbarNextFrame();
                LayoutToolbarLoop();
                LayoutToolbarSpeedSlider();
                LayoutToolbarScaleSlider();
                LayoutToolbarAnimName();
            }
            GUILayout.EndHorizontal();

            //
            // Preview
            //

            var lastRect = GUILayoutUtility.GetLastRect();

            var previewRect = new Rect(lastRect.xMin, lastRect.yMax, position.width - INFO_PANEL_WIDTH,
                position.height - lastRect.yMax - TIMELINE_HEIGHT);
            if (_previewResetScale) {
                ResetPreviewScale(previewRect);
                _previewResetScale = false;

                // Also reset timeline length
                _timelineScale = position.width/(MathEx.Max(0.5f, _clip.LengthTime)*1.25f);
            }
            LayoutPreview(previewRect);

            //
            // Info Panel
            //
            var infoPanelRect = new Rect(lastRect.xMin + position.width - INFO_PANEL_WIDTH, lastRect.yMax,
                INFO_PANEL_WIDTH, position.height - lastRect.yMax - TIMELINE_HEIGHT);
            LayoutInfoPanel(infoPanelRect);

            //
            // Timeline
            //
            var timelineRect = new Rect(0, previewRect.yMax, position.width, TIMELINE_HEIGHT);
            LayoutTimeline(timelineRect);

            //
            // Handle keypress events that are also used in text fields, this requires check that a text box doesn't have focus
            //
            var e = Event.current;
            if (focusedWindow == this) {
                var allowKeypress = string.IsNullOrEmpty(GUI.GetNameOfFocusedControl()) ||
                                    GUI.GetNameOfFocusedControl() == "none";
                if (allowKeypress && e.type == EventType.KeyDown) {
                    switch (e.keyCode) {
                        case KeyCode.LeftArrow:
                        case KeyCode.RightArrow: {
                            // Change selected frame (if only 1 frame is selected)
                            if (_selectedFrames.Count > 0) {
                                // Find index of frame before selected frames (if left arrow) or after selected frames (if right arrow)
                                var index = 0;
                                if (e.keyCode == KeyCode.LeftArrow)
                                    index = _frames.FindIndex(frame => frame == _selectedFrames[0]) - 1;
                                else
                                    index =
                                        _frames.FindLastIndex(
                                            frame => frame == _selectedFrames[_selectedFrames.Count - 1]) + 1;
                                index = Mathf.Clamp(index, 0, _frames.Count - 1);
                                SelectFrame(_frames[index]);
                                e.Use();
                                Repaint();
                            }
                        }
                            break;
                    }
                }
            }

            //
            // Handle event commands- Delete, selectall, duplicate, copy, paste...
            //
            if (e.type == EventType.ValidateCommand) {
                switch (e.commandName) {
                    case "Delete":
                    case "SoftDelete":
                    case "SelectAll":
                    case "Duplicate":
                    case "Copy":
                    case "Paste": {
                        e.Use();
                    }
                        break;
                }
            }
            if (e.type == EventType.ExecuteCommand) {
                switch (e.commandName) {
                    case "Delete":
                    case "SoftDelete": {
                        DeleteSelected();
                        e.Use();
                    }
                        break;

                    case "SelectAll": {
                        //if (_selectedEvents.Count > 0 && _events.Count > 0) {
                        //    _selectedEvents.Clear();
                        //    _selectedEvents.AddRange(_events);
                        //}
                        if (_frames.Count > 0) {
                            _selectedFrames.Clear();
                            _selectedFrames.AddRange(_frames);
                        }

                        e.Use();
                    }
                        break;

                    case "Duplicate": {
                        DuplicateSelected();
                        e.Use();
                    }
                        break;

                    case "Copy": {
                        //_copiedEvents = null;
                        _copiedFrames = null;
                        CopySelected();
                        e.Use();
                    }
                        break;

                    case "Paste": {
                        Paste();
                        e.Use();
                    }
                        break;
                }
            }
        }

        /// Unity event called when the selectd object changes
        private void OnSelectionChange() {
            var obj = Selection.activeObject;
            if (obj != _clip && obj is SpriteAnimation) {
                _clip = Selection.activeObject as SpriteAnimation;
                OnClipChange();
            }
        }

        private void OnUndoRedo() {
            OnClipChange(false);
        }

        /// Duplicates the selected frames or events, selecting new items
        private bool Paste() {
            if (_copiedFrames != null && _copiedFrames.Count > 0) {
                // Find place to insert, either after selected frame, at caret, or at end of anim
                var index = _frames.Count;
                if (_selectedFrames.Count > 0) {
                    // If there's a selected item, then insert after it
                    var lastSelected = _selectedFrames[_selectedFrames.Count - 1];
                    index = _frames.FindLastIndex(item => item == lastSelected) + 1;
                }
                else if (_playing == false) {
                    index = GetCurrentFrame();
                }

                var pastedItems = _copiedFrames.ConvertAll(item => Utils.Clone(item));
                _frames.InsertRange(index, pastedItems);
                ClearSelection();
                _selectedFrames = pastedItems;

                RecalcFrameTimes();
            }
            Repaint();
            ApplyChanges();

            return true;
        }

        /// Update the lengths of each frame based on the times
        private void RecalcFrameLengths() {
            for (var i = 0; i < _frames.Count - 1; ++i) {
                _frames[i].RealLength = _frames[i + 1].Time - _frames[i].Time;
            }
            // If last frame has invalid length, set it to minimum length
            if (_frames.Count > 0 && _frames[_frames.Count - 1].RealLength < GetFrameTime())
                _frames[_frames.Count - 1].RealLength = GetFrameTime();

            //RepositionLinkedEvents();
        }

        /// Update the times of all frames based on the lengths
        private void RecalcFrameTimes() {
            float time = 0;
            foreach (var frame in _frames) {
                frame.Time = time;
                time += frame.RealLength;
            }

            //RepositionLinkedEvents();
        }

        private void ResetPreviewScale(Rect rect) {
            Sprite sprite = null;
            if (_frames.Count > 0)
                sprite = _frames[0].Sprite;

            _previewScale = 1;
            if (sprite != null && rect.width > 0 && rect.height > 0 && sprite.rect.width > 0 && sprite.rect.height > 0) {
                var widthScaled = rect.width/sprite.rect.width;
                var heightScaled = rect.height/sprite.rect.height;

                // Finds best fit for preview window based on sprite size
                if (widthScaled < heightScaled) {
                    _previewScale = rect.width/sprite.rect.width;
                }
                else {
                    _previewScale = rect.height/sprite.rect.height;
                }

                _previewScale = Mathf.Clamp(_previewScale, 0.1f, 100.0f)*0.95f;
            }
        }

        /// Handles selection a single frame on timeline and list, and puts playhead at start
        private void SelectFrame(AnimFrame selectedFrame) {
            var ctrlClick = Event.current.control;
            var shiftClick = Event.current.shift && _selectedFrames.Count == 1;
                // Can only shift click if 1 is selected already

            // Clear existing events unless ctrl is clicked, or we're select dragging
            if (ctrlClick == false && shiftClick == false) {
                _selectedFrames.Clear();
            }
            //_selectedEvents.Clear();

            // Don't add if already in selection list, and if holding ctrl remove it from the list
            if (_selectedFrames.Contains(selectedFrame) == false) {
                if (shiftClick) {
                    // Add frames between selectd and clicked.
                    var indexFrom = _frames.FindIndex(item => item == _selectedFrames[0]);
                    var indexTo = _frames.FindIndex(item => item == selectedFrame);
                    if (indexFrom > indexTo) Utils.Swap(ref indexFrom, ref indexTo);
                    for (var i = indexFrom + 1; i < indexTo; ++i) {
                        _selectedFrames.Add(_frames[i]);
                    }
                }
                _selectedFrames.Add(selectedFrame);

                if (ctrlClick == false) {
                    _framesReorderableList.index = _frames.FindIndex(item => item == selectedFrame);

                    // Put playhead at beginning of selected frame (if not playing)
                    if (_playing == false)
                        _animTime = selectedFrame.Time;
                }
            }
            else if (ctrlClick) {
                _selectedFrames.Remove(selectedFrame);
            }

            // Sort selection
            _selectedFrames.Sort((a, b) => a.Time.CompareTo(b.Time));
        }

        /// Sets the length of a particular frame, updates other frame times
        private void SetFrameLength(int frameId, float length) {
            if (Mathf.Approximately(length, _frames[frameId].RealLength) == false) {
                _frames[frameId].RealLength = MathEx.Max(GetFrameTime(), SnapTimeToFrameRate(length));
                RecalcFrameTimes();
            }
        }

        private void SetFrameLength(AnimFrame frame, float length) {
            if (Mathf.Approximately(length, frame.RealLength) == false) {
                frame.RealLength = MathEx.Max(GetFrameTime(), SnapTimeToFrameRate(length));
                RecalcFrameTimes();
            }
        }

        /// Snaps a time to the closest sample time on the timeline
        private float SnapTimeToFrameRate(float value) {
            return Mathf.Round(value*_clip.FramesPerSecond)/_clip.FramesPerSecond;
        }

        private void Update() {
            if (_clip != null && _playing && _dragState != eDragState.Scrub) {
                // Update anim time if playing (and not scrubbing)
                var delta = (float) (EditorApplication.timeSinceStartup - _editorTimePrev);

                _animTime += delta*_previewSpeedScale;

                if (_animTime >= GetAnimLength()) {
                    if (_previewloop) {
                        _animTime -= GetAnimLength();
                    }
                    else {
                        _playing = false;
                        _animTime = 0;
                    }
                }

                Repaint();
            }
            else if (_dragDropHovering || _dragState != eDragState.None) {
                Repaint();
            }

            // When going to Play, we need to clear the selection since references get broken.
            if (_wasPlaying != EditorApplication.isPlayingOrWillChangePlaymode) {
                _wasPlaying = EditorApplication.isPlayingOrWillChangePlaymode;
                if (_wasPlaying)
                    ClearSelection();
            }

            _editorTimePrev = EditorApplication.timeSinceStartup;
        }

        [MenuItem("Tools/Sprite Animator Window")] public static void ShowWindow() {
            GetWindow(typeof(SpriteAnimatorWindow), false, "Sprite Animator");
        }
    }

    /// Handy extention methods
    public static class ExtentionMethods {
        public static Color WithAlpha(this Color col, float alpha) {
            return new Color(col.r, col.g, col.b, alpha);
        }
    }

    /// Handy utils
    public static class Utils {

        // Creates new instance of passed object and copies variables
        public static T Clone<T>(T from) where T : new() {
            var result = new T();

            var finfos = from.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (var i = 0; i < finfos.Length; ++i) {
                finfos[i].SetValue(result, finfos[i].GetValue(from));
            }
            return result;
        }

        /// Returns float value snapped to closest point
        public static float Snap(float value, float snapTo) {
            if (snapTo <= 0) return value;
            return Mathf.Round(value/snapTo)*snapTo;
        }

        /// Swaps two objects
        public static void Swap<T>(ref T lhs, ref T rhs) {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }

    /// For sorting strings by natural order (so, for example walk_9.png is sorted before walk_10.png)
    public class NaturalComparer : Comparer<string>, IDisposable {
        // NaturalComparer function courtesy of Justin Jones http://www.codeproject.com/Articles/22517/Natural-Sort-Comparer

        private Dictionary<string, string[]> _table;

        public NaturalComparer() {
            _table = new Dictionary<string, string[]>();
        }

        public void Dispose() {
            _table.Clear();
            _table = null;
        }

        private static int PartCompare(string left, string right) {
            int x, y;
            if (!int.TryParse(left, out x)) {
                return left.CompareTo(right);
            }

            if (!int.TryParse(right, out y)) {
                return left.CompareTo(right);
            }

            return x.CompareTo(y);
        }

        public override int Compare(string x, string y) {
            if (x == y)
                return 0;

            string[] x1, y1;
            if (!_table.TryGetValue(x, out x1)) {
                x1 = Regex.Split(x.Replace(" ", ""), "([0-9]+)");
                _table.Add(x, x1);
            }
            if (!_table.TryGetValue(y, out y1)) {
                y1 = Regex.Split(y.Replace(" ", ""), "([0-9]+)");
                _table.Add(y, y1);
            }

            for (var i = 0; i < x1.Length && i < y1.Length; i++) {
                if (x1[i] != y1[i]) {
                    return PartCompare(x1[i], y1[i]);
                }
            }

            if (y1.Length > x1.Length) {
                return 1;
            }
            if (x1.Length > y1.Length) {
                return -1;
            }

            return 0;
        }
    }

}