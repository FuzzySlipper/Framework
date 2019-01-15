using TMPro;
using UnityEngine;

namespace PixelComrades {
    public class UIFrameCounter : MonoSingleton<UIFrameCounter> {

        [System.Serializable] private struct FPSColor {
            public Color Color;
            public int MinFPS;

            public FPSColor(Color color, int minFps) {
                Color = color;
                MinFPS = minFps;
            }
        }

        [SerializeField] private TextMeshProUGUI _frameLabel = null;
        [SerializeField] private TextMeshProUGUI _highLabel = null;
        [SerializeField] private TextMeshProUGUI _lowLabel = null;
        [SerializeField] private int _frameRange = 60;
        [SerializeField] private FPSColor[] _frameColors = new FPSColor[5] {
            new FPSColor(Color.cyan, 60), new FPSColor(Color.green,45), new FPSColor(Color.yellow,30),
                    new FPSColor(new Color(255,191,0,1), 15), new FPSColor(Color.red,15)
        };

        private UnscaledTimer _updateRate = new UnscaledTimer(0.25f);
        private int[] _fpsBuffer;
        private int _fpsBufferIndex;
        private int _averageFps;
        private int _highestFps;
        private int _lowestFps;

        private static string[] _stringsFrom00To99 = {
            "00", "01", "02", "03", "04", "05", "06", "07", "08", "09",
            "10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
            "20", "21", "22", "23", "24", "25", "26", "27", "28", "29",
            "30", "31", "32", "33", "34", "35", "36", "37", "38", "39",
            "40", "41", "42", "43", "44", "45", "46", "47", "48", "49",
            "50", "51", "52", "53", "54", "55", "56", "57", "58", "59",
            "60", "61", "62", "63", "64", "65", "66", "67", "68", "69",
            "70", "71", "72", "73", "74", "75", "76", "77", "78", "79",
            "80", "81", "82", "83", "84", "85", "86", "87", "88", "89",
            "90", "91", "92", "93", "94", "95", "96", "97", "98", "99"
        };

        void Awake() {
            main = this;
            _fpsBuffer = new int[_frameRange];
            _fpsBufferIndex = 0;
            gameObject.SetActive(false);
        }

        void Update() {
            _fpsBuffer[_fpsBufferIndex++] = (int) (1f / Time.unscaledDeltaTime);
            if (_fpsBufferIndex >= _frameRange) {
                _fpsBufferIndex = 0;
            }
            CalculateFPS();
            if (_updateRate.IsActive) {
                return;
            }
            _updateRate.StartTimer();
            Display(_frameLabel, _averageFps);
            Display(_highLabel, _highestFps);
            Display(_lowLabel, _lowestFps);
        }

        public void Toggle() {
            gameObject.SetActive(!gameObject.activeInHierarchy);
        }

        private void Display(TextMeshProUGUI label, int fps) {
            label.text = _stringsFrom00To99[Mathf.Clamp(fps, 0, 99)];
            for (int i = 0; i < _frameColors.Length; i++) {
                if (fps >= _frameColors[i].MinFPS) {
                    label.color = _frameColors[i].Color;
                    break;
                }
            }
        }

        private void CalculateFPS() {
            int sum = 0;
            int highest = 0;
            int lowest = int.MaxValue;
            for (int i = 0; i < _frameRange; i++) {
                int fps = _fpsBuffer[i];
                sum += fps;
                if (fps > highest) {
                    highest = fps;
                }
                if (fps < lowest) {
                    lowest = fps;
                }
            }
            _averageFps = sum / _frameRange;
            _highestFps = highest;
            _lowestFps = lowest;
        }

    }
}