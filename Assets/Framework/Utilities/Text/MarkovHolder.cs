using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class MarkovHolder : ScriptableObject {
        [SerializeField] private string _label = "";
        [SerializeField] private TextAsset _textAsset = null;
        [SerializeField] private char[] _splitWords = new char[] { '\n' };
        [SerializeField] private int _order = 3;
        [SerializeField] private int _minLength = 5;

        private MarkovNameGenerator _markovGenerator;

        public string Label { get { return _label; } }

        public string GetName() {
            if (_markovGenerator == null) {
                var textArray = _textAsset.text.Split(_splitWords);
                _markovGenerator = new MarkovNameGenerator(textArray, _order, _minLength);
            }
            return _markovGenerator.NextName();
        }

        public void Reset() {
            var textArray = _textAsset.text.Split(_splitWords);
            _markovGenerator = new MarkovNameGenerator(textArray, _order, _minLength);
        }
    }
}
