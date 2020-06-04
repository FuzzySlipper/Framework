using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace PixelComrades {
    public class TestPerformance : MonoBehaviour {

        [SerializeField] private float _updateInterval = 0.5F;
        [SerializeField] private GameObject[] _testObjects = new GameObject[0];
        [SerializeField] private float _testLength = 5;

        private double _lastInterval;
        private int _frames = 0;
        private float _fps;
        private List<float> _testRecords = new List<float>();
        private float _startTest;
        private bool _recordingTest = false;
        private string _label;
        //private float[] _records;
        //private bool _completeTest;

        void Start() {
            _lastInterval = Time.realtimeSinceStartup;
            _frames = 0;
        }

        void OnGUI() {
            GUILayout.Label("" + _fps.ToString("f2"));
        }

        void Update() {
            if (!_recordingTest) {
                for (int i = 0; i < _testObjects.Length+1; i++) {
                    if (Keyboard.current[PlayerControls.NumericKeys[i]].wasPressedThisFrame) {
                        var index = i - 1;
                        if (_testObjects.HasIndex(index)) {
                            StartTest(index);
                            break;
                        }
                    }
                }
            }
            ++_frames;
            float timeNow = Time.realtimeSinceStartup;
            if (timeNow > _lastInterval + _updateInterval) {
                _fps = (float) (_frames / (timeNow - _lastInterval));
                if (_recordingTest) {
                    _testRecords.Add(_fps);
                }
                _frames = 0;
                _lastInterval = timeNow;
            }
            if (_recordingTest && timeNow > _startTest + _testLength) {
                FinishTest();
            }
        }

        private void FinishTest() {
            _recordingTest = false;
            if (_testRecords.Count == 0) {
                Debug.Log("Empty test");
                return;
            }
            float cnt = 0;
            for (int i = 0; i < _testRecords.Count; i++) {
                cnt += _testRecords[i];
            }
            Debug.LogFormat("{0} Average FPS: {1:F2}", _label, cnt/_testRecords.Count);
        }

        private void StartTest(int index) {
            _label = _testObjects[index].name;
            for (int i = 0; i < _testObjects.Length; i++) {
                _testObjects[i].SetActive(false);
            }
            _testObjects[index].SetActive(true);
            var poolListeners = _testObjects[index].GetComponentsInChildren<IPoolEvents>();
            for (int i = 0; i < poolListeners.Length; i++) {
                poolListeners[i].OnPoolSpawned();
            }
            _testRecords.Clear();
            _startTest = Time.realtimeSinceStartup;
            _recordingTest = true;
        }
    }
}
