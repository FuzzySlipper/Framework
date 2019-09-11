using UnityEngine;
using System.Collections;
using TMPro;
namespace PixelComrades {
    public class UICurrency : MonoBehaviour {

        [SerializeField] private TextMeshProUGUI _currency = null;
        private float _lastValue = 0;

        void Awake() {
            MessageKit.addObserver(Messages.PlayerNewGame, StartNewGame);
            MessageKit.addObserver(Messages.GameStarted, GameLoaded);
        }

        private void GameLoaded() {
            Player.DefaultCurrencyHolder.OnResourceChanged += ChangeText;
        }

        private void StartNewGame() {
            _lastValue = 0;
        }

        private void ChangeText() {
            var difference = Player.DefaultCurrencyHolder.Value - _lastValue;
            if (difference > 0) {
                UIFloatingText.Spawn(string.Format("+{0}", difference.ToString("F0")), 2.5f, transform as RectTransform, Color.green);
            }
            else if (difference < 0) {
                UIFloatingText.Spawn(string.Format("-{0}", difference.ToString("F0")), 2.5f, transform as RectTransform, Color.red);
            }
            _lastValue = Player.DefaultCurrencyHolder.Value;
            _currency.text = _lastValue.ToString("F0");
        }
    }
}