using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIMainMenu : MonoSingleton<UIMainMenu> {

        private static float[] _lodBias = new float[7] {
        0.3f,0.55f,0.68f,1.09f,1.22f,1.59f,4.37f
    };

        private static float[] _shadowDist = new[] {
        12.6f, 17.4f,29.7f,82, 110,338, 800
    };

        private const int IndexMain = 0;
        private const int IndexOptions = 1;
        private const int IndexSide = 2;
        private const int IndexInfo = 3;

        [SerializeField] private float _transitionLength = 0.75f;
        [SerializeField] private GameObject _buttonprefab = null;
        [SerializeField] private GameObject _labelPrefab = null;
        [SerializeField] private GameObject _sliderPrefab = null;
        [SerializeField] private GameObject _togglePrefab = null;
        [SerializeField] private GameObject _dropdownPrefab = null;
        [SerializeField] private PrefabEntity _horizontalListPrefab = null;
        [SerializeField] private CanvasGroup _canvasGroup = null;
        [SerializeField] private CanvasGroup[] _layoutGroups = new CanvasGroup[3];
        [SerializeField] private EasingTypes _moveInEasing = EasingTypes.BackIn;
        [SerializeField] private EasingTypes _moveOutEasing = EasingTypes.BounceOut;
        [SerializeField] private TextMeshProUGUI _optionsLabel = null;
        [SerializeField] private Transform _optionControlsTr = null;
        [SerializeField] private Image _background = null;
        [SerializeField] private TextMeshProUGUI _version = null;
        [SerializeField] private AudioSource _menuMusic = null;
        [SerializeField] private float _maxMenuMusicVolume = 0.5f;
        [SerializeField] private UISaveLoad _saveLoad = null;
        [SerializeField] private SceneField _editorScene = new SceneField();

        private Vector3[] _startPositions;
        private Vector3[] _exitPositions;
        private Status _status = Status.Main;
        private UIGenericButton _resume;
        private UIGenericButton _save;
        private List<GameObject> _targetOptions = new List<GameObject>();
        private List<string> _resolutionLabel = new List<string>();
        private Task[,] _fadeTasks;
        private int _qualitySetting;
        private string[] _qualtyNames;
        private Resolution[] _allRes;
        private Resolution _currentRes;
        private int _currentResIndex;
        private List<string> _monitorLabels = new List<string>();
        private Display[] _monitors;
        private int _monitorIndex = -1;
        private InputMapper _inputMapper;
        private Task _animating = null;

        void Awake() {
            _qualtyNames = QualitySettings.names;
            _inputMapper = InputMapper.Default;
            _inputMapper.options.timeout = 5;
            _inputMapper.options.ignoreMouseXAxis = true;
            _inputMapper.options.ignoreMouseYAxis = true;
            ReInput.userDataStore.Load();
            _version.text = Game.Version.ToString("F2");
            //_partyManagement.OnWindowClose += CloseClassMenu;
            //_partyManagement.OnWindowConfirm += StartNewGame;
        }

        void Start() {
            _fadeTasks = new Task[_layoutGroups.Length,2];
            _startPositions = new Vector3[_layoutGroups.Length];
            _exitPositions = new Vector3[_layoutGroups.Length];
            for (int i = 0; i < _layoutGroups.Length; i++) {
                _startPositions[i] = _layoutGroups[i].transform.localPosition;
            }
            SetExitPositions();
            for (int i = 0; i < _layoutGroups.Length; i++) {
                _layoutGroups[i].transform.localPosition = _exitPositions[i];
                _layoutGroups[i].SetActive(false);
            }
            FindResolutions();
            _monitors = Display.displays;
            for (int i = 0; i < _monitors.Length; i++) {
                _monitorLabels.Add(string.Format("Display {0}", i));
                if (_monitors[i] == Display.main) {
                    _monitorIndex = i;
                }
            }
            SetupMain();
            SetupOptions();
        }

        private void FindResolutions() {
            _allRes = Screen.resolutions;
            _currentRes = Screen.currentResolution;
            for (int i = 0; i < _allRes.Length; i++) {
                _resolutionLabel.Add(string.Format("{0} x {1}", _allRes[i].width, _allRes[i].height));
                if (_currentRes.width == _allRes[i].width && _currentRes.height == _allRes[i].height) {
                    _currentResIndex = i;
                }
            }
        }

        private void SetExitPositions() {
            _exitPositions[0] = _startPositions[0] + new Vector3(0, -Screen.height * 2, 0);
            _exitPositions[1] = _startPositions[1] + new Vector3(-Screen.width * 2, 0, 0);
            _exitPositions[2] = _startPositions[2] + new Vector3(Screen.width * 2, 0, 0);
            for (int i = 3; i < _layoutGroups.Length; i++) {
                _exitPositions[i] = _startPositions[i] + new Vector3(0, Screen.height * 2, 0);
            }
        }

        public void Toggle() {
            if (_animating != null) {
                return;
            }
            if (_canvasGroup.alpha > 0) {
                CloseMainMenu(0);
            }
            else {
                OpenMenu();
            }
        }

        public void OpenMenu() {
            if (_animating != null) {
                return;
            }
            bool inGame = Game.GameActive;
            var bgColor = Color.black;
            bgColor.a = inGame ? 155 : 255;
            _background.color = bgColor;
            _animating = _canvasGroup.FadeTo(1, _transitionLength * 0.5f, EasingTypes.SinusoidalOut, true, Tween.TweenRepeat.Once, ()=> _animating = null);
            _canvasGroup.interactable = _canvasGroup.blocksRaycasts = true;
            Game.PauseAndUnlockCursor("UIMainMenu");
            LoadMainMenu(0);
            EventSystem.current.SetSelectedGameObject(_layoutGroups[IndexMain].transform.GetChild(1).gameObject);
            if (!inGame && _menuMusic != null) {
                TimeManager.StartUnscaled(FadeMusic(true, 0.5f));
            }
        }

        private void SetupMain() {
            _resume = SetupButton(_layoutGroups[IndexMain].transform, "Resume", CloseMainMenu);
            SetupButton(_layoutGroups[IndexMain].transform, "New Game", OpenPartyManagement);
            SetupButton(_layoutGroups[IndexMain].transform, "Options", OpenOptionsMenu);
            _save = SetupButton(_layoutGroups[IndexMain].transform, "Save", OpenSave);
            SetupButton(_layoutGroups[IndexMain].transform, "Load", OpenLoad);
            SetupButton(_layoutGroups[IndexMain].transform, "Instructions", OpenInstructions);
            SetupButton(_layoutGroups[IndexMain].transform, "Level Editor", i => SceneManager.LoadSceneAsync(_editorScene, LoadSceneMode.Single));
            //SetupButton(_layoutGroups[IndexMain].transform, "Top Scores", OpenScores);
            SetupButton(_layoutGroups[IndexMain].transform, "Quit", CheckQuit);
            _resume.gameObject.SetActive(false);
            _save.gameObject.SetActive(false);
        }

        private void SetupOptions() {
            SetupButton(_layoutGroups[IndexOptions].transform, "Gameplay", SetTargetOptionsMenu);
            SetupButton(_layoutGroups[IndexOptions].transform, "Input", SetTargetOptionsMenu);
            SetupButton(_layoutGroups[IndexOptions].transform, "Audio", SetTargetOptionsMenu);
            SetupButton(_layoutGroups[IndexOptions].transform, "Video", SetTargetOptionsMenu);
            SetupButton(_layoutGroups[IndexOptions].transform, "Return", (index)=> {
                ClearTargetOptions();
                LoadMainMenu(index);
            });
        }

        private void LoadMainMenu(int index) {
            if (_status != Status.Main) {
                FadeOut(IndexOptions);
            }
            if (_status == Status.Right) {
                FadeOut(IndexSide);
            }
            _status = Status.Main;
            _resume.gameObject.SetActive(Game.GameActive);
            _save.gameObject.SetActive(Game.GameActive);
            FadeIn(IndexMain);
        }

        private void OpenOptionsMenu(int index) {
            FadeOut(IndexMain);
            if (_status == Status.Right) {
                FadeOut(IndexSide);
            }
            _status = Status.Left;
            FadeIn(IndexOptions);
        }

        private void OpenInstructions(int index) {
            FadeOut(IndexMain);
            FadeIn(IndexInfo);
        }

        private void OpenSave(int index) {
            _saveLoad.OpenSaveMenu();
        }

        private void OpenLoad(int index) {
            _saveLoad.OpenLoadMenu();
        }

        public void CloseInstructions() {
            FadeIn(IndexMain);
            FadeOut(IndexInfo);
        }

        //private void OpenScores(int index) {
        //    FadeOut(IndexMain);
        //    FadeIn(IndexHighScores);
        //    UIScoreList.main.ListScores();
        //}

        //public void CloseScores() {
        //    FadeIn(IndexMain);
        //    FadeOut(IndexHighScores);
        //}

        private void OpenPartyManagement(int index) {
            MessageKit.post(Messages.SetupNewGame);
            //FadeOut(IndexMain);
            //FadeIn(IndexClass);
        }

        //public void CloseClassMenu() {
        //    FadeIn(IndexMain);
        //    FadeOut(IndexClass);
        //}

        public void StartNewGame() {
            CloseMainMenu(0);
            if (_menuMusic != null && _menuMusic.isPlaying) {
                TimeManager.StartUnscaled(FadeMusic(false, 0.2f));
            }
            TimeManager.PauseFor(_transitionLength * 0.25f, true, () => {
                MessageKit.post(Messages.StartNewGame);
            });
        }

        private IEnumerator FadeMusic(bool active, float length) {
            if (active) {
                _menuMusic.volume = 0;
                _menuMusic.Play();
            }
            while (true) {
                if (active) {
                    _menuMusic.volume += length * Time.deltaTime;
                    if (_menuMusic.volume >= _maxMenuMusicVolume) {
                        break;
                    }
                }
                else {
                    _menuMusic.volume -= length * Time.deltaTime;
                    if (_menuMusic.volume <= 0) {
                        _menuMusic.Stop();
                        break;
                    }
                }
                yield return null;
            }
        }

        private UIGenericButton SetupButton(Transform parent, string label, System.Action<int> onClick) {
            var newButton = ItemPool.SpawnUIPrefab<UIGenericButton>(_buttonprefab, parent);
            newButton.Index = newButton.transform.GetSiblingIndex();
            newButton.SetText(label);
            newButton.OnButtonClicked = onClick;
            return newButton;
        }

        private UIGenericLabel SetupLabel(Transform parent, string label) {
            var newButton = ItemPool.SpawnUIPrefab<UIGenericLabel>(_labelPrefab, parent);
            newButton.SetText(label);
            return newButton;
        }

        private UIGenericSlider SetupSlider(Transform parent, float value, float min, float max, System.Action<UIGenericSlider, float> onChange, bool wholeNumbers) {
            var newButton = ItemPool.SpawnUIPrefab<UIGenericSlider>(_sliderPrefab, parent);
            newButton.SetNewSlider(min, max, value, wholeNumbers);
            newButton.OnValueChanged = onChange;
            return newButton;
        }

        private UIGenericDropdown SetupDropdown(Transform parent, List<string> options, int index, Action<int> onChange, string label) {
            var newButton = ItemPool.SpawnUIPrefab<UIGenericDropdown>(_dropdownPrefab, parent);
            newButton.SetText(label);
            newButton.Dropdown.ClearOptions();
            newButton.Dropdown.AddOptions(options);
            newButton.Dropdown.value = index;
            newButton.OnValueChanged = onChange;
            return newButton;
        }

        private UIGenericToggle SetupToggle(Transform parent, bool currentValue, Action<bool> onChange, string label) {
            var newButton = ItemPool.SpawnUIPrefab<UIGenericToggle>(_togglePrefab, parent);
            newButton.SetText(label);
            newButton.SetInitialValue(currentValue);
            newButton.OnValueChanged = onChange;
            return newButton;
        }

        private void FadeOut(int index) {
            CheckTasks(index);
            _fadeTasks[index, 0] = _layoutGroups[index].transform.LocalMoveTo(_exitPositions[index], _transitionLength, _moveOutEasing, true, Tween.TweenRepeat.Once, () => _fadeTasks[index, 0] = null);
            _fadeTasks[index, 1] = _layoutGroups[index].FadeTo(0, _transitionLength, EasingTypes.SinusoidalOut, true, Tween.TweenRepeat.Once, () => _fadeTasks[index, 1] = null);
        }

        private void CheckTasks(int index) {
            if (_fadeTasks[index, 0] != null) {
                _fadeTasks[index, 0].Cancel();
            }
            if (_fadeTasks[index, 1] != null) {
                _fadeTasks[index, 1].Cancel();
            }
        }

        private void FadeIn(int index) {
            CheckTasks(index);
            _fadeTasks[index, 0] = _layoutGroups[index].transform.LocalMoveTo(_startPositions[index], _transitionLength, _moveInEasing, true, Tween.TweenRepeat.Once, () => _fadeTasks[index, 0] = null);
            _fadeTasks[index, 1] = _layoutGroups[index].FadeTo(1, _transitionLength, EasingTypes.SinusoidalIn, true, Tween.TweenRepeat.Once, () => _fadeTasks[index, 1] = null);
            EventSystem.current.SetSelectedGameObject(_layoutGroups[index].transform.GetChild(0).gameObject);
        }


        private void QuitConfirm(int index) {
            if (index > 0) {
                return;
            }
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }

        public void CloseMainMenu() {
            CloseMainMenu(0);
        }

        private void CloseMainMenu(int index) {
            if (_animating != null) {
                return;
            }
            _animating = _canvasGroup.FadeTo(0, _transitionLength * 0.5f, EasingTypes.SinusoidalIn, true, Tween.TweenRepeat.Once, () => _animating = null);
            _canvasGroup.interactable = _canvasGroup.blocksRaycasts = false;
            _saveLoad.Close();
            Game.RemovePauseAndLockCursor("UIMainMenu");
            MessageKit<GameObject>.post(Messages.MenuClosed, gameObject);
        }

        private void CheckQuit(int index) {
            UIModalQuestion.Set(QuitConfirm, "Are you sure you want to Quit?", "Yes", "No");
        }

        private void SetTargetOptionsMenu(int index) {
            if (index == 0) {
                OpenGameplayPanel();
            }
            else if (index == 1) {
                SetupControlsMenu();
            }
            else if (index == 2) {
                SetupAudioMenu();
            }
            else if (index == 3) {
                OpenVideoPanel();
            }
            if (_status != Status.Right) {
                _status = Status.Right;
                FadeIn(2);
            }

        }

        private void SetupControlsMenu() {
            ClearTargetOptions();
            _optionsLabel.text = "Controls";
            //inputMapper.ConflictFoundEvent += OnConflictFound;
            _inputMapper.StoppedEvent += OnStopped;
            RefreshControls();
        }

        private void RefreshControls() {
            Controller controller = PlayerInput.RewiredPlayer.controllers.GetLastActiveController();
            if (controller.type == ControllerType.Mouse) {
                controller = PlayerInput.RewiredPlayer.controllers.Keyboard;
            }
            var controllerMap = PlayerInput.RewiredPlayer.controllers.maps.GetFirstMapInCategory(controller, 0);
            foreach (InputAction inputAction in ReInput.mapping.ActionsInCategory(0)) {
                _targetOptions.Add(SetupLabel(_optionControlsTr, inputAction.descriptiveName).gameObject);
                var pivot = ItemPool.SpawnUIPrefab(_horizontalListPrefab, _optionControlsTr);
                _targetOptions.Add(pivot.gameObject);
                var action = inputAction;
                _targetOptions.Add(SetupButton(pivot.transform, "Add New", delegate (int i) {
                    InputMapper.Context context = new InputMapper.Context() {
                        actionId = action.id,
                        actionRange = AxisRange.Positive,
                        controllerMap = controllerMap
                    };
                    SetInputListen(context);

                }).gameObject);
                // Write out assigned elements
                for (var i = 0; i < controllerMap.AllMaps.Count; i++) {
                    ActionElementMap elementMap = controllerMap.AllMaps[i];
                    if (elementMap.actionId != action.id) {
                        continue;
                    }
                    _targetOptions.Add(SetupButton(pivot.transform, elementMap.elementIdentifierName, delegate (int i1) {
                        InputMapper.Context context = new InputMapper.Context() {
                            actionId = action.id,
                            actionRange = AxisRange.Positive,
                            controllerMap = controllerMap,
                            actionElementMapToReplace = elementMap
                        };
                        SetInputListen(context);
                    }).gameObject
                    );
                }
            }
        }

        private void OnStopped(InputMapper.StoppedEventData data) {
            UIModalQuestion.Stop();
            PlayerInput.AllInputBlocked = false;
            ReInput.userDataStore.Save();
            for (int i = 0; i < _targetOptions.Count; i++) {
                ItemPool.Despawn(_targetOptions[i]);
            }
            _targetOptions.Clear();
            RefreshControls();
        }

        public void SetInputListen(InputMapper.Context context) {
            PlayerInput.AllInputBlocked = true;
            _inputMapper.Start(context);
            UIModalQuestion.Set((int i) => {
                PlayerInput.AllInputBlocked = false;
                _inputMapper.Stop();
            }, "Press the new button", "Cancel");
        }

        private void SetupAudioMenu() {
            ClearTargetOptions();
            _optionsLabel.text = "Audio";
            _targetOptions.Add(SetupLabel(_optionControlsTr, "Volume").gameObject);
            _targetOptions.Add(SetupSlider(_optionControlsTr, AudioListener.volume, 0, 1, UpdateVolume, false).gameObject);
        }

        private void UpdateVolume(UIGenericSlider slider, float newValue) {
            AudioListener.volume = newValue;
            slider.SetText(AudioListener.volume.ToString("F1"));
        }

        private void OpenGameplayPanel() {
            ClearTargetOptions();
            _optionsLabel.text = "Gameplay";

            _targetOptions.Add(SetupLabel(_optionControlsTr, "Look Sensitivity").gameObject);
            _targetOptions.Add(SetupSlider(_optionControlsTr, GameOptions.Get("LookSensitivity", 2.5f),
                0.1f, 6, (slider, f) => { GameOptions.Set("LookSensitivity", f); }, false).gameObject);

            //_targetOptions.Add(SetupLabel(_optionControlsTr, "Look Smoothing").gameObject);
            //_targetOptions.Add(SetupSlider(_optionControlsTr, GameOptions.LookSmooth,
            //    0f, 1, (slider, f) => { GameOptions.LookSmooth = f; }, false).gameObject);

            _targetOptions.Add(SetupLabel(_optionControlsTr, "Verbose Inventory").gameObject);
            _targetOptions.Add(SetupToggle(_optionControlsTr, GameOptions.VerboseInventory, delegate (bool b) {
                GameOptions.VerboseInventory.Value = b;
            }, "Explicit status messages in the inventory").gameObject);

            _targetOptions.Add(SetupLabel(_optionControlsTr, "Show Misses").gameObject);
            _targetOptions.Add(SetupToggle(_optionControlsTr, GameOptions.ShowMiss, delegate (bool b) {
                GameOptions.ShowMiss.Value = b;
            }, "Log messages on every miss").gameObject);

            _targetOptions.Add(SetupLabel(_optionControlsTr, "Log All Damage").gameObject);
            _targetOptions.Add(SetupToggle(_optionControlsTr, GameOptions.LogAllDamage, delegate (bool b) {
                GameOptions.LogAllDamage.Value = b;
            }, "Log all damage for player and all npcs").gameObject);

            _targetOptions.Add(SetupLabel(_optionControlsTr, "Pause On Player Input").gameObject);
            _targetOptions.Add(SetupToggle(_optionControlsTr, GameOptions.PauseForInput, delegate (bool b) {
                GameOptions.PauseForInput.Value = b;
            }, "Phased mode where the action pauses when the player starts input").gameObject);

            _targetOptions.Add(SetupLabel(_optionControlsTr, "Turn Based").gameObject);
            _targetOptions.Add(SetupToggle(_optionControlsTr, GameOptions.TurnBased, delegate (bool b) {
                GameOptions.TurnBased = b;
            }, "Turn based mode where the action pauses whenever the player can act").gameObject);

            _targetOptions.Add(SetupLabel(_optionControlsTr, "MouseLook").gameObject);
            _targetOptions.Add(SetupToggle(_optionControlsTr, GameOptions.MouseLook, delegate (bool b) {
                GameOptions.MouseLook = b;
            }, "Mouse Look locks the cursor during normal gameplay. Disable for keyboard turning only").gameObject);

            _targetOptions.Add(SetupLabel(_optionControlsTr, "Ready Messages").gameObject);
            _targetOptions.Add(SetupToggle(_optionControlsTr, GameOptions.ReadyNotice, delegate(bool b) { GameOptions.ReadyNotice.Value = b; }, "Shows a message in the center of screen when a party member can act").gameObject);


            //_targetOptions.Add(SetupLabel(_optionControlsTr, "Menu Pause").gameObject);
            //_targetOptions.Add(SetupToggle(_optionControlsTr, PlayerInput.PauseInMenus, delegate(bool b) {
            //    PlayerInput.PauseInMenus = b;}, "Pause when in menus or inventory").gameObject);

            //_targetOptions.Add(SetupLabel(_optionControlsTr, "Camera Head bob").gameObject);
            //_targetOptions.Add(SetupToggle(_optionControlsTr, GameOptions.UseHeadBob, delegate(bool b) {
            //    GameOptions.UseHeadBob = b;}, "Should the camera have head bobbing").gameObject);

            //_targetOptions.Add(SetupLabel(_optionControlsTr, "Weapon Bob").gameObject);
            //_targetOptions.Add(SetupToggle(_optionControlsTr, GameOptions.UseSway, delegate(bool b) {
            //    GameOptions.UseSway = b;}, "Should the weapons bob with movement").gameObject);

            _targetOptions.Add(SetupLabel(_optionControlsTr, "Camera Shaking").gameObject);
            _targetOptions.Add(SetupToggle(_optionControlsTr, GameOptions.UseShaking, delegate (bool b) {
                GameOptions.UseShaking.Value = b;
            }, "Should the camera shake on damage").gameObject);

            _targetOptions.Add(SetupLabel(_optionControlsTr, "Camera Pain Flash").gameObject);
            _targetOptions.Add(SetupToggle(_optionControlsTr, GameOptions.UsePainFlash, delegate (bool b) {
                GameOptions.UsePainFlash.Value = b;
            }, "Should the camera flash a color on damage").gameObject);
        }

        private void OpenVideoPanel() {
            ClearTargetOptions();
            _optionsLabel.text = "Video";
            _qualitySetting = QualitySettings.GetQualityLevel();
            _targetOptions.Add(SetupLabel(_optionControlsTr, "Resolution").gameObject);
            _targetOptions.Add(SetupDropdown(_optionControlsTr, _resolutionLabel, _currentResIndex,
                ChangeResolution, "Game resolution").gameObject);
            if (_monitorLabels != null && _monitorLabels.Count > 0) {
                _targetOptions.Add(SetupLabel(_optionControlsTr, "Monitor").gameObject);
                _targetOptions.Add(SetupDropdown(_optionControlsTr, _monitorLabels, _monitorIndex,
                    ChangeMonitor, "Move the game window to another display").gameObject);
            }
            // need to add cull distance to render shadow casters further away
            _targetOptions.Add(SetupLabel(_optionControlsTr, "Quality").gameObject);
            _targetOptions.Add(SetupSlider(_optionControlsTr, _qualitySetting, 2, _qualtyNames.Length - 1, ChangeQualityLevel, true).gameObject);
            _targetOptions.Add(SetupLabel(_optionControlsTr, "FOV").gameObject);
            _targetOptions.Add(SetupSlider(_optionControlsTr, Player.Cam.fieldOfView, 40, 80, ChangeFov, true).gameObject);
            _targetOptions.Add(SetupLabel(_optionControlsTr, "Shadow Distance").gameObject);
            _targetOptions.Add(SetupSlider(_optionControlsTr, QualitySettings.shadowDistance, 15, 70, UpdateShadowDistance, true).gameObject);
            _targetOptions.Add(SetupLabel(_optionControlsTr, "Shadow Cascades").gameObject);
            _targetOptions.Add(SetupSlider(_optionControlsTr, QualitySettings.shadowCascades, 0, 3, UpdateShadowCascades, true).gameObject);
            //_targetOptions.Add(SetupToggle(_optionControlsTr, FirstPersonCamera.main.DepthOfField, delegate(bool b) {
            //    FirstPersonCamera.main.DepthOfField = b;}, "Depth of Field effects").gameObject);
            //_targetOptions.Add(SetupToggle(_optionControlsTr, FirstPersonCamera.main.SpaceReflections, delegate(bool b) {
            //    FirstPersonCamera.main.SpaceReflections = b;}, "Screen Space Reflections").gameObject);
            //_targetOptions.Add(SetupToggle(_optionControlsTr, FirstPersonCamera.main.Bloom, delegate(bool b) {
            //    FirstPersonCamera.main.Bloom = b;}, "Bloom").gameObject);
            _targetOptions.Add(SetupLabel(_optionControlsTr, "Full Screen").gameObject);
            _targetOptions.Add(SetupToggle(_optionControlsTr, Screen.fullScreen, delegate (bool b) {
                Screen.fullScreen = b;
            }, "Full screen borderless window").gameObject);
            _targetOptions.Add(SetupLabel(_optionControlsTr, "V-Sync").gameObject);
            _targetOptions.Add(SetupToggle(_optionControlsTr, QualitySettings.vSyncCount == 1, delegate (bool b) {
                QualitySettings.vSyncCount = b ? 1 : 0;
            }, "V-sync can affect performance").gameObject);

        }

        private void ChangeResolution(int resIndex) {
            _currentResIndex = resIndex;
            Screen.SetResolution(_allRes[resIndex].width, _allRes[resIndex].height, Screen.fullScreen);
            SetExitPositions();
        }

        private void ChangeMonitor(int i1) {
            if (_monitorIndex == i1) {
                return;
            }
            _monitorIndex = i1;
            PlayerPrefs.SetInt("UnitySelectMonitor", _monitorIndex);
            FindResolutions();
            SetExitPositions();
        }

        private void ChangeQualityLevel(UIGenericSlider slider, float newValue) {
            var quality = (int)newValue;
            QualitySettings.SetQualityLevel(quality);
            QualitySettings.shadowDistance = _shadowDist[quality];
            QualitySettings.lodBias = _lodBias[quality];
            slider.SetText(QualitySettings.names[quality]);
        }

        private void ChangeFov(UIGenericSlider slider, float newValue) {
            Player.Cam.fieldOfView = newValue;
        }

        private void ClearTargetOptions() {
            MessageKit.post(Messages.OptionsChanged);
            _inputMapper.RemoveAllEventListeners();
            for (int i = 0; i < _targetOptions.Count; i++) {
                ItemPool.Despawn(_targetOptions[i]);
            }
            _targetOptions.Clear();
        }

        private void UpdateShadowCascades(UIGenericSlider slider, float cascades) {
            var c = Mathf.RoundToInt(cascades);
            if (c == 1) {
                c = 2;
            }
            else if (c == 3) {
                c = 4;
            }
            QualitySettings.shadowCascades = c;
        }

        private void UpdateShadowDistance(UIGenericSlider slider, float dist) {
            QualitySettings.shadowDistance = dist;
        }

        private enum Status {
            Main,
            Left,
            Right,
        }
    }
}