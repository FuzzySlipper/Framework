using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace PixelComrades {
    public static partial class Game {

        public static float Version { get; set; }
        public static string Title { get; set; }
        public static bool InTown { get; set; } = false;
        public static int MapCellSize { get; set; }
        private static ValueHolder<bool> _paused = new ValueHolder<bool>(false, PauseChanged);
        private static ValueHolder<bool> _cursorUnlocked = new ValueHolder<bool>(false);
        private static bool _gameStarted = false;
        private static Random _random = new Random();
        private static Random _levelRandom = new Random();
        private static Camera _spriteCamera;
        private static int _mapSectorSize = 50;

        public static IMapCamera MiniMap;
        public static IMapCamera LevelMap;
        public static Camera SpriteCamera { get { return _spriteCamera != null ? _spriteCamera : Camera.main; } set { _spriteCamera = value; } }
        public static Random Random { get { return _random; } }
        public static Random LevelRandom { get { return _levelRandom; } }
        public static int Seed { get; private set; }
        public static ValueHolder<bool> PauseHolder { get { return _paused; } }
        public static ValueHolder<bool> CursorUnlockedHolder { get { return _cursorUnlocked; } }
        public static bool CursorUnlocked { get { return _cursorUnlocked.Value; } }
        public static bool GameActive { get; private set; } = false;
        public static bool GameStarted { get { return _gameStarted; } set { _gameStarted = value; } }
        public static bool IsEditor { get; set; }
        public static bool Debug { get { return GameOptions.DebugMode; } }
        public static string DefaultCurrencyId { get { return "Default"; } }

        public static void SetGameActive(bool status) {
            GameActive = status;
            if (status) {
                IsEditor = false;
            }
        }

        public static bool CoinFlip() {
            return _random.CoinFlip();
        }

        public static bool DiceRollSuccess(float chance) {
            return _random.DiceRollSucess(chance);
        }

        public static void PauseAndUnlockCursor(string id) {
            CursorUnlock(id);
            Pause(id);
        }

        public static void RemovePauseAndLockCursor(string id) {
            RemoveCursorUnlock(id);
            RemovePause(id);
        }

        public static void CursorUnlock(string id) {
            _cursorUnlocked.AddValue(true, id);
            Cursor.lockState = CursorLockMode.None;
            UICursor.main.SetCursor(UICursor.DefaultCursor);
            //if (!GameOptions.MouseLook) {
            //    return;
            //}
        }

        public static void RemoveCursorUnlock(string mod) {
            _cursorUnlocked.RemoveValue(mod);
            //if (!GameOptions.MouseLook) {
            //    return;
            //}
            if (!_cursorUnlocked.Value) {
                UICursor.main.SetCursor(UICursor.CrossHair);
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        public static bool Paused { get { return _paused.Value; } }

        public static void Pause(string id) {
            _paused.AddValue(true, id);
        }

        public static void RemovePause(string mod) {
            _paused.RemoveValue(mod);
        }

        private static void PauseChanged() {
            MessageKit.post(Messages.PauseChanged);
            //UnityEngine.Time.timeScale = Paused ? 0f : 1f;
            //UnityEngine.Time.fixedDeltaTime = 0.02f * Mathf.Clamp(UnityEngine.Time.timeScale, 0.00001f, 1);
        }

        public static Point3 WorldToSector(Vector3 position) {
            return new Point3(
                (int) Math.Round((double) position.x / _mapSectorSize),
                (int) Math.Round((double) position.y / _mapSectorSize),
                (int) Math.Round((double) position.z / _mapSectorSize));
        }

        public static void SetRandomSeed(int seed) {
            _random = new Random(seed);
            _levelRandom = new Random(seed);
            UnityEngine.Random.InitState(seed);
            Seed = seed;
        }

        public static void SetLevelSeed(int seed) {
            _levelRandom = new Random(seed);
        }

        public static void SetDataString(string key, string data) {
            if (Player.Data.DataString.ContainsKey(key)) {
                Player.Data.DataString[key] = data;
            }
            else {
                Player.Data.DataString.Add(key, data);
            }
            MessageKit<string>.post(Messages.GlobalDataChanged, key);
        }

        public static string GetDataString(string key) {
            string data;
            return Player.Data.DataString.TryGetValue(key, out data) ? data : null;
        }

        public static void AddToData(string key, int data) {
            if (Player.Data.DataInt.ContainsKey(key)) {
                Player.Data.DataInt[key] += data;
            }
            else {
                Player.Data.DataInt.Add(key, data);
            }
            MessageKit<string>.post(Messages.GlobalDataChanged, key);
        }

        public static int GetDataInt(string key) {
            int data;
            return Player.Data.DataInt.TryGetValue(key, out data) ? data : -1;
        }

        public static bool IsDataTrue(string key) {
            int data;
            if (Player.Data.DataInt.TryGetValue(key, out data)) {
                return data > 0;
            }
            return false;
        }

        public static void SetDataInt(string key, int data) {
            if (Player.Data.DataInt.ContainsKey(key)) {
                Player.Data.DataInt[key] = data;
            }
            else {
                Player.Data.DataInt.Add(key, data);
            }
            MessageKit<string>.post(Messages.GlobalDataChanged, key);
        }

        public static void DisplayData(Image source, Entity entity) {
            DisplayData(source, entity.Get<IconComponent>()?.Sprite, entity.Get<LabelComponent>()?.Text, entity.Get<DescriptionComponent>()?.Text, entity.Get<DataDescriptionComponent>()?.Text);
        }

        public static void DisplayData(Image source, Sprite sprite, string title, string descr, string data) {
            if (UIDataDetailDisplay.Current != null) {
                UIDataDetailDisplay.Current.Show(sprite, title, descr, data);
            }
            else {
                UITooltip.main.ShowToolTip(source, sprite, title, data);
            }
        }

        public static void DisplayCompareData(Entity entity) {
            DisplayCompareData(entity.Get<IconComponent>()?.Sprite, entity.Get<LabelComponent>()?.Text, entity.Get<DescriptionComponent>()?.Text, entity.Get<DataDescriptionComponent>()?.Text);
        }

        public static void DisplayCompareData(Sprite sprite, string title, string descr, string data) {
            if (UIDataDetailDisplay.Current != null) {
                UIDataDetailDisplay.Current.ShowCompare(sprite, title, descr, data);
            }
            else {
                UITooltip.main.ShowCompareToolTip(sprite, title, data);
            }
        }

        public static void HideDataDisplay() {
            if (UIDataDetailDisplay.Current == null) {
                UITooltip.main.HideTooltip();
            }
        }

        private static GameObject _mainObject;
        public static GameObject MainObject {
            get {
                if (_mainObject == null) {
                    if (TimeManager.IsQuitting) {
                        return null;
                    }
                    _mainObject = GameObject.Find(StringConst.MainObject);
                    if (_mainObject == null) {
                        _mainObject = new GameObject(StringConst.MainObject);
                    }
                    Object.DontDestroyOnLoad(_mainObject);
                }
                return _mainObject;
            }
        }

        public static Transform GetMainChild(string childName) {
            if (MainObject == null) {
                return new GameObject(childName).transform;
            }
            var child = MainObject.transform.Find(childName);
            if (child != null) {
                return child;
            }
            var go = new GameObject(childName);
            go.transform.SetParent(MainObject.transform);
            return go.transform;
        }

        public static Point3 LocalGridRotated(Point3 pos, Transform tr) {
            var calcPosV3 = Quaternion.AngleAxis(tr.rotation.eulerAngles.y, Vector3.up) * pos.toVector3();
            return new Point3(
                (int) Math.Round(calcPosV3.x),
                (int) Math.Round(calcPosV3.y),
                (int) Math.Round(calcPosV3.z));
        }

        public static Point3 LocalGridRotated(Point3 pos, float angle) {
            var calcPosV3 = Quaternion.AngleAxis(angle, Vector3.up) * pos.toVector3();
            return new Point3(
                (int) Math.Round(calcPosV3.x),
                (int) Math.Round(calcPosV3.y),
                (int) Math.Round(calcPosV3.z));
        }

        public static Point3 WorldToMapGrid(Vector3 position) {
            return new Point3(
                (int) Math.Round((double) position.x / MapCellSize),
                (int) Math.Round((double) position.y / MapCellSize),
                (int) Math.Round((double) position.z / MapCellSize));
        }

        public static Vector3 MapGridToWorld(Point3 position) {
            return new Vector3(position.x * MapCellSize, position.y * MapCellSize, position.z * MapCellSize);
        }

        public static Point3 WorldToUnitGrid(Vector3 position) {
            return new Point3(
                (int) Math.Round((double) position.x / GameConstants.UnitGrid),
                (int) Math.Round((double) position.y / GameConstants.UnitGrid),
                (int) Math.Round((double) position.z / GameConstants.UnitGrid));
        }

        public static Point3 WorldToUnitGridZeroY(Vector3 position) {
            return new Point3(
                (int) Math.Round((double) position.x / GameConstants.UnitGrid),
                0,
                (int) Math.Round((double) position.z / GameConstants.UnitGrid));
        }

        public static Vector3 UnitGridToWorld(Point3 position) {
            return new Vector3(position.x * GameConstants.UnitGrid, position.y * GameConstants.UnitGrid, position.z * GameConstants.UnitGrid);
        }
    }
}
