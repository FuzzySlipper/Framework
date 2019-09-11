using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;

namespace PixelComrades {
    public class UITextParser : MonoSingleton<UITextParser> {

        public static void Toggle() {
            if (_active) {
                main.Cancel();
            }
            else {
                main.Activate();
            }
        }
        private static bool _active;

        [SerializeField] private TMP_InputField _inputField = null;
        [SerializeField] private CanvasGroup _canvasGroup = null;
        [SerializeField] private Dictionary<string, System.Action> _actionDictionary = new Dictionary<string, System.Action>();

        void Awake() {
            //_actionDictionary.Add("attack", () => {
            //    Player.SelectedActor.Entity.Get<DefaultCommand>( d =>d.Get.TryStart());
            //});
            //_actionDictionary.Add("forward", () => {
            //    Player.Controller.TryMove(Point3.forward);
            //});
            //_actionDictionary.Add("back", () => {
            //    Player.Controller.TryMove(Point3.back);
            //});
            //_actionDictionary.Add("right", () => {
            //    Player.Controller.TryMove(Point3.right);
            //});
            //_actionDictionary.Add("left", () => {
            //    Player.Controller.TryMove(Point3.left);
            //});
        }

        private void Activate() {
            _active = true;
            _canvasGroup.SetActive(true);
            _inputField.text = "";
            _inputField.Select();
            PlayerInput.AllInputBlocked = true;
        }

        private void Cancel() {
            _active = false;
            _canvasGroup.SetActive(false);
            _inputField.text = "";
            EventSystem.current.SetSelectedGameObject(null);
            PlayerInput.AllInputBlocked = false;
        }

        public void ExecuteCommand() {
            //var input = _inputField.text.ToLower();
            //if (input.StartsWith("cast ")) {
            //    string[] words = (input.Remove(0, 5)).Split(' ');
            //    //var spell = Player.SelectedActor.SpellInventory.GetSpell(words);
            //    //if (spell != null) {
            //    //    spell.TryStart();
            //    //}
            //}
            //else {
            //    Action command;
            //    if (_actionDictionary.TryGetValue(input, out command)) {
            //        command();
            //    }
            //}
            //Cancel();
        }

    }
}