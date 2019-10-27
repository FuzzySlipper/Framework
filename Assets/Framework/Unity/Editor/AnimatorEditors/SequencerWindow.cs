using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PixelComrades {
    public class SequencerWindow : EditorWindow {

        [MenuItem("Window/Sequencer Window %#L")]
        public static SequencerWindow ShowWindow() {
            var window = (SequencerWindow) GetWindow(typeof(SequencerWindow), false, "Sequencer Window");
            window.Init();
            _myControlId = window.GetInstanceID();
            return window;
        }

        
        private const float LabelWidth = 30;
        private const float LabelOffset = 5f;
        private const float NotchWidth = 5f;
        private const float HandleWidth = 10f;
        private static int _myControlId;
        //private readonly Dictionary<int, TempObjectValues> _playTimeChanges = new Dictionary<int, TempObjectValues>();
        private readonly List<SequenceObject> _selectedObjects = new List<SequenceObject>();
        private SequenceObjectWindow _objectWindow;
        private GenericMenu _addTrackMenu;
        private bool _batchSelect;
        private GenericSequence _currentSequence;
        private SequenceObject _draggedObject;
        private bool _endHandleDragged;
        private float _hScrollPosition;
        private bool _startHandleDragged;
        private float _startPositionX;
        private float _visibleDuration;
        private float _vScrollPosition;
        private int _boxHeight = 100;
        private float _visibleOffset;
        private float _visibleScale = 1f;
        
        protected float CurrentTime;
        protected bool Paused = false;
        protected WhileLoopLimiter PlayLimiter = new WhileLoopLimiter(99999);
        protected bool CancelPlay = false;
        protected Task PlayTask;
        protected List<Type> AnimationObjectTypes = new List<Type>();
        private GenericSequence CurrentSequence { get => _currentSequence; set => _currentSequence = value; }
        protected virtual float TimelinePositionY { get { return 25f; }}
        private float NotchPosition { get { return TimelinePositionY + 18f; } }
        protected virtual void Init() {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int a = 0; a < assemblies.Length; a++) {
                var types = assemblies[a].GetTypes();
                for (int t = 0; t < types.Length; t++) {
                    var type = types[t];
                    if (type.IsSubclassOf(typeof(SequenceObject))) {
                        AnimationObjectTypes.Add(type);
                    }
                }
            }
        }
        
        void OnDestroy() {
            if (PlayTask != null) {
                CancelPlay = true;
                PlayTask = null;
            }
        }

        public void Set(GenericSequence sequence) {
            CurrentSequence = sequence;
        }
        
        protected virtual void OnGUI() {
            if (Application.isPlaying) {
                if (RuntimeSequence.DebugSequence != null && _currentSequence != RuntimeSequence.DebugSequence.Sequence) {
                    Set(RuntimeSequence.DebugSequence.Sequence);
                }
            }
            if (CurrentSequence == null) {
                CheckSequence();
                return;
            }
            var objs = CurrentSequence.Objects;
            var newBatchSelect = Event.current.command || Event.current.control;
            if (_batchSelect != newBatchSelect) {
                _batchSelect = newBatchSelect;
                Repaint();
            }
            if (Event.current.type == EventType.ScrollWheel) {
                _visibleScale = Mathf.Clamp(_visibleScale + Mathf.Sign(Event.current.delta.y) * 0.1f, 0.1f, 100);
                Repaint();
            }
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            DrawToolbar();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(50);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            DrawEditLine();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
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
            var vScrollVisible = CurrentSequence.MaxTracks * _boxHeight > position.height;
            var tempMaxDuration = vScrollVisible ? CurrentSequence.MaxDuration + 0.1f : CurrentSequence.MaxDuration;
            var hScrollVisible = _visibleDuration < tempMaxDuration;
            if (vScrollVisible) {
                _vScrollPosition =
                    GUI.VerticalScrollbar(
                        new Rect(
                            position.width - 15f, lastRect.yMax, 15f, position.height - lastRect.yMax - (hScrollVisible ? 15f : 0f)),
                        _vScrollPosition, position.height - lastRect.yMax - _boxHeight, 0f,
                        CurrentSequence.MaxTracks * _boxHeight);
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
                    for (var i = 0; i < AnimationObjectTypes.Count; i++) {
                        _addTrackMenu.AddItem(new GUIContent(AnimationObjectTypes[i].ToString()), false, CreateContextItem,
                            AnimationObjectTypes[i]);
                    }
                }
                _addTrackMenu.ShowAsContext();
                Event.current.Use();
            }
        }

        protected virtual void CheckSequence() {
            GUILayout.Label("Select Sequence");
            if (Selection.objects.Length == 1) {
                OnSelectionChange();
            }
        }

        protected virtual void DrawToolbar() {
            if (!Application.isPlaying) {
                if (PlayTask == null) {
                    if (GUILayout.Button("PLAY")) {
                        Play();
                    }
                }
                else {
                    if (GUILayout.Button("STOP")) {
                        CancelPlay = true;
                        PlayTask = null;
                    }
                    Paused = GUILayout.Toggle(Paused, "Paused");
                }
            }
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth *= 0.75f;
            _visibleScale = EditorGUILayout.Slider("Scale:", _visibleScale, 0.1f, 10f);
            CurrentSequence.MaxDuration = EditorGUILayout.FloatField(
                "Max duration:",
                CurrentSequence.MaxDuration);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            CurrentSequence.MaxTracks = EditorGUILayout.IntField("Max tracks:", CurrentSequence.MaxTracks);
            _boxHeight = EditorGUILayout.IntSlider("Track height:", _boxHeight, 50, 150);
            if (_draggedObject == null) {
                var newVisibleDuration = CurrentSequence.MaxDuration / _visibleScale;
                if (Math.Abs(_visibleDuration - newVisibleDuration) > 0) {
                    _visibleDuration = newVisibleDuration;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            CurrentSequence.Looping = EditorGUILayout.Toggle("Looping", CurrentSequence.Looping);
            EditorGUILayout.LabelField("Current Time: " + CurrentTime.ToString("F2"));
            GUILayout.EndHorizontal();
        }

        protected virtual void DrawEditLine() {
            var barPosition = position;
            var currentTime = GUILayout.HorizontalSlider(
                CurrentTime, 0,
                _currentSequence.MaxDuration, "box", "box", GUILayout.Height(40), GUILayout.ExpandWidth(true));
            if (Application.isPlaying && RuntimeSequence.DebugSequence != null) {
                CurrentTime = RuntimeSequence.DebugSequence.CurrentTime;
            }
            else {
                CurrentTime = currentTime;
            }
            var centerStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            centerStyle.alignment = TextAnchor.UpperCenter;
            for (int i = 0; i <= 20; i++) {
                var percent = i * 0.05f * _visibleScale;
                if (percent > 1) {
                    break;
                }
                DrawTrackLabel(barPosition, percent, centerStyle);
            }
        }

        protected virtual void Play() {
            TimeManager.ForceCheckEditor();
            PlayTask = TimeManager.StartUnscaled(PlayAnimation());
        }

        private void CreateContextItem(object obj) {
            var targetType = obj as Type;
            if (targetType == null) {
                return;
            }
            var newObj = CreateInstance(targetType);
            AssetDatabase.AddObjectToAsset(newObj, _currentSequence);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newObj));
            var animObj = newObj as SequenceObject;
            CurrentSequence.Add(animObj);
            FindOpenStartTime(animObj);
            Repaint();
        }

        private void FindOpenStartTime(SequenceObject animObject) {
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

        private bool Intersects(SequenceObject dragged, int track, float start, float duration) {
            var endTime = start + duration;
            for (int i = 0; i < _currentSequence.Objects.Count; i++) {
                var obj = _currentSequence.Objects[i];
                if (obj == dragged || obj.EditingTrack != track) {
                    continue;
                }
                if (obj.StartTime < endTime && start < obj.EndTime) {
                    return true;
                }
            }
            return false;
        }
        
        private void DragAction(SequenceObject dragged, float visibleDuration, float offset, int trackOffset) {
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
            newDuration = Mathf.Abs(newDuration);
            if (dragged.StartTime != newStartTime || dragged.Duration != newDuration || dragged.EditingTrack != newEditingTrack) {
                if (Intersects(dragged, newEditingTrack, newStartTime, newDuration) || newStartTime < 0) {
                    return;
                }
                if (newStartTime + newDuration > CurrentSequence.MaxDuration) {
                    CurrentSequence.MaxDuration = newStartTime + newDuration;
                }
                foreach (var selectedTrack in _selectedObjects.Except(new[] {dragged})) {
                    selectedTrack.StartTime -= dragged.StartTime - newStartTime;
                    selectedTrack.Duration -= dragged.Duration - dragged.Duration;
                    selectedTrack.EditingTrack += newEditingTrack - dragged.EditingTrack;
                }
                dragged.StartTime = newStartTime;
                dragged.Duration = newDuration;
                dragged.EditingTrack = newEditingTrack;
                EditorUtility.SetDirty(dragged);
                Repaint();
            }
        }

        private void DrawObjects(List<SequenceObject> objs, float offset, float duration, float hOffset, int trackOffset) {
            var maxVisibleTracks = position.height / _boxHeight + 1;
            var maxTracks = CurrentSequence.MaxTracks;
            for (var i = 0; i < (maxTracks < maxVisibleTracks ? maxTracks : maxVisibleTracks); i++) {
                var trackStyle = new GUIStyle(GUI.skin.box) {
                    normal = {background = TextureUtilities.MakeTexture(2, 2, new Color(0f, 0f, 0f, 0.1f))},
                    name = "Track"
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
                Rect boxRect;
                bool startHandle = false, endHandle = false, mainHandle = false;
                if (obj.CanResize) {
                    boxRect = new Rect(
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
                    startHandle = boxStartHandleRect.Contains(Event.current.mousePosition);
                    endHandle = boxEndHandleRect.Contains(Event.current.mousePosition);
                }
                else {
                    boxRect = new Rect(horizontalPosStart, offset + _boxHeight * (obj.EditingTrack - trackOffset), width,_boxHeight);
                }
                obj.DrawTimelineGui(boxRect);
                mainHandle = boxRect.Contains(Event.current.mousePosition);
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
                                _objectWindow = (SequenceObjectWindow) GetWindow(typeof(SequenceObjectWindow), true, "Object Editor");
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
            if (PlayTask != null) {
                var horizontalPosStart =
                    position.width * (CurrentTime / _visibleDuration) - _visibleOffset;
                var timeRect = new Rect(horizontalPosStart, yMax, 1f, position.height - yMax);
                GUI.Box(timeRect, string.Empty);
                Repaint();
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
            GUI.Label(new Rect(xPos + offset, TimelinePositionY, LabelWidth, 20), (_currentSequence.MaxDuration * percent).ToString("F2"), style);
            GUI.DrawTexture(new Rect(xPos- sizeOffset, NotchPosition, NotchWidth, 20), Texture2D.normalTexture);
        }

        private IEnumerator PlayAnimation() {
            CancelPlay = false;
            var entity = Entity.New("Tester");
            var player = new RuntimeSequence(entity,  _currentSequence);
            entity.Add(new PoseAnimatorComponent(PoseAnimator.Main.Avatar, PoseAnimator.Main.DefaultPose, PoseAnimator.Main.transform));
            player.Play();
            PlayLimiter.Reset();
            var lastPauseTime = 1f;
            while (PlayLimiter.Advance()) {
                if (CancelPlay) {
                    break;
                }
                var dt = Paused ? 0 : TimeManager.DeltaUnscaled;
                CurrentTime += dt;
                if (Paused && Math.Abs(CurrentTime - lastPauseTime) > 0.0001f) {
                    lastPauseTime = CurrentTime;
                    player.SetTime(CurrentTime);
                }
                player.Update(dt);
                if (player.IsComplete && !Paused) {
                    CurrentTime = 0;
                    if (!_currentSequence.Looping) {
                        break;
                    }
                    player.Play();
                }
                yield return null;
            }
            entity.Destroy();
            PlayTask = null;
        }

        private void OnSelectionChange() {
            var newAnim = Selection.objects.Length == 1 ? Selection.activeObject as GenericSequence : null;
            if (newAnim == null) {
                return;
            }
            CurrentSequence = newAnim;
            if (_objectWindow != null) {
                _objectWindow.Close();
            }
            foreach (var track in CurrentSequence.Objects) {
                var newList = CurrentSequence.Objects.ToList();
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
                EditorUtility.SetDirty(_currentSequence);
                _draggedObject = null;
            }
            if (_draggedObject != null) {
                DragAction(_draggedObject, visibleDuration, offset, trackOffset);
            }
        }

        private void RemoveContextItem(object obj) {
            var track = obj as SequenceObject;
            if (track == null) {
                return;
            }
            CurrentSequence.Remove(track);
            DestroyImmediate(track, true);
            EditorUtility.SetDirty(_currentSequence);
            Repaint();
        }

        private static float RoundToPoint1(float value) {
            return (float) Math.Round(value * 100, MidpointRounding.AwayFromZero) / 100;
        }
    }
}