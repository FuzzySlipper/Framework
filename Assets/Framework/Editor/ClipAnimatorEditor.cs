using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace PixelComrades {
    [CustomEditor(typeof(ClipAnimator), true)]
    public class ClipAnimatorEditor : Editor {
        private static AnimationClipState _currentAnimation;
        private static PlayType _playType = PlayType.Limited;
        private static float _forceTime = 0;
        private static float _animationTime;
        private static bool _canPlay = true;
        private static int _forceFrame = 0;
        private static Texture2D _selectedBackground = null;
        private static Texture2D _errorBackground = null;
        private static Texture2D SelectedBackground {
            get {
                if (_selectedBackground == null) {
                    _selectedBackground = new Texture2D(_backgroundRez, _backgroundRez);
                    for (int i = 0; i < _backgroundRez; i++) {
                        _selectedBackground.SetPixel(_backgroundRez, _backgroundRez, Color.green);
                    }
                }
                return _selectedBackground;
            }
        }
        private static Texture2D ErrorBackground {
            get {
                if (_errorBackground == null) {
                    _errorBackground = new Texture2D(_backgroundRez, _backgroundRez);
                    for (int i = 0; i < _backgroundRez; i++) {
                        _errorBackground.SetPixel(_backgroundRez, _backgroundRez, Color.green);
                    }
                }
                return _errorBackground;
            }
        }

        private static int _backgroundRez = 16;

        private enum PlayType {
            Limited,
            Normal,
            Frame,
            Percent,
            NormalLimited,
        }

        private ClipAnimator _script;

        public float DefaultFps = 12;
        public float MinimumTime = 0.35f;

        public override void OnInspectorGUI() {
            _script = (ClipAnimator) target;
            if (_script.Animator == null) {
                _script.Animator = _script.gameObject.GetComponent<Animator>();
                if (_script.Animator == null) {
                    EditorGUILayout.LabelField("No Valid Animator");
                    DrawDefaultInspector();
                    return;
                }
            }
            EditorGUIUtility.labelWidth *= 0.5f;
            EditorGUI.BeginChangeCheck();
            var normal = new GUIStyle("Box");
            var selected = new GUIStyle("Box");
            selected.normal.background = SelectedBackground;
            var animationLabels = PlayerAnimationIds.GetNames().ToArray();
            var events = AnimationEvents.GetNames().ToArray();
            for (int i = 0; i < _script.Clips.Length; i++) {
                var state = _script.Clips[i];
                if (state.Clip != null) {
                    state.ClipName = state.Clip.name;
                }
                EditorGUILayout.BeginVertical(_currentAnimation == state ? selected : normal);
                EditorGUILayout.BeginHorizontal();
                var newClip = EditorGUILayout.ObjectField(state.Clip, typeof(AnimationClip), false);
                if (newClip != null && newClip is AnimationClip newAnimationClip && newAnimationClip != state.Clip) {
                    state.Clip = newAnimationClip;
                }
                //EditorGUILayout.LabelField(state.Clip.name, EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                var index = System.Array.IndexOf(animationLabels, state.Id);
                //var index = labels.IndexOf(state.Id);
                var newIndex =  EditorGUILayout.Popup(index, animationLabels);
                if (newIndex >= 0) {
                    state.Id = animationLabels[newIndex];
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Fps:");
                state.Fps = EditorGUILayout.IntSlider((int)state.Fps, 1, 60);
                EditorGUILayout.EndHorizontal();

                //EditorGUILayout.BeginHorizontal();
                //EditorGUILayout.LabelField("Event:");
                //state.PercentEvent = EditorGUILayout.Slider(state.PercentEvent, 0, 1);
                //EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Length Multi:");
                state.PlaySpeedMultiplier = EditorGUILayout.Slider(state.PlaySpeedMultiplier, 0.1f, 5);
                EditorGUILayout.LabelField((state.ClipLength/state.PlaySpeedMultiplier).ToString("F1"));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Loop: " + state.Clip?.isLooping);
                if (GUILayout.Button("Delete")) {
                    if (EditorUtility.DisplayDialog("", "Are you sure?", "Yes", "No")) {
                        var list = _script.Clips.ToList();
                        list.RemoveAt(i);
                        _script.Clips = list.ToArray();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (_currentAnimation == null) {
                    if (GUILayout.Button("Play")) {
                        TimeManager.StartUnscaled(PlayAnimation(_script, state));
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Events: " + state.EventTotal());
                    
                }
                else if (_currentAnimation == state) {
                    if (GUILayout.Button("Stop")) {
                        Stop();
                    }
                    var inputEvent = Event.current;
                    if (inputEvent.type == EventType.KeyDown && inputEvent.shift) {
                        switch (inputEvent.keyCode) {
                            case KeyCode.Alpha1:
                                _playType = (PlayType) 0;
                                KeyUsed(inputEvent);
                                break;
                            case KeyCode.Alpha2:
                                _playType = (PlayType) 1;
                                KeyUsed(inputEvent);
                                break;
                            case KeyCode.Alpha3:
                                _playType = (PlayType) 2;
                                KeyUsed(inputEvent);
                                break;
                            case KeyCode.Alpha4:
                                _playType = (PlayType) 3;
                                KeyUsed(inputEvent);
                                break;
                        }
                    }
                    _playType = (PlayType) EditorGUILayout.EnumPopup(_playType);
                    switch (_playType) {
                        case PlayType.Percent:
                            _forceTime = EditorGUILayout.Slider(_forceTime, 0, state.ClipLength);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Frame " + _currentFrame);
                            break;
                        case PlayType.Frame:
                            state.CheckArraysLength();
                            _forceFrame = EditorGUILayout.IntSlider(_forceFrame, 0, state.RenderFrames.Length - 1);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            var width = EditorGUIUtility.labelWidth;
                            EditorGUIUtility.labelWidth = width * 0.2f;
                            var frameLayoutOptions = new GUILayoutOption[] {
                                GUILayout.ExpandWidth(false), GUILayout.Width(50)
                            };
                            state.RenderFrames[_forceFrame] = EditorGUILayout.Toggle("R: ", state.RenderFrames[_forceFrame], frameLayoutOptions);
                            state.FrameLengths[_forceFrame] = EditorGUILayout.FloatField("L: ", state.FrameLengths[_forceFrame], frameLayoutOptions);
                            EditorGUIUtility.labelWidth = width;
                            var eventIndex = System.Array.IndexOf(events, state.Events[_forceFrame]);
                            var newEventIndex = EditorGUILayout.Popup(eventIndex, events);
                            if (eventIndex != newEventIndex) {
                                state.Events[_forceFrame] = events[newEventIndex];
                            }
                            var textBg = new GUIStyle();
                            if (!string.IsNullOrEmpty(state.Events[_forceFrame])) {
                                textBg.normal.background = ErrorBackground;
                            }
                            state.Events[_forceFrame] = EditorGUILayout.TextArea(state.Events[_forceFrame], textBg);

                            break;
                        default:
                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Frame " + _currentFrame);
                            break;
                    }
                    if (inputEvent.type == EventType.KeyDown && _playType == PlayType.Frame) {
                        switch (inputEvent.keyCode) {
                            case KeyCode.F:
                                state.RenderFrames[_forceFrame] = !state.RenderFrames[_forceFrame];
                                KeyUsed(inputEvent);
                                break;
                            case KeyCode.W:
                                _forceFrame++;
                                if (_forceFrame > state.RenderFrames.Length - 1) {
                                    _forceFrame = 0;
                                }
                                KeyUsed(inputEvent);
                                break;
                            case KeyCode.S:
                                _forceFrame--;
                                if (_forceFrame < 0) {
                                    _forceFrame = state.RenderFrames.Length -1;
                                }
                                KeyUsed(inputEvent);
                                break;
                            case KeyCode.D:
                                state.FrameLengths[_forceFrame] += 0.25f;
                                KeyUsed(inputEvent);
                                break;
                            case KeyCode.A:
                                state.FrameLengths[_forceFrame] -= 0.25f;
                                KeyUsed(inputEvent);
                                break;
                            case KeyCode.X:
                                state.FrameLengths[_forceFrame] = 1;
                                KeyUsed(inputEvent);
                                break;
                        }
                    }
                    //EditorGUILayout.LabelField(_displayTime.ToString("F1"));
                }
                else {
                    if (GUILayout.Button("Play")) {
                        Stop();
                        TimeManager.StartUnscaled(PlayAnimation(_script, state));
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Playing " + _currentAnimation?.ClipName);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                //EditorGUILayout.Space();
                //EditorGUILayout.Space();
            }
            //if (GUILayout.Button("Add Animation State")) {
            //    _script.AnimationStates.Add(new AnimationState());
            //}
            //EditorGUILayout.Space();
            DropAreaGUI();
            
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(_script, _script.name);
                EditorUtility.SetDirty(_script);
            }
            EditorGUIUtility.labelWidth *= 2;
            if (GUILayout.Button("Save")) {
                var saveLocation = Application.dataPath + "/GameData";
                var path = EditorUtility.SaveFilePanel("Save", saveLocation, "Layout", "json");
                if (!string.IsNullOrEmpty(path) && path.Length > 0) {
                    SaveFile(path);
                }
            }
            if (GUILayout.Button("Load")) {
                var saveLocation = Application.dataPath + "/GameData";
                var path = EditorUtility.OpenFilePanel("Load", saveLocation, "json");
                if (!string.IsNullOrEmpty(path) && path.Length > 0) {
                    LoadFile(path);
                }
            }
            if (GUILayout.Button("Clear")) {
                if (EditorUtility.DisplayDialog("Are you sure?", "", "Yes", "No")){
                    _script.Clips = new AnimationClipState[0];
                }
            }
            DrawDefaultInspector();
            if (AnimationMode.InAnimationMode()) {
                if (GUILayout.Button("End Animation Mode")) {
                    AnimationMode.StopAnimationMode();
                    _canPlay = false;
                }
            }
        }

        private void KeyUsed(Event inputEvent) {
            inputEvent.Use();
            Undo.RecordObject(_script, _script.name);
            EditorUtility.SetDirty(_script);
        }

        private void Stop() {
            _canPlay = false;
            Undo.RecordObject(_script, _script.name);
            EditorUtility.SetDirty(_script);
        }

        private void ProcessClip(AnimationClip clip) {
            if (clip == null) {
                return;
            }
            Debug.Log("Processing " + clip.name);
            if (_script.GetState(clip) != null) {
                return;
            }
            var list = _script.Clips.ToList();
            
            list.Add(new AnimationClipState() {
                ClipName =  clip.name,
                Clip = clip,
                Fps = DefaultFps,
                PlaySpeedMultiplier = 1,
            });
            _script.Clips = list.ToArray();
        }

        private static int _currentFrame;

        private IEnumerator PlayAnimation(ClipAnimator animator, AnimationClipState state) {
            if (!Application.isPlaying) {
                AnimationMode.StartAnimationMode();
            }
            int limiter = 0;
            _animationTime = 0;
            _canPlay = true;
            _currentAnimation = state;
            _currentFrame = 0;
            UnscaledTimer normalLimited = null;
            int lastFrame = 999;
            var weaponTrails = animator.GetComponentInChildren<WeaponModel>(true);
            while (limiter < 99999) {
                limiter++;
                if (_currentAnimation == null || !_canPlay) {
                    break;
                }
                switch (_playType) {
                    case PlayType.Percent:
                        _animationTime = _forceTime;
                        break;
                    case PlayType.Normal:
                    case PlayType.Limited:
                    case PlayType.NormalLimited:
                        _animationTime += TimeManager.DeltaUnscaled * state.PlaySpeedMultiplier;
                        break;
                    case PlayType.Frame:
                        //_animationTime = state.ConvertFrameToAnimationTime(_currentFrame);
                        _currentFrame = _forceFrame;
                        break;
                }
                switch (_playType) {
                    case PlayType.Limited:
                    case PlayType.Percent:
                    case PlayType.Normal:
                    case PlayType.NormalLimited:
                        if (_animationTime > state.ClipLength) {
                            _animationTime -= state.ClipLength;
                        }
                        _currentFrame = _currentAnimation.CalculateCurrentFrame(_animationTime);
                        switch (_playType) {
                            case PlayType.Percent:
                            case PlayType.Limited:
                                if (_currentFrame < 0) {
                                    _animationTime = 0;
                                    continue;
                                }
                                break;
                        }
                        break;
                }
                switch (_playType) {
                        default:
                            if (_currentFrame != lastFrame) {
                                _currentAnimation.Play(animator.Animator, _currentAnimation.ConvertFrameToAnimationTime(_currentFrame));
                                if (weaponTrails != null) {
                                    if (state.Events[_currentFrame] == AnimationEvents.FxOn) {
                                        weaponTrails.SetFx(true);
                                    }
                                    else if (state.Events[_currentFrame] == AnimationEvents.FxOff) {
                                        weaponTrails.SetFx(false);
                                    }
                                }
                            }
                            break;
                        case PlayType.Normal:
                            _currentAnimation.Play(animator.Animator, _animationTime);
                            break;
                        case PlayType.NormalLimited:
                            if (normalLimited == null) {
                                normalLimited = new UnscaledTimer(1/state.Fps);
                            }
                            if (!normalLimited.IsActive) {
                                _currentAnimation.Play(animator.Animator, _animationTime);
                                normalLimited.StartNewTime(1 / state.Fps);
                            }
                            break;
                }
                lastFrame = _currentFrame;
                SceneView.RepaintAll();
                yield return null;
            }
            if (!Application.isPlaying) {
                AnimationMode.StopAnimationMode();
            }
            _currentAnimation = null;
        }

        private void SaveFile(string path) {
            if (path.Length == 0) {
                return;
            }
            var save = JsonConvert.SerializeObject(_script.Clips, Formatting.Indented, Serializer.ConverterSettings);
            FileUtility.SaveFile(path, save);
        }

        private void LoadFile(string path) {
            string text = FileUtility.ReadFile(path);
            if (string.IsNullOrEmpty(text)) {
                return;
            }
            var data = JsonConvert.DeserializeObject<AnimationClipState[]>(text, Serializer.ConverterSettings);
            if (data == null) {
                Debug.LogErrorFormat("Error deserializing layout {0}", path);
                return;
            }
            var oldClips = _script.Clips.ToList();
            for (int i = 0; i < data.Length; i++) {
                var loadedData = data[i];
                for (int o = 0; o < oldClips.Count; o++) {
                    if (oldClips[o].ClipName == loadedData.ClipName) {
                        data[i].Clip = oldClips[o].Clip;
                        oldClips.RemoveAt(o);
                        break;
                    }
                }
            }
            oldClips.AddRange(data);
            _script.Clips = oldClips.ToArray();
        }

        private Bounds GetBounds() {
            //gameObject.transform.localScale = Vector3.one;
            //gameObject.transform.position = Vector3.zero;
            var bounds = new Bounds();
            var renderers = _script.gameObject.GetComponentsInChildren<Renderer>(false);
            foreach (var renderer in renderers) {
                if (renderer is ParticleSystemRenderer) {
                    continue;
                }
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

        //private AnimationClip GetClip(AnimatorController controller, string clip) {
        //    var states = controller.layers[0].stateMachine.states;
        //    for (int i = 0; i < states.Length; i++) {
        //        if (states[i].state.name == clip) {
        //            return states[i].state.motion as AnimationClip;
        //        }
        //    }
        //    return null;
        //}

        
        private void DropAreaGUI() {
            Event evt = Event.current;
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 25.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Add Clip");
            switch (evt.type) {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition)) {
                        return;
                    }
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (evt.type == EventType.DragPerform) {
                        DragAndDrop.AcceptDrag();
                        for (int i = 0; i < DragAndDrop.paths.Length; i++) {
                            var path = DragAndDrop.paths[i];
                            CheckAtPath(path);
                        }
                        for (int i = 0; i < DragAndDrop.objectReferences.Length; i++) {
                            var folder = DragAndDrop.objectReferences[i] as DefaultAsset;
                            if (folder == null) {
                                continue;
                            }
                            var path = AssetDatabase.GetAssetPath(folder);
                            path = path.Remove(0, 7);
                            var clips = GetAssetList<AnimationClip>(path);
                            for (int c = 0; c < clips.Count; c++) {
                                ProcessClip(clips[c]);
                            }
                            //var playables = GetAssetList<PlayableAsset>(path);
                            //for (int c = 0; c < playables.Count; c++) {
                            //    ProcessClip(playables[c]);
                            //}
                        }
                    }
                    break;
            }
            DefaultFps = EditorGUILayout.IntSlider("Default FPS", (int) DefaultFps, 1, 24);
        }

        private List<T> GetAssetList<T>(string path) where T : class {
            string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);
            return fileEntries.Select(
                    fileName => {
                        string temp = fileName.Replace("\\", "/");
                        int index = temp.LastIndexOf("/");
                        string localPath = "Assets/" + path;

                        if (index > 0)
                            localPath += temp.Substring(index);

                        return AssetDatabase.LoadAssetAtPath(localPath, typeof(T));
                    })
                //Filtering null values, the Where statement does not work for all types T
                .OfType<T>() //.Where(asset => asset != null)
                .ToList();
        }

        private void CheckAtPath(string path) {
            //var modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;
            //if (modelImporter != null) {
            //    for (int c = 0; c < modelImporter.clipAnimations.Length; c++) {
            //        ProcessClip(modelImporter.clipAnimations[c]);
            //    }
            //}
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null) {
                ProcessClip(clip);
            }
            //var playable = AssetDatabase.LoadAssetAtPath<PlayableAsset>(path);
            //if (playable != null) {
            //    ProcessClip(playable);
            //}
        }
    }
}
