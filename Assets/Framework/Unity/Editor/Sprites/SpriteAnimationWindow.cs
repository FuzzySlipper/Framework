using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace PixelComrades {
    public class SpriteAnimationWindow : EditorWindow {
        [MenuItem("Window/Sprite Animation Window")]
        public static SpriteAnimationWindow ShowWindow() {
            var window = (SpriteAnimationWindow) GetWindow(typeof(SpriteAnimationWindow), false, "Sprite Animation Window");
            window.Init();
            return window;
        }

        private static float _checkerboardScale = 32.0f;
        private static float _timelineOffsetMin = -10;
        private static Texture2D _textureCheckerboard;
        private static readonly float TimelineScrubberHeight = 16;
        private static readonly float TimelineBottomBarHeight = 18;
        private static readonly float TimelineEventHeight = 12;
        private static readonly float ScrubberIntervalWidthMin = 10.0f;
        private static readonly float ScrubberIntervalWidthMax = 80.0f;
        private static readonly float ScrubberIntervalToShowLabel = 60.0f;
        private float _timelineEventBarHeight = TimelineEventHeight;
        private static readonly float FrameResizeRectWidth = 8;
        private static readonly Color ColorUnityBlue = new Color(0.3f, 0.5f, 0.85f, 1);
        private static readonly Color ColorEventBarBg = new Color(0.2f, 0.2f, 0.2f);
        private static readonly Color ColorEventLabelBg = ColorEventBarBg * 0.8f + Color.grey * 0.2f;
        private static readonly Color ColorEventLabelBgSelected = ColorEventBarBg * 0.8f + ColorUnityBlue * 0.2f;
        private static readonly float TimelineHeight = 200;
        private static readonly float InfoPanelWidth = 260;
        private int _resizeFrameId;
        
        [SerializeField] private bool _autoPlay = false;
        [SerializeField] private SpriteAnimation _clip;
        [SerializeField] private bool _playing;
        [SerializeField] private bool _previewLoop = true;

        private Vector2 _scrollPosition = Vector2.zero;
        private float _animTime;
        private Vector2 _previewOffset = Vector2.zero;
        private bool _previewResetScale;
        private float _previewScale = 1.0f;
        private float _previewSpeedScale = 1.0f;
        private float _timelineAnimWidth = 1;
        private float _timelineScale = 1000;
        private Texture2D _eventTexture = null;
        private int _currentFrame;
        private eDragState _dragState = eDragState.None;
        private double _editorTimePrev;
        private float _timelineOffset = -_timelineOffsetMin;

        private void Init() {
            EditorApplication.update += Update;
            Undo.undoRedoPerformed += OnUndoRedo;
            var size = 50;
            var border = 10;
            _eventTexture = new Texture2D(size, size, TextureFormat.ARGB32, false);
            for (int x = 0; x < size; x++) {
                for (int y = 0; y < size; y++) {
                    bool isBorder = x < border || y < border || x > size - border || y > size - border;
                    _eventTexture.SetPixel(x, y, isBorder ? Color.white : Color.clear);
                }
            }
            _eventTexture.Apply();
        }
        void OnEnable() {
            _editorTimePrev = EditorApplication.timeSinceStartup;
            OnSelectionChange();
        }

        void OnFocus() {
            OnSelectionChange();
        }

        private void Update() {
            if (_clip != null && _playing && _dragState != eDragState.Scrub) {
                var delta = (float) (EditorApplication.timeSinceStartup - _editorTimePrev);

                _animTime += delta * _previewSpeedScale;
                var length = _clip.LengthTime;

                if (_animTime >= length) {
                    if (_previewLoop) {
                        _animTime -= length;
                    }
                    else {
                        _playing = false;
                        _animTime = 0;
                    }
                }
                Repaint();
            }
            else if (_dragState != eDragState.None) {
                Repaint();
            }
            _currentFrame = _clip.ConvertAnimationTimeToFrame(_animTime);
            _editorTimePrev = EditorApplication.timeSinceStartup;
        }

        void OnGUI() {
            GUI.SetNextControlName("none");
            // If no sprite selected, show editor	
            if (_clip == null) {
                GUILayout.Space(10);
                GUILayout.Label("No animation selected", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            
            GUILayout.BeginHorizontal(Styles.PREVIEW_BUTTON);
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

            var lastRect = GUILayoutUtility.GetLastRect();

            var previewRect = new Rect(lastRect.xMin, lastRect.yMax, position.width - InfoPanelWidth,
                position.height - lastRect.yMax - TimelineHeight);
            if (_previewResetScale) {
                ResetPreviewScale(previewRect);
                _previewResetScale = false;
                _timelineScale = position.width/(MathEx.Max(0.5f, _clip.LengthTime)*1.25f);
            }
            LayoutPreview(previewRect);
            var infoPanelRect = new Rect(lastRect.xMin + position.width - InfoPanelWidth, lastRect.yMax,
                InfoPanelWidth, position.height - lastRect.yMax - TimelineHeight);
            LayoutInfoPanel(infoPanelRect);
            var timelineRect = new Rect(0, previewRect.yMax, position.width, TimelineHeight);
            LayoutTimeline(timelineRect);
        }

        void OnSelectionChange() {
            var obj = Selection.activeObject;
            if (obj != _clip && obj is SpriteAnimation) {
                _clip = Selection.activeObject as SpriteAnimation;
                OnClipChange();
            }
        }

        private void OnUndoRedo() {
            OnClipChange(false);
        }

        private void OnClipChange(bool resetPreview = true) {
            if (_clip == null)
                return;
            if (resetPreview) {
                _previewResetScale = true;
                _animTime = 0;
                _playing = _autoPlay;
                _scrollPosition = Vector2.zero;
                _previewOffset = Vector2.zero;
                _timelineOffset = -_timelineOffsetMin;
            }
            Repaint();
        }
        
        private void LayoutPreview(Rect rect) {
            var checkboardCoords = new Rect(Vector2.zero, rect.size/(_checkerboardScale*_previewScale));
            checkboardCoords.center = new Vector2(-_previewOffset.x, _previewOffset.y)/
                                      (_checkerboardScale*_previewScale);
            GUI.DrawTextureWithTexCoords(rect, GetCheckerboardTexture(), checkboardCoords, false);
            Sprite sprite = _clip.GetSprite(_currentFrame);
            if (sprite != null) {
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
                else if (e.type == EventType.MouseDown && e.button == 0) {
                    var frame = _clip.Frames[_currentFrame];
                    if (frame.Event == AnimationFrame.EventType.None) {
                        frame.Event = AnimationFrame.EventType.Default;
                    }
                    var mousePos = Event.current.mousePosition;
                    frame.EventPosition.x = Mathf.InverseLerp(rect.xMin, rect.xMax, mousePos.x);
                    frame.EventPosition.y = Mathf.InverseLerp(rect.yMax, rect.yMin, mousePos.y);
                }
            }
        }
        
        private void LayoutFrameSprite(Rect rect, Sprite sprite, float scale, Vector2 offset, bool useTextureRect, bool clipToRect) {
            var spriteRectOriginal = useTextureRect ? sprite.textureRect : sprite.rect;
            var texCoords = new Rect(spriteRectOriginal.x/sprite.texture.width,
                spriteRectOriginal.y/sprite.texture.height, spriteRectOriginal.width/sprite.texture.width,
                spriteRectOriginal.height/sprite.texture.height);

            var spriteRect = new Rect(Vector2.zero, spriteRectOriginal.size*scale);
            spriteRect.center = rect.center + offset;
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

                
                GUI.DrawTextureWithTexCoords(croppedRect, sprite.texture, texCoords, true);
                
                if (HasValidEvent(_animTime)) {
                    var rectSize = 25;
                    var eventRec = spriteRect;
                    var eventPos = _clip.GetFrameAtTime(_animTime).EventPosition;
                    var eventRectPos = new Vector2(Mathf.Lerp(eventRec.xMin, eventRec.xMax, eventPos.x),
                        Mathf.Lerp(eventRec.yMax, eventRec.yMin, eventPos.y));
                    
                    var eventRect = new Rect(eventRectPos, Vector2.one * rectSize);
                    GUI.DrawTexture(eventRect, _eventTexture);
                }
            }
            else {
                GUI.DrawTextureWithTexCoords(spriteRect, sprite.texture, texCoords, true);
            }
        }

        private void ResetPreviewScale(Rect rect) {
            Sprite sprite = _clip.GetSprite(0);
            _previewScale = 1;
            if (sprite != null && rect.width > 0 && rect.height > 0 && sprite.rect.width > 0 && sprite.rect.height > 0) {
                var widthScaled = rect.width / sprite.rect.width;
                var heightScaled = rect.height / sprite.rect.height;

                if (widthScaled < heightScaled) {
                    _previewScale = rect.width / sprite.rect.width;
                }
                else {
                    _previewScale = rect.height / sprite.rect.height;
                }

                _previewScale = Mathf.Clamp(_previewScale, 0.1f, 100.0f) * 0.95f;
            }
        }
        
        private bool HasValidEvent(float time) {
            var frame = _clip.GetFrameAtTime(time);
            return frame.Event != AnimationFrame.EventType.None;
        }
        
        private void LayoutInfoPanel(Rect rect) {
            GUILayout.BeginArea(rect, EditorStyles.inspectorFullWidthMargins);
            GUILayout.Space(20);
            EditorGUILayout.LabelField(
                string.Format("Length: {0:0.00} sec  {1:D} samples", _clip.LengthTime,
                    Mathf.RoundToInt(_clip.LengthTime/GetFrameTime())), new GUIStyle(EditorStyles.miniLabel) {
                        normal = {
                            textColor = Color.gray
                        }
                    });

            GUI.SetNextControlName("Framerate");
            var newFrameRate = EditorGUILayout.Slider(_clip.FramesPerSecond, 1, 30);
            newFrameRate = EditorGUILayout.DelayedFloatField(newFrameRate);
            if (Mathf.Approximately(newFrameRate, _clip.FramesPerSecond) == false) {
                ChangeFrameRate(newFrameRate, true);
            }
            GUI.SetNextControlName("Length");
            var oldLength = Utils.Snap(_clip.LengthTime, 0.05f);
            var newLength = Utils.Snap(EditorGUILayout.FloatField("Length (sec)", oldLength), 0.05f);
            if (Mathf.Approximately(newLength, oldLength) == false && newLength > 0) {
                newFrameRate = MathEx.Max(Utils.Snap(_clip.FramesPerSecond*(_clip.LengthTime/newLength), 1), 1);
                ChangeFrameRate(newFrameRate, false);
            }

            var looping = EditorGUILayout.Toggle("Looping", _clip.Looping);
            if (looping != _clip.Looping) {
                ChangeLooping(looping);
            }

            GUILayout.Space(10);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false);
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        
        private void LayoutTimeline(Rect rect) {
            var e = Event.current;

            _timelineScale = Mathf.Clamp(_timelineScale, 10, 10000);
            _timelineAnimWidth = _timelineScale*_clip.LengthTime;
            if (_timelineAnimWidth > rect.width/2.0f) {
                _timelineOffset = Mathf.Clamp(_timelineOffset, rect.width - _timelineAnimWidth - rect.width/2.0f,
                    -_timelineOffsetMin);
            }
            else {
                _timelineOffset = -_timelineOffsetMin;
            }
            var elementPosY = rect.yMin;
            var elementHeight = TimelineScrubberHeight;
            LayoutScrubber(new Rect(rect) {
                yMin = elementPosY,
                height = elementHeight
            });
            elementPosY += elementHeight;

            elementHeight = rect.height -
                            (elementPosY - rect.yMin + _timelineEventBarHeight + TimelineBottomBarHeight);
            var rectFrames = new Rect(rect) {
                yMin = elementPosY,
                height = elementHeight
            };
            LayoutFrames(rectFrames);
            elementPosY += elementHeight;

            elementHeight = _timelineEventBarHeight;
            LayoutEventsBarBack(new Rect(rect) {
                yMin = elementPosY,
                height = elementHeight
            });

            LayoutPlayhead(new Rect(rect) {
                height = rect.height - TimelineBottomBarHeight
            });

            elementHeight = _timelineEventBarHeight;
            LayoutEvents(new Rect(rect) {
                yMin = elementPosY,
                height = elementHeight
            });
            elementPosY += elementHeight;

            elementHeight = TimelineBottomBarHeight;
            LayoutBottomBar(new Rect(rect) {
                yMin = elementPosY,
                height = elementHeight
            });

            if (rect.Contains(e.mousePosition)) {
                if (e.type == EventType.ScrollWheel) {
                    var scale = 10000.0f;
                    while (_timelineScale/scale < 1.0f || _timelineScale/scale > 10.0f) {
                        scale /= 10.0f;
                    }

                    var oldCursorTime = GuiPosToAnimTime(rect, e.mousePosition.x);

                    _timelineScale -= e.delta.y*scale*0.05f;
                    _timelineScale = Mathf.Clamp(_timelineScale, 10.0f, 10000.0f);

                    // Offset to time at old cursor pos is same as at new position (so can zoom in/out of current cursor pos)
                    _timelineOffset += e.mousePosition.x - AnimTimeToGuiPos(rect, oldCursorTime);

                    Repaint();
                    e.Use();
                }
                else if (e.type == EventType.MouseDrag) {
                    if (e.button == 1 || e.button == 2) {
                        _timelineOffset += e.delta.x;
                        Repaint();
                        e.Use();
                    }
                }
            }

            if (e.rawType == EventType.MouseUp && e.button == 0 &&
                (_dragState == eDragState.SelectFrame)) {
                _dragState = eDragState.None;
                Repaint();
            }
        }

        private float AnimTimeToGuiPos(Rect rect, float time) {
            return rect.xMin + _timelineOffset + time * _timelineScale;
        }

        private void LayoutTimelineSprite(Rect rect, Sprite sprite) {
            if (sprite == null)
                return;

            // Can't display packed sprites at the moment, so don't bother trying
            if (sprite.packed && Application.isPlaying)
                return;

            var scale = 0.85f;
            if (sprite.textureRect.width > 0 && sprite.textureRect.height > 0) {
                var widthScaled = rect.width/sprite.textureRect.width;
                var heightScaled = rect.height/sprite.textureRect.height;
                // Finds best fit for timeline window based on sprite size
                if (widthScaled < heightScaled) {
                    scale *= rect.width/sprite.textureRect.width;
                }
                else {
                    scale *= rect.height/sprite.textureRect.height;
                }
            }

            LayoutFrameSprite(rect, sprite, scale, Vector2.zero, true, false);
        }
        private void LayoutToolbarLoop() {
            _previewLoop = GUILayout.Toggle(_previewLoop, _previewLoop ? Contents.LOOP_ON : Contents.LOOP_OFF,
                Styles.PREVIEW_BUTTON_LOOP, GUILayout.Width(25));
        }

        private void LayoutToolbarAnimName() {
            GUILayout.Space(10);
            if (GUILayout.Button(
                _clip.name, new GUIStyle(Styles.PREVIEW_BUTTON) {
                    stretchWidth = true,
                    alignment = TextAnchor.MiddleLeft
                })) {
                Selection.activeObject = _clip;
                EditorGUIUtility.PingObject(_clip);
            }
        }

        private void LayoutToolbarNextFrame() {
            if (GUILayout.Button(Contents.NEXT, Styles.PREVIEW_BUTTON, GUILayout.Width(25))) {
                _playing = false;
                ChangeFrame(1);
            }
        }

        private void ChangeFrame(int change) {
            var frame = Mathf.Clamp(_currentFrame + change, 0, _clip.Frames.Length - 1);
            _animTime = _clip.GetFrameStartTime(frame);
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
                    if (_animTime >= _clip.LengthTime) {
                        _animTime = 0;
                    }
                }
            }
        }

        private void LayoutToolbarPrevFrame() {
            if (GUILayout.Button(Contents.PREV, Styles.PREVIEW_BUTTON, GUILayout.Width(25))) {
                _playing = false;
                ChangeFrame(-1);
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

        private void ChangeFrameRate(float newFramerate, bool preserveTiming) {
            Undo.RecordObject(_clip, "Change Animation Framerate");
            _clip.FramesPerSecond = newFramerate;
            ApplyChanges();
        }

        private void ChangeLooping(bool looping) {
            Undo.RecordObject(_clip, "Change Animation Looping");
            _clip.Looping = looping;
            ApplyChanges();
        }

        private void LayoutBottomBar(Rect rect) {
            // Give bottom bar a min width
            rect = new Rect(rect) {
                width = MathEx.Max(rect.width, 655)
            };

            GUI.BeginGroup(rect, Styles.TIMELINE_BOTTOMBAR_BG);

            LayoutBottomBarFrameData(rect);
            GUI.EndGroup();
        }
        private void LayoutBottomBarFrameData(Rect rect) {
            var frame = _clip.GetFrameAtTime(_animTime);

            EditorGUI.BeginChangeCheck();

            float xOffset = 10;
            float width = 60;
            GUI.Label(new Rect(xOffset, 1, width, rect.height), "Frame:", EditorStyles.boldLabel);

            xOffset += width;
            width = 250;

            xOffset += width + 5;
            width = 50;
            // Frame length (in samples)
            EditorGUI.LabelField(new Rect(xOffset, 2, width, rect.height - 3), "Length");

            xOffset += width + 5;
            width = 30;

            GUI.SetNextControlName("Frame Length");
            frame.Length = EditorGUI.FloatField(new Rect(xOffset, 2, width, rect.height - 3), frame.Length);
            xOffset += width;

            width = 55;
            GUI.Label(new Rect(xOffset, 1, width, rect.height), "Event:", EditorStyles.boldLabel);
            xOffset += width;

            // Function Name
            //width = 125;
            //frame.DefaultEventTrigger = GUI.Toggle(new Rect(xOffset, 0, width, rect.height), frame.DefaultEventTrigger,
            //    "Default Event Trigger", Styles.TIMELINE_EVENT_TOGGLE);
            //xOffset += width;

            width = 80;
            frame.Event =
                (AnimationFrame.EventType)
                    EditorGUI.EnumPopup(new Rect(xOffset, 2, width, rect.height), frame.Event);
            xOffset += width;

            if (frame.Event == AnimationFrame.EventType.None) {
                if (EditorGUI.EndChangeCheck()) {
                    ApplyChanges();
                }
                return;
            }

            width = 50;
            // Frame length (in samples)
            EditorGUI.LabelField(new Rect(xOffset, 2, width, rect.height - 3), "Position");
            xOffset += width + 5;

            width = 150;
            frame.EventPosition.x = EditorGUI.Slider(new Rect(xOffset, 2, width, rect.height - 3), frame.EventPosition.x, -1, 1);
            xOffset += width + 5;
            frame.EventPosition.y = EditorGUI.Slider(new Rect(xOffset, 2, width, rect.height - 3), frame.EventPosition.y, -1, 1);
            xOffset += width + 5;

            width = 70;
            GUI.Label(new Rect(xOffset, 0, width, rect.height), "Event Name:", EditorStyles.miniLabel);
            xOffset += width;

            width = 150;
            // Give gui control name using it's time and function name so it can be auto-selected when creating new evnet
            GUI.SetNextControlName("EventFunctionName");
            frame.EventName = EditorGUI.TextField(new Rect(xOffset, 2, width, rect.height + 5),
                frame.EventName, EditorStyles.miniTextField);
            xOffset += width + 5;

            width = 60;
            frame.EventDataString = EditorGUI.TextField(
                new Rect(xOffset, 2, width, rect.height),
                frame.EventDataString, EditorStyles.miniTextField);
            xOffset += width + 5;

            frame.EventDataFloat = EditorGUI.FloatField(
                new Rect(xOffset, 2, width, rect.height - 3),
                frame.EventDataFloat, EditorStyles.miniTextField);
            xOffset += width + 5;

            width = 150;
            frame.EventDataObject = EditorGUI.ObjectField(
                new Rect(xOffset, 2, width, rect.height - 3), frame.EventDataObject, typeof(Object),
                false);

            if (EditorGUI.EndChangeCheck()) {
                ApplyChanges();
            }
        }
        

        private void LayoutPlayhead(Rect rect) {
            var offset = rect.xMin + _timelineOffset + _animTime * _timelineScale;
            DrawLine(new Vector2(offset, rect.yMin), new Vector2(offset, rect.yMax), Color.red);
        }
        
        private void LayoutEvents(Rect rect) {
            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition) && _playing == false) {
                _animTime = SnapTimeToFrameRate(GuiPosToAnimTime(rect, e.mousePosition.x));
            }
            GUI.BeginGroup(rect);
            for (int i = 0; i < _clip.Frames.Length; i++) {
                if (_clip.Frames[i].Event == AnimationFrame.EventType.None) {
                    continue;
                }
                var frame = _clip.Frames[i];
                var start = AnimTimeToGuiPos(rect, SnapTimeToFrameRate(_clip.GetFrameStartTime(i)));
                var text = frame.EventName;
                var textWidth = Styles.TIMELINE_EVENT_TEXT.CalcSize(new GUIContent(text)).x;
                var end = start + textWidth + 4;
                DrawRect(new Rect(start, 0, 1, rect.height), Color.grey);
                if (start > rect.xMax || end < rect.xMin)
                    continue;
                var heightOffset = 1 * TimelineEventHeight;
                var eventRect = new Rect(start, heightOffset, 0, TimelineEventHeight);
                var labelRect = new Rect(start + 2, heightOffset, textWidth, TimelineEventHeight - 2);
                LayoutEventVisuals(frame, false, eventRect, text, labelRect);
            }
            GUI.EndGroup();
            if (e.type == EventType.MouseDown && e.button == 0) {
                if (rect.Contains(e.mousePosition)) {
                    e.Use();
                }
            }
        }

        
        private void LayoutEventVisuals(AnimationFrame frame, bool selected, Rect eventRect, string labelText,
            Rect labelRect) {
            var eventColor = selected ? ColorUnityBlue : Color.grey;
            var eventBGColor = selected ? ColorEventLabelBgSelected : ColorEventLabelBg;

            //DrawRect( new Rect(eventRect) { width = EVENT_WIDTH }, eventColor  );
            DrawRect(labelRect, eventBGColor);
            GUI.Label(
                new Rect(labelRect) {
                    yMin = labelRect.yMin - 4
                }, labelText, new GUIStyle(Styles.TIMELINE_EVENT_TEXT) {
                    normal = {
                        textColor = eventColor
                    }
                });
            if (selected)
                GUI.color = ColorUnityBlue;
            GUI.Box(
                new Rect(eventRect.xMin - 2, eventRect.yMin, 6, 20), Contents.EVENT_MARKER,
                new GUIStyle(Styles.TIMELINE_EVENT_TICK) {
                    normal = {
                        textColor = eventColor
                    }
                });
            GUI.color = Color.white;
        }

        private void LayoutEventsBarBack(Rect rect) {
            GUI.BeginGroup(rect, Styles.TIMELINE_KEYFRAME_BG);
            GUI.EndGroup();
        }
        
        private void LayoutFrames(Rect rect) {
            var e = Event.current;

            GUI.BeginGroup(rect, Styles.TIMELINE_ANIM_BG);

            //DrawRect( new Rect(0,0,rect.width,rect.height), new Color(0.3f,0.3f,0.3f,1));

            for (var i = 0; i < _clip.Frames.Length; ++i) {
                // Calc time of next frame
                var time = _clip.GetFrameStartTime(i);
                var endTime = time + _clip.FrameTime * _clip.Frames[i].Length;
                var sprite = _clip.GetSprite(i);
                LayoutFrame(rect, i, time, endTime, sprite);
            }

            // Draw rect over area that has no frames in it
            if (_timelineOffset > 0) {
                // Before frames start
                DrawRect(new Rect(0, 0, _timelineOffset, rect.height), new Color(0.4f, 0.4f, 0.4f, 0.2f));
                DrawLine(new Vector2(_timelineOffset, 0), new Vector2(_timelineOffset, rect.height),
                    new Color(0.4f, 0.4f, 0.4f));
            }
            var endOffset = _timelineOffset + _clip.LengthTime*_timelineScale;
            if (endOffset < rect.xMax) {
                // After frames end
                DrawRect(new Rect(endOffset, 0, rect.width - endOffset, rect.height), new Color(0.4f, 0.4f, 0.4f, 0.2f));
            }

            GUI.EndGroup();

            if (_dragState == eDragState.None) {
                //
                // Check for unhandled mouse left click. It should deselect any selected frames
                //
                if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition)) {
                    e.Use();
                }
                // Check for unhanlded drag, it should start a select
                if (e.type == EventType.MouseDrag && e.button == 0 && rect.Contains(e.mousePosition)) {
                    _dragState = eDragState.SelectFrame;
                    e.Use();
                }
            }
            else if (_dragState == eDragState.ResizeFrame) {
                // While resizing frame, show the resize cursor
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
            }
            else if (_dragState == eDragState.MoveFrame) {
                // While moving frame, show the move cursor
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.MoveArrow);
            }
        }

        
        private void LayoutFrame(Rect rect, int frameId, float startTime, float endTime, Sprite sprite) {
            var startOffset = _timelineOffset + startTime*_timelineScale;
            var endOffset = _timelineOffset + endTime*_timelineScale;

            // check if it's visible on timeline
            if (startOffset > rect.xMax || endOffset < rect.xMin)
                return;
            var animFrame = _clip.Frames[frameId];
            var frameRect = new Rect(startOffset, 0, endOffset - startOffset, rect.height);
            var selected = _currentFrame == frameId;
            if (selected) {
                // highlight selected frames
                DrawRect(frameRect, Color.grey.WithAlpha(0.3f));
            }
            DrawLine(new Vector2(endOffset, 0), new Vector2(endOffset, rect.height), new Color(0.4f, 0.4f, 0.4f));
            LayoutTimelineSprite(frameRect, sprite);

            //
            // Frame clicking events
            //

            var e = Event.current;

            if (_dragState == eDragState.None) {
                //
                // Resize rect
                //
                var resizeRect = new Rect(endOffset - FrameResizeRectWidth*0.5f, 0, FrameResizeRectWidth,
                    rect.height);
                EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeHorizontal);

                //
                // Check for Start Resizing frame
                //
                if (e.type == EventType.MouseDown && e.button == 0 && resizeRect.Contains(e.mousePosition)) {
                    // Start resizing the frame
                    _dragState = eDragState.ResizeFrame;
                    _resizeFrameId = frameId;
                    GUI.FocusControl("none");
                    e.Use();
                }
                if (e.type == EventType.MouseDown && e.button == 0 && frameRect.Contains(e.mousePosition)) {
                    // Started clicking unselected - start selecting
                    _dragState = eDragState.SelectFrame;
                    SelectFrame(frameId);
                    GUI.FocusControl("none");
                    e.Use();
                }
            }
            else if (_dragState == eDragState.ResizeFrame) {
                // Check for resize frame by dragging mouse
                if (e.type == EventType.MouseDrag && e.button == 0 && _resizeFrameId == frameId) {
                    var newFrameLength =
                        GuiPosToAnimTime(new Rect(0, 0, position.width, position.height), e.mousePosition.x) -
                        startTime;
                    newFrameLength = MathEx.Max(newFrameLength, 1.0f / _clip.FramesPerSecond);
                    //SetFrameLength(frameId, newFrameLength);
                    e.Use();
                    Repaint();
                }

                // Check for finish resizing frame
                if (e.type == EventType.MouseUp && e.button == 0 && _resizeFrameId == frameId) {
                    _dragState = eDragState.None;
                    ApplyChanges();
                    e.Use();
                }
            }
            else if (_dragState == eDragState.SelectFrame) {
                if (e.type == EventType.MouseUp && e.button == 0) {
                    _dragState = eDragState.None;
                    e.Use();
                }
            }
        }
        

        private void LayoutScrubber(Rect rect) {
            var minUnitSecond = 1.0f/_clip.FramesPerSecond;
            var curUnitSecond = 1.0f;
            var curCellWidth = _timelineScale;
            var intervalScales = CreateIntervalSizeList(out var intervalId);

            // get curUnitSecond and curIdx
            if (curCellWidth < ScrubberIntervalWidthMin) {
                while (curCellWidth < ScrubberIntervalWidthMin) {
                    curUnitSecond = curUnitSecond*intervalScales[intervalId];
                    curCellWidth = curCellWidth*intervalScales[intervalId];

                    intervalId += 1;
                    if (intervalId >= intervalScales.Count) {
                        intervalId = intervalScales.Count - 1;
                        break;
                    }
                }
            }
            else if (curCellWidth > ScrubberIntervalWidthMax) {
                while ((curCellWidth > ScrubberIntervalWidthMax) &&
                       (curUnitSecond > minUnitSecond)) {
                    intervalId -= 1;
                    if (intervalId < 0) {
                        intervalId = 0;
                        break;
                    }

                    curUnitSecond = curUnitSecond/intervalScales[intervalId];
                    curCellWidth = curCellWidth/intervalScales[intervalId];
                }
            }

            // check if prev width is good to show
            if (curUnitSecond > minUnitSecond) {
                var intervalIdPrev = intervalId - 1;
                if (intervalIdPrev < 0)
                    intervalIdPrev = 0;
                var prevCellWidth = curCellWidth/intervalScales[intervalIdPrev];
                var prevUnitSecond = curUnitSecond/intervalScales[intervalIdPrev];
                if (prevCellWidth >= ScrubberIntervalWidthMin) {
                    intervalId = intervalIdPrev;
                    curUnitSecond = prevUnitSecond;
                    curCellWidth = prevCellWidth;
                }
            }

            // get lod interval list
            var lodIntervalList = new int[intervalScales.Count + 1];
            lodIntervalList[intervalId] = 1;
            for (var i = intervalId - 1; i >= 0; --i) {
                lodIntervalList[i] = lodIntervalList[i + 1]/intervalScales[i];
            }
            for (var i = intervalId + 1; i < intervalScales.Count + 1; ++i) {
                lodIntervalList[i] = lodIntervalList[i - 1]*intervalScales[i - 1];
            }

            // Calc width of intervals
            var lodWidthList = new float[intervalScales.Count + 1];
            lodWidthList[intervalId] = curCellWidth;
            for (var i = intervalId - 1; i >= 0; --i) {
                lodWidthList[i] = lodWidthList[i + 1]/intervalScales[i];
            }
            for (var i = intervalId + 1; i < intervalScales.Count + 1; ++i) {
                lodWidthList[i] = lodWidthList[i - 1]*intervalScales[i - 1];
            }

            // Calc interval id to start from
            var idxFrom = intervalId;
            for (var i = 0; i < intervalScales.Count + 1; ++i) {
                if (lodWidthList[i] > ScrubberIntervalWidthMax) {
                    idxFrom = i;
                    break;
                }
            }

            // NOTE: +50 here can avoid us clip text so early 
            var iStartFrom = Mathf.CeilToInt(-(_timelineOffset + 50.0f)/curCellWidth);
            var cellCount = Mathf.CeilToInt((rect.width - _timelineOffset)/curCellWidth);

            // draw the scrubber bar
            GUI.BeginGroup(rect, EditorStyles.toolbar);

            for (var i = iStartFrom; i < cellCount; ++i) {
                var x = _timelineOffset + i*curCellWidth + 1;
                var idx = idxFrom;

                while (idx >= 0) {
                    if (i%lodIntervalList[idx] == 0) {
                        var heightRatio = 1.0f - lodWidthList[idx]/ScrubberIntervalWidthMax;

                        // draw scrubber bar
                        if (heightRatio >= 1.0f) {
                            DrawLine(new Vector2(x, 0),
                                new Vector2(x, TimelineScrubberHeight),
                                Color.gray);
                            DrawLine(new Vector2(x + 1, 0),
                                new Vector2(x + 1, TimelineScrubberHeight),
                                Color.gray);
                        }
                        else {
                            DrawLine(new Vector2(x, TimelineScrubberHeight*heightRatio),
                                new Vector2(x, TimelineScrubberHeight),
                                Color.gray);
                        }

                        // draw lable
                        if (lodWidthList[idx] >= ScrubberIntervalToShowLabel) {
                            GUI.Label(new Rect(x + 4.0f, -2, 50, 15),
                                ToTimelineLabelString(i*curUnitSecond, _clip.FramesPerSecond), EditorStyles.miniLabel);
                        }

                        //
                        break;
                    }
                    --idx;
                }
            }

            GUI.EndGroup();
            var e = Event.current;
            if (rect.Contains(e.mousePosition)) {
                if (e.type == EventType.MouseDown) {
                    if (e.button == 0) {
                        _dragState = eDragState.Scrub;
                        _animTime = GuiPosToAnimTime(rect, e.mousePosition.x);
                        GUI.FocusControl("none");
                        e.Use();
                    }
                }
            }
            if (_dragState == eDragState.Scrub && e.button == 0) {
                if (e.type == EventType.MouseDrag) {
                    _animTime = GuiPosToAnimTime(rect, e.mousePosition.x);
                    e.Use();
                }
                else if (e.type == EventType.MouseUp) {
                    _dragState = eDragState.None;
                    e.Use();
                }
            }
        }

        private void SelectFrame(int frame) {
            _animTime = _clip.GetFrameStartTime(frame) + 0.01f;
        }

        public static string ToTimelineLabelString(float seconds, float sampleRate) {
            return string.Format("{0:0}:{1:00}", Mathf.FloorToInt(seconds), seconds % 1.0f * 100.0f);
        }

        private static void DrawRect(Rect rect, Color backgroundColor) {
            EditorGUI.DrawRect(rect, backgroundColor);
        }

        private void ApplyChanges() {
            Undo.RecordObject(_clip, "Animation Change");
            _clip.LastModified = System.DateTime.Now.ToString("G");
            EditorUtility.SetDirty(_clip);
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

        private float GetFrameTime() {
            return 1.0f / _clip.FramesPerSecond;
        }

        private float GuiPosToAnimTime(Rect rect, float mousePosX) {
            var pos = mousePosX - rect.xMin;
            return (pos - _timelineOffset) / _timelineScale;
        }

        private float SnapTimeToFrameRate(float value) {
            return Mathf.Round(value * _clip.FramesPerSecond) / _clip.FramesPerSecond;
        }

        private List<int> CreateIntervalSizeList(out int intervalId) {
            var intervalSizes = new List<int>();
            var tmpSampleRate = (int) _clip.FramesPerSecond;
            while (true) {
                var div = 0;
                if (tmpSampleRate == 30) {
                    div = 3;
                }
                else if (tmpSampleRate % 2 == 0) {
                    div = 2;
                }
                else if (tmpSampleRate % 5 == 0) {
                    div = 5;
                }
                else if (tmpSampleRate % 3 == 0) {
                    div = 3;
                }
                else {
                    break;
                }
                tmpSampleRate /= div;
                intervalSizes.Insert(0, div);
            }
            intervalId = intervalSizes.Count;
            intervalSizes.AddRange(
                new[] {
                    5, 2, 3, 2,
                    5, 2, 3, 2
                });
            return intervalSizes;
        }

        private static Texture2D GetCheckerboardTexture() {
            if (_textureCheckerboard == null) {
                _textureCheckerboard = new Texture2D(2, 2);
                _textureCheckerboard.name = "[Generated] Checkerboard Texture";
                _textureCheckerboard.hideFlags = HideFlags.DontSave;
                _textureCheckerboard.filterMode = FilterMode.Point;
                _textureCheckerboard.wrapMode = TextureWrapMode.Repeat;

                var c0 = new Color(0.4f, 0.4f, 0.4f, 1.0f);
                var c1 = new Color(0.278f, 0.278f, 0.278f, 1.0f);
                _textureCheckerboard.SetPixel(0, 0, c0);
                _textureCheckerboard.SetPixel(1, 1, c0);
                _textureCheckerboard.SetPixel(0, 1, c1);
                _textureCheckerboard.SetPixel(1, 0, c1);
                _textureCheckerboard.Apply();
            }
            return _textureCheckerboard;
        }

        private enum eDragState {
            None,
            Scrub,
            ResizeFrame,
            MoveFrame,
            SelectFrame,
            //MoveEvent,
            //SelectEvent
        }

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
    }
}