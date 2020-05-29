using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

namespace PixelComrades {
    public class UIModalQuestion : MonoSingleton<UIModalQuestion> {

        private static int _questionCount = 0;


        [SerializeField] private UIGenericButton[] _buttons = new UIGenericButton[3];
        [SerializeField] private TextMeshProUGUI _question = null;

        private System.Action<int> _del;
        private System.Action _simpleYes;
        private CanvasGroup _canvasGroup;
        private bool _active = false;
        
        public static bool Active { get => main._active; }

        void Awake() {
            main = this;
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public static void Set(System.Action<int> del, string question) {
            _questionCount = 2;
            main._buttons[0].SetText("Yes");
            main._buttons[0].gameObject.SetActive(true);
            main._buttons[1].SetText("No");
            main._buttons[1].gameObject.SetActive(true);
            main._question.text = question;
            main._del = del;
            main.StartModal();
        }

        public static void Set(System.Action<int> del, string question, string answer0) {
            _questionCount = 1;
            main._buttons[2].SetText(answer0);
            main._buttons[2].gameObject.SetActive(true);
            main._question.text = question;
            main._del = del;
            main.StartModal();
        }

        public static void Set(System.Action<int> del, string question, string answer0, string answer1) {
            _questionCount = 3;
            main._buttons[0].SetText(answer0);
            main._buttons[0].gameObject.SetActive(true);
            main._buttons[1].SetText(answer1);
            main._buttons[1].gameObject.SetActive(true);
            main._question.text = question;
            main._del = del;
            main.StartModal();
        }

        public static void SimpleQuestion(System.Action onYes, string question, string answer0 = "Yes", string answer1 = "No") {
            _questionCount = 3;
            main._buttons[0].SetText(answer0);
            main._buttons[0].gameObject.SetActive(true);
            main._buttons[1].SetText(answer1);
            main._buttons[1].gameObject.SetActive(true);
            main._question.text = question;
            main._simpleYes = onYes;
            main._del = main.YesNoAnswer;
            main.StartModal();
        }

        public static void Stop() {
            main.DisableModal();
        }

        private void YesNoAnswer(int answer) {
            if (answer == 0 && _simpleYes != null) {
                _simpleYes();
            }
            DisableModal();
        }

        void Update() {
            if (!_active) {
                return;
            }
            if (PlayerInputSystem.GetKeyDown(Key.Enter) || PlayerInputSystem.GetKeyDown(Key.Space) ||
                PlayerInputSystem.GetKeyDown(Key.Digit1) || PlayerInputSystem.GetKeyDown(Key.Y)) {
                ButtonAnswer(0);
                return;
            }
            if (_questionCount > 1) {
                if (PlayerInputSystem.GetKeyDown(Key.Digit2) || PlayerInputSystem.GetKeyDown(Key.N)) {
                    ButtonAnswer(1);
                    return;
                }
            }
            if (_questionCount > 2) {
                if (PlayerInputSystem.GetKeyDown(Key.Digit3)) {
                    ButtonAnswer(2);
                }
            }
        }

        public void ButtonAnswer(int index) {
            if (_del != null) {
                _del(index);
            }
            DisableModal();
        }

        private void StartModal() {
            _active = true;
            PlayerInputSystem.AllInputBlocked = true;
            Game.CursorUnlock("Modal");
            _canvasGroup.SetActive(true);
        }

        private void DisableModal() {
            PlayerInputSystem.AllInputBlocked = false;
            Game.RemoveCursorUnlock("Modal");
            _active = false;
            _del = null;
            _simpleYes = null;
            for (int i = 0; i < _buttons.Length; i++) {
                _buttons[i].gameObject.SetActive(false);
            }
            _canvasGroup.SetActive(false);
        }

    }
}