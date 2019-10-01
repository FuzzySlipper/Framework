using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using SimpleFileBrowser;
using Sirenix.Utilities;

namespace PixelComrades {
    public class UISaveLoad : MonoBehaviour {

        [SerializeField] private FileBrowser _fileBrowser = null;

        private const string SaveFileExtension = ".sav";

        void Start() {
            FileBrowser.SetFilters(false, new FileBrowser.Filter("Save File", SaveFileExtension));
            FileBrowser.SetDefaultFilter(SaveFileExtension);
        }

        //Need to put limitation on saves so you can not save in combat, near enemies, or with any active negative actorMods. ActorMods will clear on save/load

        public void OpenSaveMenu() {
            if (FileBrowser.IsOpen) {
                return;
            }
            _fileBrowser.ShowSaveDialogInternal(SaveFile, null, false, Application.persistentDataPath);
        }

        public void Close() {
            if (FileBrowser.IsOpen) {
                FileBrowser.HideDialog();
            }
        }

        private void SaveFile(string path) {
            if (path.Length == 0) {
                return;
            }
            if (!path.EndsWith(SaveFileExtension)) {
                path += SaveFileExtension;
            }
            var rootNode = new SerializedSaveGame(DateTime.Now.ToString("G"));
            var scenegraph = JsonConvert.SerializeObject(rootNode, Formatting.Indented, Serializer.ConverterSettings);
            FileUtility.SaveFile(path, scenegraph);
        }

        public void OpenLoadMenu() {
            if (FileBrowser.IsOpen) {
                return;
            }
            _fileBrowser.ShowLoadDialogInternal(LoadFile, null, false, Application.persistentDataPath);
        }

        private void LoadFile(string path) {
            string text = FileUtility.ReadFile(path);
            if (string.IsNullOrEmpty(text)) {
                return;
            }
            var saveGame = JsonConvert.DeserializeObject<SerializedSaveGame>(text, Serializer.ConverterSettings);
            if (saveGame == null) {
                Debug.LogErrorFormat("Error deserializing save game {0}", path);
                return;
            }
            MessageKit.post(Messages.ToggleMainMenu);
            saveGame.Load();
        }

    }
}