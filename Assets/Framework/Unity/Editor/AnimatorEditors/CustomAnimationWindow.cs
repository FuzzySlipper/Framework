using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PixelComrades {
    public class CustomAnimationWindow : EditorWindow {

        private const float TimelinePositionY = 25f;
        private const float NotchPosition = 43f;
        private const float LabelWidth = 20;
        private const float LabelOffset = 5f;
        private const float NotchWidth = 5f;
        private const float HandleWidth = 10f;
        private static int _myControlId;
        //private readonly Dictionary<int, TempObjectValues> _playTimeChanges = new Dictionary<int, TempObjectValues>();
        private readonly List<AnimationObject> _selectedObjects = new List<AnimationObject>();
        private AnimationObjectWindow _objectWindow;
        private GenericMenu _addTrackMenu;
        private bool _batchSelect;
        private GenericAnimation _currentAnimation;
        private AnimationObject _draggedObject;
        private bool _endHandleDragged;
        private List<Type> _animationObjectTypes = new List<Type>();
        private float _hScrollPosition;
        private bool _startHandleDragged;
        private float _startPositionX;
        private float _visibleDuration;
        private float _vScrollPosition;
        private int _boxHeight = 35;
        private Task _playTask;
        private float _currentTime;
        private float _visibleOffset;
        private float _visibleScale = 1f;
        private bool _paused = false;
        private WhileLoopLimiter _playLimiter = new WhileLoopLimiter(99999);
        
        private GenericAnimation CurrentAnimation { get => _currentAnimation; set => _currentAnimation = value; }

        public CustomAnimationWindow() {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int a = 0; a < assemblies.Length; a++) {
                var types = assemblies[a].GetTypes();
                for (int t = 0; t < types.Length; t++) {
                    var type = types[t];
                    if (type.IsSubclassOf(typeof(AnimationObject))) {
                        _animationObjectTypes.Add(type);
                    }
                }
            }
        }

        private void CreateContextItem(object obj) {
            var targetType = obj as Type;
            if (targetType == null) {
                return;
            }
            var newObj = CreateInstance(targetType);
            AssetDatabase.AddObjectToAsset(newObj, _currentAnimation);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newObj));
            var animObj = newObj as AnimationObject;
            CurrentAnimation.Add(animObj);
            FindOpenStartTime(animObj);
            Repaint();
        }

        private void FindOpenStartTime(AnimationObject animObject) {
            float start = animObject.StartTime;
            while (true) {
                if (Intersects(animObject, animObject.EditingTrack, start, animObject.Duration)) {
                    start += 0.1f;
                }
                else {
                    animObject.StartTime = start;
                    break;
                }
            }
        }

        private bool Intersects(AnimationObject dragged, int track, float start, float duration) {
            var endTime = start + duration;
            for (int i = 0; i < _currentAnimation.Objects.Count; i++) {
                var obj = _currentAnimation.Objects[i];
                if (obj == dragged || obj.EditingTrack != track) {
                    continue;
                }
                if (obj.StartTime < endTime && start < obj.EndTime) {
                    return true;
                }
            }
            return false;
        }
        
        private void DragAction(AnimationObject dragged, float visibleDuration, float offset, int trackOffset) {
            var newStartTime = dragged.StartTime;
            var newDuration = dragged.Duration;
            var newEditingTrack = dragged.EditingTrack;
            var diff = (Event.current.mousePosition.x - _startPositionX) * visibleDuration / position.width;
            
            if (Mathf.Abs(diff) >= 0.01f) {
                if (_endHandleDragged) {
                    newDuration = RoundToPoint1(dragged.Duration + diff);
                }
                else if (_startHandleDragged) {
                    newStartTime = RoundToPoint1(dragged.StartTime + diff);
                    newDuration = dragged.Duration + dragged.StartTime - newStartTime;
                }
                else {
                    newStartTime = RoundToPoint1(dragged.StartTime + diff);
                }
                _startPositionX = Event.current.mousePosition.x;
            }
            if (!_startHandleDragged && !_endHandleDragged) {
                var trackBorder = (dragged.EditingTrack + 1) * _boxHeight + offset;
                if (Event.current.mousePosition.y > trackBorder || Event.current.mousePosition.y < trackBorder - _boxHeight) {
                    newEditingTrack = Mathf.FloorToInt((Event.current.mousePosition.y - offset) / _boxHeight + trackOffset);
                    newEditingTrack = Mathf.Clamp(newEditingTrack, 0, newEditingTrack);
                }
            }
            if (dragged.StartTime != newStartTime || dragged.Duration != newDuration || dragged.EditingTrack != newEditingTrack) {
                if (Intersects(dragged, newEditingTrack, newStartTime, newDuration) || newStartTime < 0) {
                    return;
                }
                if (newStartTime + newDuration > CurrentAnimation.MaxDuration) {
                    CurrentAnimation.MaxDuration = newStartTime + newDuration;
                }
                foreach (var selectedTrack in _selectedObjects.Except(new[] {dragged})) {
                    selectedTrack.StartTime -= dragged.StartTime - newStartTime;
                    selectedTrack.Duration -= dragged.Duration - dragged.Duration;
                    selectedTrack.EditingTrack += newEditingTrack - dragged.EditingTrack;
                }
                dragged.StartTime = newStartTime;
                dragged.Duration = newDuration;
                dragged.EditingTrack = newEditingTrack;
                Repaint();
            }
        }

        private void DrawObjects(List<AnimationObject> objs, float offset, float duration, float hOffset, int trackOffset) {
            var maxVisibleTracks = position.height / _boxHeight + 1;
            var maxTracks = CurrentAnimation.MaxTracks;
            for (var i = 0; i < (maxTracks < maxVisibleTracks ? maxTracks : maxVisibleTracks); i++) {
                var trackStyle = new GUIStyle(GUI.skin.box) {
                    normal = {background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.1f))},
                    name = "Track Style"
                };
                GUI.Box(new Rect(0, offset + _boxHeight * i, position.width, _boxHeight + 1), string.Empty, trackStyle);
            }
            foreach (var obj in objs) {
                if (obj.EditingTrack < trackOffset || obj.EditingTrack >= trackOffset + maxVisibleTracks) {
                    continue;
                }
                var horizontalPosStart = position.width * (obj.StartTime / duration) - hOffset;
                var horizontalPosEnd = position.width * (obj.EndTime / duration) - hOffset;
                var width = horizontalPosEnd - horizontalPosStart;
                var boxRect = new Rect(
                    horizontalPosStart + HandleWidth, offset + _boxHeight * (obj.EditingTrack - trackOffset), width - HandleWidth * 2,
                    _boxHeight);
                EditorGUIUtility.AddCursorRect(boxRect, MouseCursor.Pan);
                var boxStartHandleRect = new Rect(
                    horizontalPosStart, offset + _boxHeight * (obj.EditingTrack - trackOffset), HandleWidth, _boxHeight);
                EditorGUIUtility.AddCursorRect(boxStartHandleRect, MouseCursor.ResizeHorizontal);
                GUI.Box(boxStartHandleRect, "<");
                var boxEndHandleRect = new Rect(
                    horizontalPosEnd - HandleWidth, offset + _boxHeight * (obj.EditingTrack - trackOffset), HandleWidth, _boxHeight);
                EditorGUIUtility.AddCursorRect(boxEndHandleRect, MouseCursor.ResizeHorizontal);
                GUI.Box(boxEndHandleRect, ">");
                obj.DrawTimelineGui(boxRect);
                var startHandle = boxStartHandleRect.Contains(Event.current.mousePosition);
                var endHandle = boxEndHandleRect.Contains(Event.current.mousePosition);
                var mainHandle = boxRect.Contains(Event.current.mousePosition);
                var alreadySelected = _selectedObjects.Contains(obj);
                if (!_batchSelect &&
                    (Event.current.type == EventType.MouseDown || Event.current.type == EventType.ContextClick) &&
                    (startHandle || mainHandle || endHandle)) {
                    if (Event.current.button == 0) {
                        switch (Event.current.clickCount) {
                            case 1:
                                _draggedObject = obj;
                                _startPositionX = Event.current.mousePosition.x;
                                _endHandleDragged = endHandle;
                                _startHandleDragged = startHandle;
                                GUIUtility.hotControl = _myControlId;
                                break;
                            case 2:
                                _objectWindow = (AnimationObjectWindow) GetWindow(typeof(AnimationObjectWindow), true, "Object Editor");
                                _objectWindow.Set(obj);
                                break;
                        }
                    }
                    else if (Event.current.button == 1 || Event.current.type == EventType.ContextClick) {
                        var contextMenu = new GenericMenu();
                        contextMenu.AddItem(new GUIContent("Remove"), false, RemoveContextItem, obj);
                        contextMenu.ShowAsContext();
                        Event.current.Use();
                    }
                }
                else if (mainHandle && _batchSelect && Event.current.type == EventType.MouseUp) {
                    if (alreadySelected) {
                        _selectedObjects.Remove(obj);
                    }
                    else {
                        _selectedObjects.Add(obj);
                    }
                    Repaint();
                }
                if (_batchSelect || alreadySelected) {
                    var toggleRect = new Rect(boxRect.xMax - 20f, boxRect.y + 1f, 5f, 5f);
                    GUI.Toggle(toggleRect, alreadySelected, string.Empty);
                }
            }
        }

        private void DrawTimeline(float yMax) {
            if (_playTask != null) {
                var horizontalPosStart =
                    position.width * (_currentTime / _visibleDuration) - _visibleOffset;
                var timeRect = new Rect(horizontalPosStart, yMax, 1f, position.height - yMax);
                GUI.Box(timeRect, string.Empty);
                Repaint();
            }
        }

        private void OnGUI() {
            if (CurrentAnimation == null) {
                GUILayout.Label("Select Animation");
                if (Selection.objects.Length == 1) {
                    OnSelectionChange();
                }
                return;
            }
            if (_animationObjectTypes.Count == 0) {
                GUILayout.Label("No Animation Types");
            }
            var objs = CurrentAnimation.Objects;
            var newBatchSelect = Event.current.command || Event.current.control;
            if (_batchSelect != newBatchSelect) {
                _batchSelect = newBatchSelect;
                Repaint();
            }
            if (Event.current.type == EventType.ScrollWheel) {
                _visibleScale = Mathf.Clamp(_visibleScale + Mathf.Sign(Event.current.delta.y) * 0.1f, 0.1f, 100);
                Repaint();
            }
            GUILayout.BeginHorizontal();
            if (!Application.isPlaying) {
                if (_playTask == null) {
                    if (GUILayout.Button("PLAY")) {
                        _playTask = TimeManager.StartUnscaled(PlayAnimation());
                    }
                }
                else {
                    if (GUILayout.Button("STOP")) {
                        _playTask.Cancel();
                        _playTask = null;
                    }
                    _paused = GUILayout.Toggle(_paused, "Paused");
                }
                
            }
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth *= 0.75f;
            _visibleScale = EditorGUILayout.Slider("Scale:", _visibleScale, 0.1f, 100f);
            CurrentAnimation.MaxDuration = EditorGUILayout.FloatField(
                "Max duration:",
                CurrentAnimation.MaxDuration);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            CurrentAnimation.MaxTracks = EditorGUILayout.IntField("Max tracks:", CurrentAnimation.MaxTracks);
            _boxHeight = EditorGUILayout.IntSlider("Track height:", _boxHeight, 20, 100);
            if (_draggedObject == null) {
                var newVisibleDuration = CurrentAnimation.MaxDuration / _visibleScale;
                if (Math.Abs(_visibleDuration - newVisibleDuration) > 0) {
                    _visibleDuration = newVisibleDuration;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            CurrentAnimation.Looping = EditorGUILayout.Toggle("Looping", CurrentAnimation.Looping);
            EditorGUILayout.LabelField("Current Time: " + _currentTime.ToString("F2"));
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            var barPosition = position;
            _currentTime = GUILayout.HorizontalSlider(_currentTime, 0, 
                _currentAnimation.MaxDuration, "box", "box", GUILayout.Height(40), GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck()) {
                
            }
            var centerStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            centerStyle.alignment = TextAnchor.UpperCenter;
            for (int i = 0; i <= 20; i++) {
                var percent = i * 0.05f * _visibleScale;
                if (percent > 1) {
                    break;
                }
                DrawTrackLabel(barPosition, percent , centerStyle);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(50);
            GUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth *= 1.25f;
            var lastRect = GUILayoutUtility.GetLastRect();
            if (lastRect.yMax <= 1f) {
                return;
            }
            var trackOffset = Mathf.FloorToInt(_vScrollPosition / _boxHeight);
            PerformDrag(lastRect.yMax, _visibleDuration, trackOffset);
            DrawObjects(objs, lastRect.yMax, _visibleDuration, _visibleOffset, trackOffset);
            DrawTimeline(lastRect.yMax);
            var vScrollVisible = CurrentAnimation.MaxTracks * _boxHeight > position.height;
            var tempMaxDuration = vScrollVisible ? CurrentAnimation.MaxDuration + 0.1f : CurrentAnimation.MaxDuration;
            var hScrollVisible = _visibleDuration < tempMaxDuration;
            if (vScrollVisible) {
                _vScrollPosition =
                    GUI.VerticalScrollbar(
                        new Rect(
                            position.width - 15f, lastRect.yMax, 15f, position.height - lastRect.yMax - (hScrollVisible ? 15f : 0f)),
                        _vScrollPosition, position.height - lastRect.yMax - _boxHeight, 0f,
                        CurrentAnimation.MaxTracks * _boxHeight);
            }
            else {
                _vScrollPosition = 0f;
            }
            if (hScrollVisible) {
                _hScrollPosition =
                    GUI.HorizontalScrollbar(
                        new Rect(0f, position.height - 15f, position.width - (vScrollVisible ? 15f : 0f), 15f),
                        _hScrollPosition, position.width, 0f, tempMaxDuration * position.width / _visibleDuration);
                _visibleOffset = _hScrollPosition;
            }
            else {
                _hScrollPosition = 0f;
            }
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1) {
                if (_addTrackMenu == null) {
                    _addTrackMenu = new GenericMenu();
                    for (var i = 0; i < _animationObjectTypes.Count; i++) {
                        _addTrackMenu.AddItem(new GUIContent(_animationObjectTypes[i].ToString()), false, CreateContextItem,
                            _animationObjectTypes[i]);
                    }
                }
                _addTrackMenu.ShowAsContext();
                Event.current.Use();
            }
        }


        private void DrawTrackLabel(Rect barPosition, float absPercent, GUIStyle style) {
            var percent = absPercent * _visibleScale;
            var xPos = percent * barPosition.width;
            float offset = LabelOffset;
            if (percent > 0.9f) {
                offset = -20f;
            }
            else if (percent > 0.1f) {
                offset = -LabelOffset;
            }
            var sizeOffset = barPosition.width * 0.0001f;
            GUI.Label(new Rect(xPos + offset, TimelinePositionY, LabelWidth, 20), (_currentAnimation.MaxDuration * percent).ToString("F1"), style);
            GUI.DrawTexture(new Rect(xPos- sizeOffset, NotchPosition, NotchWidth, 20), Texture2D.normalTexture);
        }

        private IEnumerator PlayAnimation() {
            var player = new GenericAnimationHolder(_currentAnimation);
            player.Play();
            _playLimiter.Reset();
            var lastPauseTime = 1f;
            while (_playLimiter.Advance()) {
                var dt = _paused ? 0 : TimeManager.DeltaUnscaled;
                _currentTime += dt;
                if (_paused && Math.Abs(_currentTime - lastPauseTime) > 0.0001f) {
                    lastPauseTime = _currentTime;
                    player.SetTime(_currentTime);
                }
                player.Update(dt);
                yield return null;
            }
            _playTask = null;
        }

        private void OnSelectionChange() {
            var newAnim = Selection.objects.Length == 1 ? Selection.activeObject as GenericAnimation : null;
            if (newAnim == null) {
                return;
            }
            CurrentAnimation = newAnim;
            if (_objectWindow != null) {
                _objectWindow.Close();
            }
            foreach (var track in CurrentAnimation.Objects) {
                var newList = CurrentAnimation.Objects.ToList();
                newList.Remove(track);
                foreach (var otherAction in newList) {
                    if (otherAction.EditingTrack == track.EditingTrack &&
                        (track.StartTime >= otherAction.StartTime && track.StartTime <= otherAction.EndTime ||
                         track.EndTime >= otherAction.StartTime && track.EndTime <= otherAction.EndTime)) {
                        otherAction.EditingTrack++;
                    }
                }
            }
            Repaint();
        }

        private void PerformDrag(float offset, float visibleDuration, int trackOffset) {
            if (_draggedObject != null && GUIUtility.hotControl == _myControlId && Event.current.rawType == EventType.MouseUp) {
//                if (Application.isPlaying) {
//                    var newVals = new TempObjectValues {
//                        StartTime = _draggedObject.StartTime,
//                        Duration = _draggedObject.Duration,
//                        EditingTrack = _draggedObject.EditingTrack
//                    };
//                    var id = _draggedObject.GetInstanceID();
//                    if (_playTimeChanges.ContainsKey(id)) {
//                        _playTimeChanges[id] = newVals;
//                    }
//                    else {
//                        _playTimeChanges.Add(id, newVals);
//                    }
//                    Repaint();
//                }
//                else {
//                    
//                }
                EditorUtility.SetDirty(_currentAnimation);
                _draggedObject = null;
            }
            if (_draggedObject != null) {
                DragAction(_draggedObject, visibleDuration, offset, trackOffset);
            }
        }

        private void RemoveContextItem(object obj) {
            var track = obj as AnimationObject;
            if (track == null) {
                return;
            }
            CurrentAnimation.Remove(track);
            DestroyImmediate(track, true);
            EditorUtility.SetDirty(_currentAnimation);
            Repaint();
        }

        private static float RoundToPoint1(float value) {
            return (float) Math.Round(value * 100, MidpointRounding.AwayFromZero) / 100;
        }

        public void Set(GenericAnimation animation) {
            CurrentAnimation = animation;
        }

        public static Texture2D MakeTex(int width, int height, Color col) {
            var pix = new Color[width * height];
            for (var i = 0; i < pix.Length; ++i)
                pix[i] = col;
            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        [MenuItem("Window/Custom Animation Editor %#L")]
        public static CustomAnimationWindow ShowWindow() {
            var window = (CustomAnimationWindow) GetWindow(typeof(CustomAnimationWindow), false, "Custom Animation Window");
            _myControlId = window.GetInstanceID();
            return window;
        }
    }
}