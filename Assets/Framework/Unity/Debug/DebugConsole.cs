using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
#if USE_LUA
using XLua;
#endif

namespace PixelComrades {

    public class DebugConsole : MonoBehaviour {

        [Header("References")]
        [SerializeField] private InputField _commandInput = null;
        [SerializeField] private Text _historyText = null;
        [SerializeField] private Image _commandInputBackgroundImage = null;
        [SerializeField] private Image _historyBackgroundImage = null;
        [Header("Config")]
        [SerializeField] private KeyCode _closedWindowKey = KeyCode.Escape;
        [SerializeField] private KeyCode _smallWindowKey = KeyCode.F1;
        [SerializeField] private KeyCode _fullWindowKey = KeyCode.F2;
        [SerializeField] private float _positionLerpTime = 0.5f;
        [SerializeField] private float _smallWindowPosition = 0.8f;
        [SerializeField] private float _outputLogTextSize = 0.8f;
        [SerializeField] private float _caretColorLerpSpeed = 5;
        [SerializeField] private float _closedWindowPosition = 1f;
        [SerializeField] private float _fullWindowPosition = 0;
        [SerializeField] private int _caretWidth = 10;
        [SerializeField] private char _navigationIndicator = '>';
        [Header("Colors")]
        [SerializeField] private Color _tertiaryCaretColor = new Color(64f / 255f, 64f / 255f, 64f / 255f, 200f / 255f);
        [SerializeField] private Color _navigationIndicatorColor = new Color(1f, 213f / 255f, 75f / 255f, 1f);
        [SerializeField] private Color _outputLogErrorColor = new Color(163f / 255f, 0f, 0f, 200f / 255f);
        [SerializeField] private Color _outputLogInfoColor = new Color(0.5f, 0.5f, 0.5f, 200f / 255f);
        [SerializeField] private Color _outputLogWarningColor = new Color(225f / 255f, 225f / 255f, 0f, 200f / 255f);
        [SerializeField] private Color _primaryCaretColor = Color.white;
        [SerializeField] private Color _secondaryCaretColor = new Color(128f / 255f, 128f / 255f, 128f / 255f, 200f / 255f);
        [SerializeField] private Color _commandInputColor = new Color(1f, 1f, 1f, 200f / 255f);
        [SerializeField] private Color _historyBackgroundColor = new Color(0f, 0f, 0f, 150f / 255f);
        [SerializeField] private Color _commandHistoryColor = new Color(1f, 1f, 1f, 200f / 255f);
        [SerializeField] private Color _commandInputBackgroundColor = new Color(0f, 0f, 0f, 200f / 255f);

        private static Dictionary<string, System.Func<string[], string>> _commands = new Dictionary<string, System.Func<string[], string>>();
        private Color _targetCaretColor;
        private RectTransform _tf;
        private float _targetPosition;
        private int _commandIndex;
        private bool _navigating;
        private List<Entry> _historyList = new List<Entry>();
#if USE_LUA
        private LuaEnv _lua;
#endif
        private bool _opened;

        public event Action<string> OnCommandEnter;
        public event System.Action OnConsoleClosed;
        public event System.Action OnConsoleOpened;


        public static void RegisterCommand(string text, Func<string[], string> cmd) {
            _commands.AddOrUpdate(text, cmd);
        }

        public static void RemoveCommand(string text) {
            _commands.Remove(text);
        }

        void Start() {
            _targetPosition = _closedWindowPosition;
            _tf = GetComponent<RectTransform>();
            _commandInput.caretWidth = _caretWidth;
            _commandInput.onValueChanged.AddListener(OnValueChanged);
            _commandInput.onEndEdit.AddListener(Submit);
            _commandInput.textComponent.color = _commandInputColor;
            _historyText.color = _commandInputColor;
            _commandInputBackgroundImage.color = _commandInputBackgroundColor;
            _historyBackgroundImage.color = _historyBackgroundColor;
#if USE_LUA
            _lua = new LuaEnv();
            _lua.DoString("ec = CS.PixelComrades.EntityController");
            _lua.DoString("GameData = CS.PixelComrades.GameData");
            _lua.DoString("px = CS.PixelComrades");
#endif
            Application.logMessageReceived += InterceptDebugLog;
        }

        void Update() {
            if (Input.GetKeyDown(_smallWindowKey)) {
                SetOpen(true, _smallWindowPosition);
            }
            else if (Input.GetKeyDown(_fullWindowKey)) {
                SetOpen(true, _fullWindowPosition);
            }
            else if (Input.GetKeyDown(_closedWindowKey)) {
                SetOpen(false, _closedWindowPosition);
            }
            if (!_opened) {
                return;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow)) {
                Navigate(-1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow)) {
                Navigate(1);
            }
            float sinLerp = (Mathf.Sin(Time.time * _caretColorLerpSpeed) + 1) / 2;
            _targetCaretColor = Color.Lerp(_secondaryCaretColor, _tertiaryCaretColor, sinLerp);
            if (!_navigating) {
                _commandInput.caretColor = Color.Lerp(_commandInput.caretColor, _targetCaretColor, _caretColorLerpSpeed * TimeManager.DeltaUnscaled);
            }
        }

        private void Navigate(int i) {
            _navigating = true;

            // Hitting the up arrow in a text input snaps the cursor to the beginning of the input and looks bad, so we hide the cursor temporarily.
            _commandInput.caretColor = Color.clear;
            _commandIndex += i;
            if (_commandIndex < 0) {
                _commandIndex = 0;
            }
            if (_commandIndex < _historyList.Count) {
                _commandInput.text = _historyList[_commandIndex].Command;
            }
            else {
                _commandIndex = _historyList.Count;
                _commandInput.text = "";
            }
            UpdateHistoryText();
            StartCoroutine(MoveTextEnd());
        }

        private void SetOpen(bool status, float targetPosition) {
            if (_currentTask != null) {
                _currentTask.Cancel();
            }
            _currentTask = TimeManager.StartUnscaled(ProcessOpen(status, targetPosition));
        }

        private Task _currentTask;

        private IEnumerator ProcessOpen(bool status, float targetPosition) {
            _opened = status;
            PlayerInput.AllInputBlocked = status;
            _targetPosition = status ? targetPosition : _closedWindowPosition;
            yield return null;
            if (_opened) {
                OnConsoleOpened?.Invoke();
                _commandInput.ActivateInputField();
            }
            else {
                OnConsoleClosed?.Invoke();
                _commandInput.DeactivateInputField();
            }
            var min = _tf.anchorMin;
            var max = _tf.anchorMax;
            var lerpHolder = new LerpHolder();
            lerpHolder.RestartLerp(min.y, _targetPosition, _positionLerpTime, true);
            while (!lerpHolder.IsFinished) {
                min.y = lerpHolder.GetLerpValue();
                max.y = min.y + 1;
                _tf.anchorMin = min;
                _tf.anchorMax = max;
                yield return null;
            }
        }

        private void InterceptDebugLog(string text, string stackTrace, LogType type) {
            Color logColor;
            switch (type) {
                case LogType.Log:
                    logColor = _outputLogInfoColor;
                    break;
                case LogType.Warning:
                    logColor = _outputLogWarningColor;
                    break;
                default:
                    logColor = _outputLogErrorColor;
                    break;
            }
            if (_pendingOutput) {
                _historyList.LastElement().Output += "\n    <size=" + _historyText.fontSize * _outputLogTextSize + "><color=\"#" + ColorUtility.ToHtmlStringRGBA(logColor) + "\">" + text + "</color></size>";
                _pendingOutput = false;
            }
            UpdateHistoryText();
        }

        // Wait for next UI update, then put the cursor at the end of the input field
        private IEnumerator MoveTextEnd() {
            yield return new WaitForEndOfFrame();
            _commandInput.MoveTextEnd(false);
            _commandInput.caretColor = _primaryCaretColor;
            _navigating = false;
        }

        // If you start typing with a previous input selected, reset the commandIndex
        private void OnValueChanged(string text) {
            if (!_navigating) {
                _commandInput.caretColor = _primaryCaretColor;
                int oldIndex = _commandIndex;
                _commandIndex = _historyList.Count;
                if (oldIndex != _commandIndex) {
                    UpdateHistoryText();
                }
            }
        }
        
        private bool _pendingOutput = false;

        private void Submit(string text) {
            if (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.KeypadEnter)) {
                return;
            }
            _commandInput.ActivateInputField();
            var command = new Entry();
            command.Command = text;
            _historyList.Add(command);
#if DEBUG
            if (text.ToLower() == "write log" || text.ToLower() == "writelog") {
                StreamWriter writer = new StreamWriter(string.Format("{0}/Log-{1:MM-dd-yy hh-mm-ss}.txt", Application.persistentDataPath, System.DateTime.Now), true);
                writer.Write(DebugLog.Current);
                command.Output = "Wrote Log";
                return;
            }
#endif
            var cmdSplit = text.Split(' ');
            if (cmdSplit.Length > 0 && _commands.TryGetValue(cmdSplit[0], out var del)) {
                command.Output = del(cmdSplit.RemoveAt(0));
                OnCommandEnter?.Invoke(text);
                UpdateHistoryText();
                return;
            }
            _pendingOutput = true;
#if USE_LUA
            _lua.DoString(text);
#endif
            OnCommandEnter?.Invoke(text);
            UpdateHistoryText();
            //command.Output = .ToString();
            //_commandInput.text = "";
            //_historyList.Add(
            //    new Entry {
            //        Command = text
            //    });
            //_commandIndex = _historyList.Count;
            //StartCoroutine(SetFocus(true));
            
            //string entityStr = string.Empty;
            //string componentStr = string.Empty;
            //string methodStr = string.Empty;
            //var args = text.Split(' ');
            //List<object> parameters = new List<object>();
            //for (int i = 0; i < args.Length; i++) {
            //    var arg = args[i];
            //    if (arg == EntityOption) {
            //        i++;
            //        int parsed;
            //        string str = ParseString(text, i, out parsed);
            //        if (parsed < 0) {
            //            Application.logMessageReceived -= InterceptDebugLog;
            //            return;
            //        }
            //        entityStr = str;
            //        i += parsed;
            //    }
            //    else if (arg == ComponentOption) {
            //        componentStr = args[++i];
            //    }
            //    else if (arg == MethodOption) {
            //        methodStr = args[++i];
            //        string next;
            //        while (i < args.Length - 1 && (next = args[++i]) != EntityOption && next != ComponentOption && next != MethodOption) {
            //            int iParse;
            //            float fParse;
            //            bool bParse;
            //            if (int.TryParse(next, out iParse)) {
            //                parameters.Add(iParse);
            //            }
            //            else if (float.TryParse(next, out fParse)) {
            //                parameters.Add(fParse);
            //            }
            //            else if (bool.TryParse(next, out bParse)) {
            //                parameters.Add(bParse);
            //            }
            //            else {
            //                int parsed;
            //                string str = ParseString(text, i, out parsed);
            //                if (parsed < 0) {
            //                    Application.logMessageReceived -= InterceptDebugLog;
            //                    return;
            //                }
            //                i += parsed;
            //                parameters.Add(str);
            //            }
            //        }
            //        i--;
            //    }
            //}
            //Entity targetEntity = null;
            //Component targetComponent = null;
            //MethodInfo targetMethod = null;

            //if (entityStr != string.Empty) {
            //    if (int.TryParse(entityStr, out int entityNum)) {
            //        targetEntity = EntityController.GetEntity(entityNum);
            //    }
            //    else {

            //    }
            //    targetEntity = GameObject.Find(entityStr);
            //}
            //if (targetEntity == null) {
            //    targetEntity = Player.SelectedActor?.Entity;
            //}
            //if (targetEntity == null) {
            //    LogError("You must either specify a GameObject with " + EntityOption + " or select one in the hierarchy.");
            //    Application.logMessageReceived -= InterceptDebugLog;
            //    return;
            //}

            //// Target Component
            //{
            //    if (componentStr != string.Empty) {
            //        targetComponent = targetEntity.GetComponent(componentStr);
            //        if (targetComponent == null) {
            //            Debug.LogError("Could not find component '" + componentStr + "'.");
            //            Application.logMessageReceived -= InterceptDebugLog;
            //            return;
            //        }
            //    }
            //}

            //// Target Method
            //{
            //    if (methodStr == string.Empty) {
            //        Debug.LogError("You must specify a method to call with " + MethodOption + ".");
            //        Application.logMessageReceived -= InterceptDebugLog;
            //        return;
            //    }
            //    if (targetComponent == null) {
            //        foreach (var component in targetEntity.GetComponents<Component>()) {
            //            var method = GetMethod(component, methodStr);
            //            if (method != null) {
            //                targetComponent = component;
            //                break;
            //            }
            //        }
            //        if (targetComponent == null) {
            //            if (string.IsNullOrEmpty(componentStr)) {
            //                Debug.LogError("Could not find a component with the method '" + methodStr + "'.");
            //            }
            //            else {
            //                Debug.LogError("Could not find component '" + componentStr + "'.");
            //            }
            //            Application.logMessageReceived -= InterceptDebugLog;
            //            return;
            //        }
            //    }
            //    targetMethod = GetMethod(targetComponent, methodStr);
            //    if (targetMethod == null) {
            //        Debug.LogError("Could not find method '" + methodStr + "'.");
            //        Application.logMessageReceived -= InterceptDebugLog;
            //        return;
            //    }
            //}
            //if (parameters.Count != targetMethod.GetParameters().Length) {
            //    Debug.LogError("Parameter count mismatch. You gave " + parameters.Count + " parameters and the method '" + methodStr + "' wants " + targetMethod.GetParameters().Length + ". Note that optional parameters are not supported.");
            //    Application.logMessageReceived -= InterceptDebugLog;
            //    return;
            //}
            //try {
            //    targetMethod.Invoke(targetComponent, parameters.ToArray());
            //}
            //catch (ArgumentException) {
            //    var wanted = new StringBuilder();
            //    var got = new StringBuilder();
            //    string comma = "";
            //    foreach (var param in targetMethod.GetParameters()) {
            //        wanted.Append(comma);
            //        comma = ", ";
            //        wanted.Append(param.ParameterType.Name);
            //    }
            //    comma = "";
            //    foreach (var param in parameters) {
            //        got.Append(comma);
            //        comma = ", ";
            //        got.Append(param.GetType().Name);
            //    }
            //    Debug.LogError("Parameter type mismatch. Wanted: (" + wanted + "), got: (" + got + ").");
            //}
        }

        private MethodInfo GetMethod(Type type, string methodName) {
            return type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private StringBuilder _builder = new StringBuilder();

        private void UpdateHistoryText() {
            _builder.Clear();
            for (int i = 0; i < _historyList.Count; i++) {
                var entry = _historyList[i];
                _builder.Append("\n");
                if (i == _commandIndex) {
                    _builder.Append("<color=\"#" + ColorUtility.ToHtmlStringRGBA(_navigationIndicatorColor) + "\">" + _navigationIndicator + "</color>");
                }
                else {
                    _builder.Append(" ");
                }
                _builder.Append("<color=\"#" + ColorUtility.ToHtmlStringRGBA(_commandHistoryColor) + "\">" + entry.Command + "</color>");
                if (!string.IsNullOrEmpty(entry.Output)) {
                    _builder.Append(entry.Output);
                }
            }
            _historyText.text = _builder.ToString();
        }

        public class Entry {
            public string Command;
            public string Output;
        }
    }
}