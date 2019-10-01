using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using PixelComrades.Debugging;
using UnityEngine;


namespace PixelComrades {
    public class Console : MonoBehaviour {

        private const int HistorySize = 200;
        private const string ConsoleControlName = "ControlField";
        private const string PrintColor = "white";
        private const string WarningColor = "orange";
        private const string ErrorColor = "red";
        private const string UserColor = "lime";
        private static Console _instance;
        private static Texture2D Pixel {
            get {
                Texture2D texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.8f));
                texture.Apply();
                return texture;
            }
        }
        private static int MaxLines {
            get {
                int lines = Mathf.RoundToInt(Screen.height * 0.45f / 16);
                return Mathf.Clamp(lines, 4, 32);
            }
        }
        private static Console Instance {
            get {
                if (!_instance) {
                    GameObject consoleGameObject = GameObject.Find("Console");
                    if (!consoleGameObject) {
                        consoleGameObject = new GameObject("Console");
                    }
                    Console console = consoleGameObject.GetComponent<Console>();
                    if (!console) {
                        console = consoleGameObject.AddComponent<Console>();
                        console.UpdateText();
                    }
                    _instance = console;
                }
                return _instance;
            }
        }
        public static bool Open { get { return Instance._open; } set { Instance._open = value; } }
        private static int Scroll {
            get { return Instance._scroll; }
            set {
                if (value < 0)
                    value = 0;
                if (value >= HistorySize)
                    value = HistorySize - 1;
                Instance._scroll = value;
            }
        }

        [SerializeField] private Font _font = null;

        private string _input;
        private float _deltaTime;
        private bool _open;
        private int _scroll;
        private int _index;
        private int _lastMaxLines;
        private List<string> _text = new List<string>();
        private List<string> _history = new List<string>();
        private List<string> _searchResults = new List<string>();
        private string _linesString;
        private bool _typedSomething;
        private GUIStyle _consoleStyle;
        private GUIStyle _fpsCounterStyle;

        private void Awake() {
            _instance = this;
            Parser.Initialize();
            CreateStyle();
        }

        private void OnEnable() {
            _instance = this;
            Parser.Initialize();
            if (!Application.isEditor) {
                Application.logMessageReceived += HandleLog;
            }
        }

        private void OnDisable() {
            if (!Application.isEditor) {
                Application.logMessageReceived -= HandleLog;
            }
        }

        //creates a style to be used in the gui calls
        private void CreateStyle() {
            Texture2D pixel = Pixel;
            _consoleStyle = new GUIStyle {
                richText = true,
                alignment = TextAnchor.UpperLeft,
                font = _font,
                fontSize = 12
            };
            _consoleStyle.normal.background = pixel;
            _consoleStyle.normal.textColor = Color.white;
            _consoleStyle.hover.background = pixel;
            _consoleStyle.hover.textColor = Color.white;
            _consoleStyle.active.background = pixel;
            _consoleStyle.active.textColor = Color.white;

            //fps counter style
            _fpsCounterStyle = new GUIStyle(_consoleStyle) {
                alignment = TextAnchor.UpperRight
            };
        }

        private void HandleLog(string message, string stack, LogType logType) {
            if (logType == LogType.Warning) {
                //dont print this, its spam
                return;
            }
            else if (logType == LogType.Log) {
                WriteLine(message, logType);
            }
            else {
                //if any kind of error, print the stack as well
                WriteLine(message + "\n" + stack, logType);
            }
        }

        private static string GetStringFromObject(object message) {
            if (message == null) {
                return null;
            }
            if (message is List<byte> listOfBytes) {
                StringBuilder builder = new StringBuilder(listOfBytes.Count * 5);
                int spacing = 25;
                for (int i = 0; i < listOfBytes.Count; i++) {
                    builder.Append(listOfBytes[i].ToString("000"));
                    builder.Append(" ");
                    if (i % spacing == 0 && i >= spacing) {
                        builder.AppendLine();
                    }
                }
                return builder.ToString();
            }
            else if (message is byte[] arrayOfBytes) {
                StringBuilder builder = new StringBuilder(arrayOfBytes.Length * 5);
                int spacing = 25;
                for (int i = 0; i < arrayOfBytes.Length; i++) {
                    builder.Append(arrayOfBytes[i].ToString("000"));
                    builder.Append(" ");
                    if (i % spacing == 0 && i >= spacing) {
                        builder.AppendLine();
                    }
                }
                return builder.ToString();
            }
            return message.ToString();
        }

        /// <summary>
        /// Prints a log message.
        /// </summary>
        public static void Print(object message) {
            string txt = GetStringFromObject(message);
            if (txt == null) {
                return;
            }
            Instance.Add(txt, PrintColor);
        }

        /// <summary>
        /// Prints a warning message.
        /// </summary>
        public static void Warn(object message) {
            string txt = GetStringFromObject(message);
            if (txt == null) {
                return;
            }
            Instance.Add(txt, WarningColor);
        }

        /// <summary>
        /// Prints an error message.
        /// </summary>
        public static void Error(object message) {
            string txt = GetStringFromObject(message);
            if (txt == null) {
                return;
            }
            Instance.Add(txt, ErrorColor);
        }

        private static string _lastText = "";

        public static void Log(string text) {
            if (!Application.isPlaying) {
                Debug.Log(text);
                return;
            }
            if (_lastText == text) {
                return;
            }
            _lastText = text;
            Instance.Add(text, PrintColor);
        }

        /// <summary>
        /// Clears the console.
        /// </summary>
        public static void Clear() {
            Scroll = 0;
            Instance._input = "";
            Instance._text.Clear();
            Instance._history.Clear();
            Instance.UpdateText();
        }

        /// <summary>
        /// Submit generic text to the console.
        /// </summary>
        public static void WriteLine(object text, LogType type = LogType.Log) {
            if (type == LogType.Log) {
                Print(text);
            }
            else if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception) {
                Error(text);
            }
            else if (type == LogType.Warning) {
                Warn(text);
            }
        }

        /// <summary>
        /// Runs a single command.
        /// </summary>
        public static async void Run(string command) {
            //run
            object result = await Parser.Run(command);
            if (result == null) {
                return;
            }
            if (result is Exception exception) {
                Exception inner = exception.InnerException;
                if (inner != null) {
                    Error(exception.Message + "\n" + exception.Source + "\n" + inner.Message + "\n" + inner.StackTrace);
                }
                else {
                    Error(exception.Message + "\n" + exception.StackTrace);
                }
            }
            else {
                Print(result.ToString());
            }
        }

        /// <summary>
        /// Runs a list of commands.
        /// </summary>
        public static void Run(List<string> commands) {
            if (commands == null) {
                return;
            }
            for (int i = 0; i < commands.Count; i++) {
                Run(commands[i]);
            }
        }

        //adds text to console text
        private void Add(string input, string color) {
            List<string> lines = new List<string>();
            string str = input.ToString();
            if (str.Contains("\n")) {
                lines.AddRange(str.Split('\n'));
            }
            else {
                lines.Add(str);
            }
            for (int i = 0; i < lines.Count; i++) {
                _text.Add("<color=" + color + ">" + lines[i] + "</color>");
                if (_text.Count > HistorySize) {
                    _text.RemoveAt(0);
                }
            }
            int newScroll = _text.Count - MaxLines;
            if (newScroll > Scroll) {
                //set scroll to bottom
                Scroll = newScroll;
            }

            //update the lines string
            UpdateText();
        }

        //creates a single text to use when display the console
        private void UpdateText() {
            string[] lines = new string[MaxLines];
            int lineIndex = 0;
            for (int i = 0; i < _text.Count; i++) {
                int index = i + Scroll;
                if (index < 0) {
                    continue;
                }
                else if (index >= _text.Count) {
                    continue;
                }
                else if (string.IsNullOrEmpty(_text[index])) {
                    break;
                }
                lines[lineIndex] = (_text[index]);

                //replace all \t with 4 spaces
                lines[lineIndex] = lines[lineIndex].Replace("\t", "    ");
                lineIndex++;
                if (lineIndex == MaxLines) {
                    break;
                }
            }
            _linesString = string.Join("\n", lines);
        }

        private void Update() {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;

            //max lines amount changed
            if (_lastMaxLines != MaxLines) {
                _lastMaxLines = MaxLines;
                UpdateText();
            }
        }

        private void ShowFPS() {
            if (CommandsBuiltin.ShowFPS) {
                if (_fpsCounterStyle == null) {
                    CreateStyle();
                }
                float msec = _deltaTime * 1000f;
                float fps = 1f / _deltaTime;
                string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
                Rect rect = new Rect(Screen.width - 100, 0, 100, 0);
                GUI.Label(rect, text, _fpsCounterStyle);
            }
        }

        private bool IsConsoleKey(KeyCode key) {
            if (key == KeyCode.BackQuote) {
                return true;
            }
            else if (key == KeyCode.Tilde) {
                return true;
            }
            else {
                return false;
            }
        }

        private bool IsConsoleChar(char character) {
            if (character == '`') {
                return true;
            }
            else if (character == '~') {
                return true;
            }
            else if (character == '§') {
                return true;
            }
            else {
                return false;
            }
        }

        private void Search(string text) {
            //search through all commands
            _searchResults.Clear();
            if (string.IsNullOrEmpty(text)) {
                return;
            }
            foreach (Category category in Library.Categories) {
                foreach (ConsoleCommand command in category.Commands) {
                    if (command.Name.StartsWith(text)) {
                        string suggestionText = string.Join("/", command.Names);
                        if (command.Member is MethodInfo method) {
                            foreach (var parameter in command.Parameters) {
                                suggestionText += " <" + parameter + ">";
                            }
                        }
                        else if (command.Member is PropertyInfo property) {
                            MethodInfo set = property.GetSetMethod();
                            if (set != null) {
                                suggestionText += " [value]";
                            }
                        }
                        else if (command.Member is FieldInfo field) {
                            suggestionText += " [value]";
                        }
                        if (command.Description != "") {
                            suggestionText += " = " + command.Description;
                        }
                        if (!command.IsStatic) {
                            suggestionText = "@id " + suggestionText;
                        }
                        _searchResults.Add(suggestionText);
                    }
                }
            }
        }

        private void MoveToEnd() {
            TextEditor te = (TextEditor) GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            if (te != null) {
                te.MoveCursorToPosition(new Vector2(int.MaxValue, int.MaxValue));
            }
        }

        public static void Toggle() {
            Instance._input = "";
            Open = !Open;
            PlayerInputSystem.AllInputBlocked = Open;
            if (Open) {
                Instance._typedSomething = false;
                Instance._index = Instance._history.Count;
                Instance.Search(Instance._input);
            }
        }

        [Command("quit")]
        private static void ExitConsole() {
            Open = false;
            PlayerInputSystem.AllInputBlocked = false;
        }

        private void OnGUI() {
            ShowFPS();
            if (Event.current.type == EventType.KeyDown) {
                if (IsConsoleKey(Event.current.keyCode)) {
                    ExitConsole();
                    Event.current.Use();
                    return;
                }
                if (IsConsoleChar(Event.current.character)) {
                    return;
                }
            }
            //dont show the console if it shouldnt be open
            if (!Open) {
                return;
            }
            bool moveToEnd = false;
            //view scrolling
            if (Event.current.type == EventType.ScrollWheel) {
                int scrollDirection = (int) Mathf.Sign(Event.current.delta.y) * 3;
                Scroll += scrollDirection;
                UpdateText();
            }

            //history scrolling
            if (Event.current.type == EventType.KeyDown) {
                if (Event.current.keyCode == KeyCode.UpArrow) {
                    if (!_typedSomething) {
                        _index--;
                        if (_index < -1) {
                            _index = -1;
                            _input = "";
                            moveToEnd = true;
                        }
                        else {
                            if (_index >= 0 && _index < _history.Count) {
                                _input = _history[_index];
                                moveToEnd = true;
                            }
                        }
                    }
                    else {
                        _index--;
                        if (_index <= -1) {
                            _index = -1;
                            _input = "";
                            moveToEnd = true;
                        }
                        else {
                            if (_index >= 0 && _index < _searchResults.Count) {
                                _input = _searchResults[_index];
                                moveToEnd = true;
                            }
                        }
                    }
                }
                else if (Event.current.keyCode == KeyCode.DownArrow) {
                    if (!_typedSomething) {
                        _index++;
                        if (_index > _history.Count) {
                            _index = _history.Count;
                            _input = "";
                            moveToEnd = true;
                        }
                        else {
                            if (_index >= 0 && _index < _history.Count) {
                                _input = _history[_index];
                                moveToEnd = true;
                            }
                        }
                    }
                    else {
                        _index++;
                        if (_index >= _searchResults.Count) {
                            _index = _searchResults.Count;
                            _input = "";
                            moveToEnd = true;
                        }
                        else {
                            if (_index >= 0 && _index < _searchResults.Count) {
                                _input = _searchResults[_index];
                                moveToEnd = true;
                            }
                        }
                    }
                }
            }

            //draw elements
            Color oldColor = GUI.color;
            GUI.color = Color.white;
            GUI.depth = -5;
            int lineHeight = _consoleStyle.fontSize + 4;
            GUILayout.Box(_linesString, _consoleStyle, GUILayout.Width(Screen.width));
            Rect lastControl = GUILayoutUtility.GetLastRect();

            //draw the typing field
            GUI.Box(new Rect(0, lastControl.y + lastControl.height, Screen.width, 2), "", _consoleStyle);
            GUI.SetNextControlName(ConsoleControlName);
            string text = GUI.TextField(
                new Rect(0, lastControl.y + lastControl.height + 1, Screen.width, lineHeight), _input, _consoleStyle);
            GUI.FocusControl(ConsoleControlName);
            if (moveToEnd) {
                MoveToEnd();
            }

            //text changed, search
            if (_input != text) {
                if (!_typedSomething) {
                    _typedSomething = true;
                    _index = -1;
                }
                if (string.IsNullOrEmpty(text) && _typedSomething) {
                    _typedSomething = false;
                    _index = _history.Count;
                }
                _input = text;
                Search(text);
            }

            //display the search box
            GUI.color = new Color(1f, 1f, 1f, 0.4f);
            GUI.Box(
                new Rect(0, lastControl.y + lastControl.height + 1 + lineHeight, Screen.width, _searchResults.Count * lineHeight),
                string.Join("\n", _searchResults), _consoleStyle);
            GUI.color = oldColor;

            //pressing enter to run command
            if (Event.current.type == EventType.KeyDown) {
                bool enter = Event.current.character == '\n' || Event.current.character == '\r';
                if (enter) {
                    Add(_input, UserColor);
                    _history.Add(_input);
                    _index = _history.Count;
                    Search(null);
                    Run(_input);
                    Event.current.Use();
                    _input = "";
                    _typedSomething = false;
                    return;
                }
            }
        }
    }
}